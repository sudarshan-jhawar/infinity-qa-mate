using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace DefectEmbedder
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static readonly string OLLAMA_URL = "http://localhost:11434/api/embeddings";

        static async Task Main()
        {
            Console.WriteLine("Computing embeddings using Ollama (nomic-embed-text)...");
            // ADD THIS LINE AT THE VERY TOP (before any DB code)
            Batteries.Init(); // This is the fix!
            using var conn = new SqliteConnection("Data Source=infinityQA.db");
            await conn.OpenAsync();

            // Ensure column exists
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "ALTER TABLE Defect ADD COLUMN Embedding BLOB";
            try { await cmd.ExecuteNonQueryAsync(); } catch { /* already exists */ }

            // Get defects without embedding
            cmd.CommandText = @"
                SELECT rowid, Title, Description 
                FROM Defect";
            using var reader = await cmd.ExecuteReaderAsync();

            var batch = new List<(long rowid, string text)>();
            const int BATCH_SIZE = 10;

            while (await reader.ReadAsync())
            {
                var rowid = reader.GetInt64(0);
                var title = reader.GetString(1);
                var desc = reader.GetString(2);
                var text = $"{title} [SEP] {desc}".Trim();

                batch.Add((rowid, text));

                if (batch.Count >= BATCH_SIZE)
                {
                    await ProcessBatch(conn, batch);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
                await ProcessBatch(conn, batch);

            Console.WriteLine("All embeddings saved!");
        }

        static async Task ProcessBatch(SqliteConnection conn, List<(long rowid, string text)> batch)
        {
            var request = new
            {
                model = "nomic-embed-text",
                prompt = string.Join("\n", batch.Select(b => b.text))
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(OLLAMA_URL, content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(jsonResponse);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Defect SET Embedding = ? WHERE rowid = ?";

            for (int i = 0; i < batch.Count; i++)
            {
                var embedding = result.embedding.Skip(i * 768).Take(768).ToArray();
                var blob = new byte[embedding.Length * 4];
                Buffer.BlockCopy(embedding, 0, blob, 0, blob.Length);

                cmd.Parameters.Clear();
                cmd.Parameters.Add(new SqliteParameter { Value = blob });
                cmd.Parameters.Add(new SqliteParameter { Value = batch[i].rowid });
                await cmd.ExecuteNonQueryAsync();
            }

            Console.WriteLine($"  Embedded {batch.Count} defect");
        }
    }

    class OllamaEmbeddingResponse
    {
        public List<float> embedding { get; set; } = new();
    }
}