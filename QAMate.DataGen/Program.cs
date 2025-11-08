using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Concurrent; // added for Partitioner
using Microsoft.EntityFrameworkCore;
using QAMate.Data;

// Adjusted: use Name field to align with QAMate.Data.Defect entity (Id, Name, Price)

// Added AI content generation support: --use-openai and --openai-max
// Description is generated but not persisted (DB schema lacks column). Included in exports.

static void PrintUsage()
{
    Console.WriteLine("QAMate.DataGen - defect data generator");
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project QAMate.DataGen -- --count 1000 --seed 42 --tag GEN --use-openai --openai-max 50");
    Console.WriteLine("Options:");
    Console.WriteLine("  --interactive     Ask for all configs via prompts (default when no args)");
    Console.WriteLine("  --count          Number of defects to generate (default 100000)");
    Console.WriteLine("  --from           Start date (UTC) yyyy-MM-dd (default now-90d)");
    Console.WriteLine("  --to             End date (UTC) yyyy-MM-dd (default now)");
    Console.WriteLine("  --seed           Integer seed for determinism (default 12345)");
    Console.WriteLine("  --tag            Tag to mark generated items (default GEN)");
    Console.WriteLine("  --run            RunId to group this run (default timestamp)");
    Console.WriteLine("  --batch-size     DB insert batch size (default 1000)");
    Console.WriteLine("  --parallelism    Parallelism for generation (default Environment.ProcessorCount)");
    Console.WriteLine("  --dry-run        Preview only, no DB writes (default false)");
    Console.WriteLine("  --cleanup        Delete prior items with tag before generating (default false)");
    Console.WriteLine("  --export         Export file path (optional).");
    Console.WriteLine("  --export-format  json|csv (default json if --export given)");
    Console.WriteLine("  --db             Optional SQLite connection string. If omitted, uses QAMate appsettings.json or local.db.");
    Console.WriteLine("  --use-openai     Use OpenAI (env OPENAI_API_KEY) for name+description (default false)");
    Console.WriteLine("  --openai-max     Max AI calls (default = count if small, else 100) - remainder use templates");
    Console.WriteLine("  --wait           Wait for Enter before exit (useful when debugging)");
    Console.WriteLine("  --help           Show this help text");
}

static string? ResolveSqliteFilePath(string connectionString)
{
    try
    {
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var p in parts)
        {
            var kv = p.Split('=', 2);
            if (kv.Length != 2) continue;
            var key = kv[0].Trim().ToLowerInvariant();
            var val = kv[1].Trim().Trim('\'','"');
            if (key is "data source" or "datasource" or "filename")
            {
                if (string.Equals(val, ":memory:", StringComparison.OrdinalIgnoreCase)) return ":memory:";
                return Path.GetFullPath(val);
            }
        }
    }
    catch { }
    return null;
}

static Dictionary<string, string> ParseArgs(string[] args)
{
    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (int i = 0; i < args.Length; i++)
    {
        var a = args[i];
        if (a.StartsWith("--"))
        {
            var key = a[2..];
            if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
            {
                dict[key] = args[++i];
            }
            else
            {
                dict[key] = "true";
            }
        }
    }
    return dict;
}

static void WaitIfNeeded(bool wait)
{
    if (wait)
    {
        Console.WriteLine();
        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
}

static string Ask(string label, string? def = null)
{
    Console.Write(def == null ? $"{label}: " : $"{label} [{def}]: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) return def ?? string.Empty;
    return input!.Trim();
}

static int AskInt(string label, int def)
{
    while (true)
    {
        var s = Ask(label, def.ToString());
        if (int.TryParse(s, out var v) && v >= 0) return v;
        Console.WriteLine("Please enter a valid non-negative integer.");
    }
}

static bool AskBool(string label, bool def)
{
    while (true)
    {
        var s = Ask(label + " (y/n)", def ? "y" : "n").ToLowerInvariant();
        if (s is "y" or "yes") return true;
        if (s is "n" or "no") return false;
        Console.WriteLine("Please enter y or n.");
    }
}

static DateTime AskDate(string label, DateTime def)
{
    while (true)
    {
        var s = Ask(label + " (yyyy-MM-dd)", def.ToString("yyyy-MM-dd"));
        if (DateTime.TryParse(s, out var v)) return DateTime.SpecifyKind(v, DateTimeKind.Utc);
        Console.WriteLine("Please enter a valid date.");
    }
}

var argsMap = ParseArgs(args);
bool interactive = argsMap.ContainsKey("interactive") || args.Length == 0;
if (argsMap.ContainsKey("help")) { PrintUsage(); return 0; }

// Defaults
int count = 100000;
int seed = 12345;
string tag = "GEN";
string runId = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
bool dryRun = false;
bool cleanup = false;
int batchSize = 1000;
int parallelism = Environment.ProcessorCount;
bool useOpenAi = false;
int openAiMax = (count <= 200 ? count : 100);
bool waitAtEnd = false;

DateTime toUtc = DateTime.UtcNow;
DateTime fromUtc = toUtc.AddDays(-90);

string? dbConnArg = argsMap.GetValueOrDefault("db");
string connectionString = string.Empty;

// Try load default connection string from appsettings.json
string? defaultConn = null;
try
{
    var appsettingsPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "QAMate", "appsettings.json");
    if (File.Exists(appsettingsPath))
    {
        var json = await File.ReadAllTextAsync(appsettingsPath);
        using var doc = JsonDocument.Parse(json);
        defaultConn = doc.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString();
    }
}
catch { /* ignore */ }

if (interactive)
{
    Console.WriteLine("[INTERACTIVE MODE]");
    var dbPrompt = Ask("SQLite connection string or file path", defaultConn ?? "Data Source=local.db");
    connectionString = dbPrompt.Contains('=') ? dbPrompt : $"Data Source={dbPrompt}";
    count = AskInt("How many defects to generate", count);
    seed = AskInt("Random seed", seed);
    tag = Ask("Tag to mark generated items", tag);
    runId = Ask("RunId for this run", runId);
    fromUtc = AskDate("Start date (UTC)", fromUtc);
    toUtc = AskDate("End date (UTC)", toUtc);
    batchSize = AskInt("DB insert batch size", batchSize);
    parallelism = AskInt("Parallelism (threads)", parallelism);
    dryRun = AskBool("Dry run (no DB writes)", dryRun);
    cleanup = AskBool("Cleanup existing rows with this tag before insert", cleanup);
    useOpenAi = AskBool("Use OpenAI for titles/descriptions", useOpenAi);
    if (useOpenAi)
    {
        openAiMax = AskInt("Max AI calls (rest use templates)", openAiMax);
    }
    var exportPath = Ask("Export file path (blank to skip)", "");
    var exportFormat = string.Empty;
    if (!string.IsNullOrWhiteSpace(exportPath))
    {
        exportFormat = Ask("Export format (json/csv)", "json");
        argsMap["export"] = exportPath;
        argsMap["export-format"] = exportFormat;
    }
    waitAtEnd = AskBool("Wait for Enter before exit", true);
}
else
{
    // Parse from args
    count = int.TryParse(argsMap.GetValueOrDefault("count"), out var c) ? c : count;
    seed = int.TryParse(argsMap.GetValueOrDefault("seed"), out var s) ? s : seed;
    tag = argsMap.GetValueOrDefault("tag") ?? tag;
    runId = argsMap.GetValueOrDefault("run") ?? runId;
    dryRun = bool.TryParse(argsMap.GetValueOrDefault("dry-run"), out var dr) && dr;
    cleanup = bool.TryParse(argsMap.GetValueOrDefault("cleanup"), out var cl) && cl;
    batchSize = int.TryParse(argsMap.GetValueOrDefault("batch-size"), out var bs) ? bs : batchSize;
    parallelism = int.TryParse(argsMap.GetValueOrDefault("parallelism"), out var pl) ? pl : parallelism;
    useOpenAi = bool.TryParse(argsMap.GetValueOrDefault("use-openai"), out var uo) && uo;
    openAiMax = int.TryParse(argsMap.GetValueOrDefault("openai-max"), out var om) ? om : (count <= 200 ? count : openAiMax);
    toUtc = DateTime.TryParse(argsMap.GetValueOrDefault("to"), out var to) ? DateTime.SpecifyKind(to, DateTimeKind.Utc) : toUtc;
    fromUtc = DateTime.TryParse(argsMap.GetValueOrDefault("from"), out var from) ? DateTime.SpecifyKind(from, DateTimeKind.Utc) : fromUtc;
    waitAtEnd = argsMap.ContainsKey("wait");

    if (!string.IsNullOrWhiteSpace(dbConnArg))
    {
        connectionString = dbConnArg.Contains('=') ? dbConnArg : $"Data Source={dbConnArg}";
    }
    else
    {
        if (!string.IsNullOrWhiteSpace(defaultConn))
            connectionString = defaultConn!;
        else
        {
            // Fallback to a local file if nothing is configured
            connectionString = "Data Source=local.db";
            Console.WriteLine("[INFO] No connection provided; using local SQLite file 'local.db'.");
        }
    }
}

Console.WriteLine($"[INFO] Starting DataGen runId={runId} tag={tag} count={count} seed={seed} dryRun={dryRun} useOpenAI={useOpenAi} openAiMax={openAiMax}");
Console.WriteLine($"[INFO] Date range UTC: {fromUtc:o} -> {toUtc:o}");
Console.WriteLine($"[INFO] DB: {connectionString}");
var resolvedDbPath = ResolveSqliteFilePath(connectionString);
if (!string.IsNullOrEmpty(resolvedDbPath) && resolvedDbPath != ":memory:")
{
    Console.WriteLine($"[INFO] SQLite file path: {resolvedDbPath} (exists={File.Exists(resolvedDbPath)})");
}

var cfg = new GenConfig(
    Count: count,
    Tag: tag,
    RunId: runId,
    Seed: seed,
    FromUtc: fromUtc,
    ToUtc: toUtc,
    DryRun: dryRun,
    BatchSize: Math.Max(1, batchSize),
    Parallelism: Math.Max(1, parallelism),
    CleanupFirst: cleanup,
    ExportPath: argsMap.GetValueOrDefault("export") ?? string.Empty,
    ExportFormat: argsMap.GetValueOrDefault("export-format") ?? string.Empty,
    UseOpenAI: useOpenAi,
    OpenAIMax: openAiMax
);

var sw = Stopwatch.StartNew();
var defects = await Generator.GenerateAsync(cfg);
Console.WriteLine($"[INFO] Generated {defects.Count} rows in {sw.Elapsed}.");

if (!string.IsNullOrEmpty(cfg.ExportPath))
{
    await Util.ExportAsync(defects, cfg.ExportPath, string.IsNullOrEmpty(cfg.ExportFormat) ? "json" : cfg.ExportFormat);
    Console.WriteLine($"[INFO] Exported {defects.Count} to {cfg.ExportPath}.");
}

// Initialize DB and show counts even in dry-run mode
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connectionString).Options;
await using var db = new AppDbContext(options);
await db.Database.EnsureCreatedAsync();
var totalBefore = await db.Defect.LongCountAsync();
Console.WriteLine($"[INFO] Current total defects in DB: {totalBefore}");

if (dryRun)
{
    Console.WriteLine("[INFO] Dry-run mode: skipping DB writes.");
    Util.PrintSample(defects);
    WaitIfNeeded(waitAtEnd);
    return 0;
}

if (cleanup)
{
    Console.WriteLine($"[INFO] Cleaning up existing defects with Tag prefix in Name '[{tag}]'...");
    var toDelete = await db.Defect.Where(d => d.Name != null && d.Name.StartsWith($"[{tag}]"))
        .ToListAsync();
    var deleteCount = toDelete.Count;
    db.Defect.RemoveRange(toDelete);
    var deleted = await db.SaveChangesAsync();
    Console.WriteLine($"[INFO] Deleted {deleted} rows (requested {deleteCount}).");
}

// Batch insert
int written = 0;
foreach (var batch in Util.Batch(defects, cfg.BatchSize))
{
    db.Defect.AddRange(batch.Select(x => new Defect { Name = x.Name, Price = x.Price }));
    await db.SaveChangesAsync();
    written += batch.Count;
    if (written % (cfg.BatchSize * 5) == 0 || written == defects.Count)
        Console.WriteLine($"[PROGRESS] {written}/{defects.Count} ({written * 100 / defects.Count}%)");
}

var totalAfter = await db.Defect.LongCountAsync();
Console.WriteLine($"[INFO] Inserted {written} defects in {sw.Elapsed}.");
Console.WriteLine($"[INFO] Total defects in DB after insert: {totalAfter}");
WaitIfNeeded(waitAtEnd);
return 0;

// Types and helpers local to Program.cs for brevity

record GenConfig(int Count, string Tag, string RunId, int Seed, DateTime FromUtc, DateTime ToUtc, bool DryRun, int BatchSize, int Parallelism, bool CleanupFirst, string ExportPath, string ExportFormat, bool UseOpenAI, int OpenAIMax);

class GenItem
{
    public required string Name { get; init; } // aligns with Defect.Name
    public string? Description { get; init; } // not persisted (no column)
    public double? Price { get; init; }
}

interface IContentGenerator
{
    Task<(string name, string description)> GenerateAsync(string category, string component, Random r, CancellationToken ct);
}

class TemplateContentGenerator : IContentGenerator
{
    private static readonly string[] UiIssues = { "button overlapped", "text truncation", "layout shift", "modal not closing" };
    private static readonly string[] ApiIssues = { "500 on POST", "422 validation", "incorrect status code" };
    private static readonly string[] PerfIssues = { "slow response (>3s)", "N+1 query", "high CPU" };
    private static readonly string[] SecIssues = { "XSS risk", "IDOR", "weak password policy" };

    public Task<(string name, string description)> GenerateAsync(string category, string component, Random r, CancellationToken ct)
    {
        string detail = category switch
        {
            "UI" => UiIssues[r.Next(UiIssues.Length)],
            "API" => ApiIssues[r.Next(ApiIssues.Length)],
            "Performance" => PerfIssues[r.Next(PerfIssues.Length)],
            _ => SecIssues[r.Next(SecIssues.Length)]
        };
        var name = $"{category}: {component} {detail}";
        var description = $"In the {component} module a {category} issue occurs: {detail}. Expected correct behavior without this problem.";
        return Task.FromResult((name, description));
    }
}

class OpenAiContentGenerator : IContentGenerator
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly SemaphoreSlim _limiter = new(1,1); // simple throttle

    public OpenAiContentGenerator(string apiKey, string model = "gpt-35-turbo")
    {
        _apiKey = apiKey;
        _model = model;
        _http = new HttpClient
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<(string name, string description)> GenerateAsync(string category, string component, Random r, CancellationToken ct)
    {
        // Minimal chat completion call; retries omitted for brevity.
        var prompt = $"Generate a concise software defect title (max 120 chars) and a 2-3 sentence description for a cab booking web application. Category: {category}. Component: {component}. Output strict JSON: {{\\\"name\\\":\\\"...\\\", \\\"description\\\":\\\"...\\\"}}";
        var payload = JsonSerializer.Serialize(new
        {
            model = _model,
            messages = new[] { new { role = "user", content = prompt } },
            temperature = 0.7,
            max_tokens = 250
        });

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        await _limiter.WaitAsync(ct); // simplistic serialization to avoid rate limits
        try
        {
            using var resp = await _http.PostAsync("chat/completions", content, ct);
            var json = await resp.Content.ReadAsStringAsync(ct);
            resp.EnsureSuccessStatusCode();
            using var doc = JsonDocument.Parse(json);
            var message = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
            // Attempt to parse JSON object from content
            string? name = null; string? description = null;
            try
            {
                using var inner = JsonDocument.Parse(message);
                name = inner.RootElement.GetProperty("name").GetString();
                description = inner.RootElement.GetProperty("description").GetString();
            }
            catch
            {
                // Fallback: split lines
                var parts = message.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                name = parts.FirstOrDefault() ?? "Untitled Defect";
                description = string.Join(' ', parts.Skip(1));
            }
            return (name ?? "Untitled Defect", description ?? "No description");
        }
        finally
        {
            _limiter.Release();
        }
    }
}

static class Util
{
    public static async Task ExportAsync(IReadOnlyList<GenItem> items, string path, string fmt)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        if (fmt.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true }));
            return;
        }
        if (fmt.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            var sb = new StringBuilder();
            sb.AppendLine("Name,Description,Price");
            foreach (var d in items)
            {
                string Esc(string v) => "\"" + (v?.Replace("\"", "\"\"") ?? "") + "\"";
                sb.AppendLine(string.Join(',', new[] { Esc(d.Name), Esc(d.Description ?? ""), d.Price?.ToString() ?? "" }));
            }
            await File.WriteAllTextAsync(path, sb.ToString());
        }
    }

    public static void PrintSample(IReadOnlyList<GenItem> items, int take = 3)
    {
        Console.WriteLine("[SAMPLE]");
        foreach (var d in items.Take(take))
            Console.WriteLine($"- {d.Name}\n  {d.Description}\n  Price={d.Price}");
    }

    public static IEnumerable<List<T>> Batch<T>(IReadOnlyList<T> items, int size)
    {
        var batch = new List<T>(size);
        for (int i = 0; i < items.Count; i++)
        {
            batch.Add(items[i]);
            if (batch.Count == size)
            {
                yield return batch;
                batch = new List<T>(size);
            }
        }
        if (batch.Count > 0) yield return batch;
    }
}

static class Generator
{
    private static readonly string[] Components =
    {
        "Cab Booking", "Driver Search", "Source Selection", "Destination Selection",
        "Fare Estimation", "Payment", "Ride Tracking", "Trip History", "Auth & Profile"
    };

    public static async Task<List<GenItem>> GenerateAsync(GenConfig cfg)
    {
        IContentGenerator template = new TemplateContentGenerator();
        IContentGenerator? openai = null;
        if (cfg.UseOpenAI)
        {
            var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(key))
            {
                Console.WriteLine("[WARN] OPENAI_API_KEY not set; falling back to template generation.");
            }
            else
            {
                openai = new OpenAiContentGenerator(key);
                Console.WriteLine("[INFO] OpenAI content generator initialized.");
            }
        }

        var results = new List<GenItem>(cfg.Count);
        var indices = Enumerable.Range(0, cfg.Count).ToArray();
        int aiCalls = 0;
        await Parallel.ForEachAsync(
            indices,
            new ParallelOptions { MaxDegreeOfParallelism = cfg.Parallelism },
            async (i, ct) =>
            {
                var r = new Random(unchecked(cfg.Seed + i * 7919));
                var category = PickCategory(r);
                var component = Components[r.Next(Components.Length)];
                (string name, string description) content;
                bool useAiForThis = openai != null && aiCalls < cfg.OpenAIMax;
                if (useAiForThis)
                {
                    var current = Interlocked.Increment(ref aiCalls);
                    if (current <= cfg.OpenAIMax)
                    {
                        try
                        {
                            content = await openai!.GenerateAsync(category, component, r, ct);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WARN] OpenAI failed: {ex.Message}; using template fallback.");
                            content = await template.GenerateAsync(category, component, r, ct);
                        }
                    }
                    else
                    {
                        content = await template.GenerateAsync(category, component, r, ct);
                    }
                }
                else
                {
                    content = await template.GenerateAsync(category, component, r, ct);
                }

                var nameTagged = $"[{cfg.Tag}][{cfg.RunId}] {content.name} #{i + 1}";
                var price = Math.Round(r.NextDouble() * 100, 2);
                var item = new GenItem { Name = nameTagged, Description = content.description, Price = price };
                lock (results) results.Add(item);
            });

        results.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
        Console.WriteLine($"[INFO] AI calls made: {aiCalls}");
        return results;
    }

    private static string PickCategory(Random r)
    {
        var x = r.NextDouble();
        return x < 0.55 ? "UI" : x < 0.80 ? "API" : x < 0.95 ? "Performance" : "Security";
    }
}
