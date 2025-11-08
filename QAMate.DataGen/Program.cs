using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using QAMate.Data;

// Data generation utility adapted to updated Defect entity (tracking fields).
// Defect fields: Id, Title, Description, Status, Severity, Priority, CreatedAt, UpdatedAt, LastModifiedAt.

static void PrintUsage()
{
    Console.WriteLine("QAMate.DataGen - defect data generator (tracking model)");
    Console.WriteLine("Usage examples:");
    Console.WriteLine("  dotnet run --project QAMate.DataGen -- --count 1000 --seed 42 --tag GEN --cleanup");
    Console.WriteLine("  dotnet run --project QAMate.DataGen -- --interactive");
    Console.WriteLine("Options:");
    Console.WriteLine("  --interactive     Prompt for all configuration values.");
    Console.WriteLine("  --count           Number of defects to generate (default 100000)");
    Console.WriteLine("  --from            Start date UTC yyyy-MM-dd (default now-90d)");
    Console.WriteLine("  --to              End date UTC yyyy-MM-dd (default now)");
    Console.WriteLine("  --seed            Random seed (default 12345)");
    Console.WriteLine("  --tag             Tag prefix in Title to identify generated rows (default GEN)");
    Console.WriteLine("  --run             RunId for this execution (default timestamp)");
    Console.WriteLine("  --batch-size      Insert batch size (default 1000)");
    Console.WriteLine("  --parallelism     Generation parallelism (default CPU count)");
    Console.WriteLine("  --dry-run         Skip DB writes (default false)");
    Console.WriteLine("  --cleanup         Delete existing rows whose Title starts with [tag] before inserting");
    Console.WriteLine("  --export          Export file path (optional)");
    Console.WriteLine("  --export-format   json|csv (default json if --export given)");
    Console.WriteLine("  --db              SQLite connection string (default QAMate/app.db)");
    Console.WriteLine("  --use-openai      Generate title/description via OpenAI (env OPENAI_API_KEY)");
    Console.WriteLine("  --openai-max      Max AI calls (default 100 unless count<=200)" );
    Console.WriteLine("  --wait            Wait for Enter before exit");
    Console.WriteLine("  --help            Show this help text");
}

// Argument parsing helpers --------------------------------------------------
static Dictionary<string,string> ParseArgs(string[] args)
{
    var d = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
    for (int i=0;i<args.Length;i++)
    {
        if (!args[i].StartsWith("--")) continue;
        var key = args[i][2..];
        if (i+1 < args.Length && !args[i+1].StartsWith("--")) d[key]=args[++i]; else d[key]="true";
    }
    return d;
}
static void WaitIfNeeded(bool wait){ if(wait){ Console.WriteLine("Press Enter to exit..."); Console.ReadLine(); }}
static string Ask(string label,string? def=null){ Console.Write(def==null? label+": ":$"{label} [{def}]: "); var v=Console.ReadLine(); return string.IsNullOrWhiteSpace(v)? def??string.Empty : v!.Trim(); }
static int AskInt(string label,int def){ while(true){ var s=Ask(label,def.ToString()); if(int.TryParse(s,out var v)&&v>=0) return v; Console.WriteLine("Enter non-negative integer."); }}
static bool AskBool(string label,bool def){ while(true){ var s=Ask(label+" (y/n)", def?"y":"n").ToLowerInvariant(); if(s is "y" or "yes") return true; if(s is "n" or "no") return false; Console.WriteLine("Enter y or n."); }}
static DateTime AskDate(string label,DateTime def){ while(true){ var s=Ask(label+" (yyyy-MM-dd)", def.ToString("yyyy-MM-dd")); if(DateTime.TryParse(s,out var dt)) return DateTime.SpecifyKind(dt,DateTimeKind.Utc); Console.WriteLine("Invalid date."); }}

// New: normalize any relative SQLite path to QAMate/app.db explicitly
static string NormalizeSqlitePath(string conn, string qamateDir)
{
    if (string.IsNullOrWhiteSpace(conn)) return conn;
    // Only handle sqlite style strings
    // Split into segments; rebuild after potential change
    var parts = conn.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    bool changed = false;
    for (int i = 0; i < parts.Count; i++)
    {
        var kv = parts[i].Split('=', 2);
        if (kv.Length != 2) continue;
        var key = kv[0].Trim().ToLowerInvariant();
        if (key is "data source" or "datasource" or "filename")
        {
            var val = kv[1].Trim().Trim('\"', '\'');
            if (val.Equals(":memory:", StringComparison.OrdinalIgnoreCase)) return conn; // in-memory
            if (!Path.IsPathFullyQualified(val))
            {
                // If the relative path does not already start from QAMate, pin it under QAMate directory.
                var abs = Path.GetFullPath(Path.Combine(qamateDir, val));
                parts[i] = $"Data Source={abs}";
                changed = true;
            }
        }
    }
    if (!changed && !parts.Any(p => p.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase)))
    {
        // No explicit Data Source found; enforce QAMate/app.db
        var abs = Path.GetFullPath(Path.Combine(qamateDir, "app.db"));
        parts.Add($"Data Source={abs}");
        changed = true;
    }
    return changed ? string.Join(';', parts) : conn;
}

// Resolve app.db default ----------------------------------------------------
var argsMap = ParseArgs(args);
if(argsMap.ContainsKey("help")){ PrintUsage(); return 0; }
bool interactive = argsMap.ContainsKey("interactive") || args.Length==0;
int count = 100000; int seed=12345; string tag="GEN"; string runId=DateTime.UtcNow.ToString("yyyyMMddHHmmss");
bool dryRun=false; bool cleanup=false; int batchSize=1000; int parallelism=Environment.ProcessorCount;
bool useOpenAi=false; int openAiMax=100; bool waitAtEnd=false;
DateTime toUtc=DateTime.UtcNow; DateTime fromUtc=toUtc.AddDays(-90);
string? dbConnArg=argsMap.GetValueOrDefault("db");
var solutionDir=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..",".."));
var qamateDir=Path.Combine(solutionDir,"QAMate");
string connectionString="";
string? defaultConn=null;
var appsettingsPath=Path.Combine(qamateDir,"appsettings.json");
if(File.Exists(appsettingsPath))
{
    try{ var json=await File.ReadAllTextAsync(appsettingsPath); using var doc=JsonDocument.Parse(json); defaultConn=doc.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString(); }catch{}
}
string AbsConn(string cs){ if(string.IsNullOrWhiteSpace(cs)) return cs; if(!cs.Contains("Data Source=")) return cs; var part=cs.Split('=')[1].Trim(); if(Path.IsPathFullyQualified(part)|| part==":memory:") return cs; var abs=Path.GetFullPath(Path.Combine(qamateDir,part)); return $"Data Source={abs}"; }

if(interactive)
{
    Console.WriteLine("[INTERACTIVE MODE]");
    connectionString = AbsConn(Ask("SQLite connection string or file path", defaultConn ?? "Data Source=app.db"));
    count=AskInt("Defect count",count); seed=AskInt("Seed",seed); tag=Ask("Tag",tag); runId=Ask("RunId",runId);
    fromUtc=AskDate("Start date UTC",fromUtc); toUtc=AskDate("End date UTC",toUtc);
    batchSize=AskInt("Batch size",batchSize); parallelism=AskInt("Parallelism",parallelism);
    dryRun=AskBool("Dry run",dryRun); cleanup=AskBool("Cleanup existing tag",cleanup);
    useOpenAi=AskBool("Use OpenAI",useOpenAi); if(useOpenAi) openAiMax=AskInt("Max AI calls", openAiMax);
    var exportPath=Ask("Export path (blank skip)","" ); if(!string.IsNullOrWhiteSpace(exportPath)){ argsMap["export"]=exportPath; argsMap["export-format"]=Ask("Export format (json/csv)","json"); }
    waitAtEnd=AskBool("Wait before exit", true);
}
else
{
    count=int.TryParse(argsMap.GetValueOrDefault("count"),out var tmp)? tmp:count;
    seed=int.TryParse(argsMap.GetValueOrDefault("seed"),out tmp)? tmp:seed;
    tag=argsMap.GetValueOrDefault("tag") ?? tag; runId=argsMap.GetValueOrDefault("run") ?? runId;
    dryRun= bool.TryParse(argsMap.GetValueOrDefault("dry-run"),out var b)&&b; cleanup= bool.TryParse(argsMap.GetValueOrDefault("cleanup"),out b)&&b;
    batchSize=int.TryParse(argsMap.GetValueOrDefault("batch-size"),out tmp)? tmp:batchSize;
    parallelism=int.TryParse(argsMap.GetValueOrDefault("parallelism"),out tmp)? tmp:parallelism;
    useOpenAi= bool.TryParse(argsMap.GetValueOrDefault("use-openai"),out b)&&b;
    openAiMax=int.TryParse(argsMap.GetValueOrDefault("openai-max"),out tmp)? tmp : (count<=200? count : openAiMax);
    toUtc=DateTime.TryParse(argsMap.GetValueOrDefault("to"),out var dtTo)? DateTime.SpecifyKind(dtTo,DateTimeKind.Utc): toUtc;
    fromUtc=DateTime.TryParse(argsMap.GetValueOrDefault("from"),out var dtFrom)? DateTime.SpecifyKind(dtFrom,DateTimeKind.Utc): fromUtc;
    waitAtEnd=argsMap.ContainsKey("wait");
    if(!string.IsNullOrWhiteSpace(dbConnArg)) connectionString=AbsConn(dbConnArg.Contains('=')? dbConnArg: $"Data Source={dbConnArg}");
    else connectionString=AbsConn(defaultConn ?? "Data Source=app.db");
}

// Ensure final connection string points to solution's QAMate/app.db if relative provided
connectionString = NormalizeSqlitePath(connectionString, qamateDir);

Console.WriteLine($"[INFO] Using database connection: {connectionString}");
var dbFile = connectionString.Split('=')[1].Trim(); if(dbFile != ":memory:") Console.WriteLine($"[INFO] SQLite path: {dbFile} exists={File.Exists(dbFile)}");

var cfg = new GenConfig(count, tag, runId, seed, fromUtc, toUtc, dryRun, batchSize, parallelism, cleanup, argsMap.GetValueOrDefault("export") ?? "", argsMap.GetValueOrDefault("export-format") ?? "", useOpenAi, openAiMax);

var genWatch = Stopwatch.StartNew();
var defects = await Generator.GenerateAsync(cfg);
Console.WriteLine($"[INFO] Generated {defects.Count} defects in {genWatch.Elapsed}.");

if(!string.IsNullOrEmpty(cfg.ExportPath))
{
    await Util.ExportAsync(defects, cfg.ExportPath, string.IsNullOrEmpty(cfg.ExportFormat)? "json" : cfg.ExportFormat);
    Console.WriteLine($"[INFO] Exported to {cfg.ExportPath}.");
}

var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connectionString).Options;
await using var db = new AppDbContext(options);
await db.Database.EnsureCreatedAsync();
var before = await db.Defect.LongCountAsync();
Console.WriteLine($"[INFO] Rows before insert: {before}");

if(dryRun)
{
    Console.WriteLine("[INFO] Dry-run: skipping persistence.");
    Util.PrintSample(defects);
    WaitIfNeeded(waitAtEnd); return 0;
}

if(cleanup)
{
    Console.WriteLine($"[INFO] Cleanup rows with Title starting '[{tag}]'...");
    var old = await db.Defect.Where(d=> d.Title != null && d.Title.StartsWith($"[{tag}]" )).ToListAsync();
    db.Defect.RemoveRange(old); var del = await db.SaveChangesAsync();
    Console.WriteLine($"[INFO] Deleted {del} rows.");
}

int written=0; var insertWatch=Stopwatch.StartNew();
foreach(var batch in Util.Batch(defects, cfg.BatchSize))
{
    db.Defect.AddRange(batch.Select(x => new Defect {
        Title = x.Title,
        Description = x.Description,
        Status = x.Status,
        Severity = x.Severity,
        Priority = x.Priority,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
        LastModifiedAt = x.LastModifiedAt
    }));
    await db.SaveChangesAsync(); written += batch.Count;
    if(written % (cfg.BatchSize*5) ==0 || written==defects.Count) Console.WriteLine($"[PROGRESS] {written}/{defects.Count} ({written*100/defects.Count}%)");
}
var after = await db.Defect.LongCountAsync();
Console.WriteLine($"[INFO] Inserted {written} defects in {insertWatch.Elapsed}.");
Console.WriteLine($"[INFO] Total rows after insert: {after}");
Util.PrintSample(defects);
WaitIfNeeded(waitAtEnd);
return 0;

// Model records -----------------------------------------------------------------
record GenConfig(int Count,string Tag,string RunId,int Seed,DateTime FromUtc,DateTime ToUtc,bool DryRun,int BatchSize,int Parallelism,bool CleanupFirst,string ExportPath,string ExportFormat,bool UseOpenAI,int OpenAIMax);

class GenItem
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public required int Severity { get; init; }
    public required int Priority { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required DateTime LastModifiedAt { get; init; }
}

interface IContentGenerator
{
    Task<(string title,string description)> GenerateAsync(string category,string component,Random r,CancellationToken ct);
}

class TemplateContentGenerator : IContentGenerator
{
    private static readonly string[] Ui = { "button overlaps text","layout shift on resize","modal not closing","icon misalignment" };
    private static readonly string[] Api = { "500 on POST booking","422 validation failure","incorrect fare in response","auth token rejected" };
    private static readonly string[] Perf = { "slow driver search (>3s)","N+1 queries on history","high CPU during tracking","memory spike after payment" };
    private static readonly string[] Sec = { "potential XSS in notes","IDOR in trip history","weak password policy","insecure redirect after login" };
    public Task<(string title,string description)> GenerateAsync(string category,string component,Random r,CancellationToken ct)
    {
        string detail = category switch{ "UI"=>Ui[r.Next(Ui.Length)],"API"=>Api[r.Next(Api.Length)],"Performance"=>Perf[r.Next(Perf.Length)],_=>Sec[r.Next(Sec.Length)]};
        var title = $"{category}: {component} {detail}";
        var desc = $"In {component}, a {category} issue occurs: {detail}. Expected normal operation without this defect.";
        return Task.FromResult((title,desc));
    }
}

class OpenAiContentGenerator : IContentGenerator
{
    private readonly HttpClient _http; private readonly string _apiKey; private readonly string _model; private readonly SemaphoreSlim _gate=new(1,1);
    public OpenAiContentGenerator(string apiKey,string model="gpt-4o-mini"){ _apiKey=apiKey; _model=model; _http=new HttpClient{ BaseAddress=new Uri("https://api.openai.com/v1/")}; _http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",_apiKey); }
    public async Task<(string title,string description)> GenerateAsync(string category,string component,Random r,CancellationToken ct)
    {
        var prompt = $@"Generate a concise software defect title (<=120 chars) and two-sentence description for a cab booking web app. Category={category}. Component={component}. JSON only: {{""title"":...,""description"":...}}";
        var payload = JsonSerializer.Serialize(new{ model=_model, messages=new[]{ new { role="user", content=prompt } }, temperature=0.7, max_tokens=200 });
        using var content = new StringContent(payload,Encoding.UTF8,"application/json");
        await _gate.WaitAsync(ct); try{ using var resp = await _http.PostAsync("chat/completions",content,ct); var json=await resp.Content.ReadAsStringAsync(ct); resp.EnsureSuccessStatusCode(); using var doc=JsonDocument.Parse(json); var msg=doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()??""; try{ using var inner=JsonDocument.Parse(msg); var t=inner.RootElement.GetProperty("title").GetString(); var d=inner.RootElement.GetProperty("description").GetString(); return (t??"Untitled", d??"No description"); } catch { var lines=msg.Split('\n',StringSplitOptions.RemoveEmptyEntries); return (lines.FirstOrDefault()??"Untitled", string.Join(' ', lines.Skip(1))); } } finally { _gate.Release(); }
    }
}

static class Util
{
    public static async Task ExportAsync(IReadOnlyList<GenItem> items,string path,string fmt)
    {
        var full=Path.GetFullPath(path); Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        if(fmt.Equals("json",StringComparison.OrdinalIgnoreCase))
        {
            await File.WriteAllTextAsync(full,JsonSerializer.Serialize(items,new JsonSerializerOptions{ WriteIndented=true })); return;
        }
        if(fmt.Equals("csv",StringComparison.OrdinalIgnoreCase))
        {
            var sb=new StringBuilder(); sb.AppendLine("Title,Description,Status,Severity,Priority,CreatedAt,UpdatedAt,LastModifiedAt");
            foreach(var i in items){ string Esc(string v)=>"\""+v.Replace("\"","\"\"")+"\""; sb.AppendLine(string.Join(',',new[]{Esc(i.Title),Esc(i.Description),Esc(i.Status),i.Severity.ToString(),i.Priority.ToString(),i.CreatedAt.ToString("o"),i.UpdatedAt.ToString("o"),i.LastModifiedAt.ToString("o")})); }
            await File.WriteAllTextAsync(full,sb.ToString()); return;
        }
    }
    public static void PrintSample(IReadOnlyList<GenItem> items,int take=3){ Console.WriteLine("[SAMPLE]"); foreach(var d in items.Take(take)) Console.WriteLine($"- {d.Title} | {d.Status} Sev={d.Severity} Pri={d.Priority} Created={d.CreatedAt:yyyy-MM-dd}"); }
    public static IEnumerable<List<T>> Batch<T>(IReadOnlyList<T> items,int size){ var list=new List<T>(size); for(int i=0;i<items.Count;i++){ list.Add(items[i]); if(list.Count==size){ yield return list; list=new List<T>(size);} } if(list.Count>0) yield return list; }
}

static class Generator
{
    private static readonly string[] Components = { "Cab Booking","Driver Search","Source Selection","Destination Selection","Fare Estimation","Payment","Ride Tracking","Trip History","Auth & Profile" };
    private static readonly string[] Statuses = { "Open","InProgress","Resolved","Closed" };

    public static async Task<List<GenItem>> GenerateAsync(GenConfig cfg)
    {
        IContentGenerator template = new TemplateContentGenerator();
        IContentGenerator? openAiGen = null;
        if(cfg.UseOpenAI)
        {
            var key=Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if(string.IsNullOrWhiteSpace(key)) Console.WriteLine("[WARN] OPENAI_API_KEY not set; using template."); else { openAiGen=new OpenAiContentGenerator(key); Console.WriteLine("[INFO] OpenAI generator ready."); }
        }
        var rng = new Random(cfg.Seed);
        int aiCalls=0;
        var list = new List<GenItem>(cfg.Count);
        var indices = Enumerable.Range(0,cfg.Count).ToArray();
        await Parallel.ForEachAsync(indices,new ParallelOptions{ MaxDegreeOfParallelism=cfg.Parallelism }, async (i,ct)=>
        {
            var r = new Random(unchecked(cfg.Seed + i*7919));
            var category = PickCategory(r);
            var component = Components[r.Next(Components.Length)];
            (string title,string description) content;
            bool useAi = openAiGen!=null && aiCalls < cfg.OpenAIMax;
            if(useAi){ var current=Interlocked.Increment(ref aiCalls); if(current<=cfg.OpenAIMax){ try{ content=await openAiGen!.GenerateAsync(category,component,r,ct); } catch(Exception ex){ Console.WriteLine($"[WARN] AI failed: {ex.Message}"); content=await template.GenerateAsync(category,component,r,ct);} } else content=await template.GenerateAsync(category,component,r,ct); }
            else content=await template.GenerateAsync(category,component,r,ct);
            var titleTagged = $"[{cfg.Tag}][{cfg.RunId}] {content.title} #{i+1}";
            var created = RandomDate(r,cfg.FromUtc,cfg.ToUtc);
            var (status,severity,priority,updated,lastMod) = Lifecycle(r,created);
            var item = new GenItem { Title=titleTagged, Description=content.description, Status=status, Severity=severity, Priority=priority, CreatedAt=created, UpdatedAt=updated, LastModifiedAt=lastMod };
            lock(list) list.Add(item);
        });
        list.Sort((a,b)=> string.CompareOrdinal(a.Title,b.Title));
        Console.WriteLine($"[INFO] AI calls used: {aiCalls}");
        return list;
    }

    private static string PickCategory(Random r){ var x=r.NextDouble(); return x<0.55?"UI": x<0.8?"API": x<0.95?"Performance":"Security"; }
    private static DateTime RandomDate(Random r,DateTime from,DateTime to){ var span=to-from; var secs=r.NextInt64(0,(long)span.TotalSeconds); return from.AddSeconds(secs); }
    private static (string status,int severity,int priority,DateTime updated,DateTime lastMod) Lifecycle(Random r,DateTime created)
    {
        // Status distribution biased towards closed/resolved for historical realism.
        var x=r.NextDouble(); string status = x<0.25?"Open": x<0.45?"InProgress": x<0.80?"Resolved": "Closed";
        int severity = status=="Open"? r.Next(1,4) : r.Next(1,6); // 1-5
        int priority = severity<=2? 1 : severity==3? 2 : severity==4? 3 : 4; // simple mapping 1(high)..5(low)
        var updated = created.AddHours(r.Next(1,240));
        var lastMod = updated.AddHours(r.Next(0,48));
        return (status,severity,priority,updated,lastMod);
    }
}
