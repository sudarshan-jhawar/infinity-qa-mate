using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QATrainer.Models;
using SQLitePCL;
using System.Text;
using System.Text.Json;
namespace QATrainer.Controllers;

[ApiController]
[Route("[controller]")]
public class DefectsController : ControllerBase
{
    private readonly InfinityQaContext _context;

    public DefectsController(InfinityQaContext context)
    {
        _context = context;
    }

    // GET: /Defects
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Defect>>> GetAll()
    {
        var defects = await _context.Defects.ToListAsync();
        return Ok(defects);
    }


    static readonly HttpClient client = new HttpClient();
    static readonly string OLLAMA_URL = "http://localhost:11434/api/embeddings";

    [HttpGet("embed")]    
    public async Task<ActionResult> EmbedAll()
    {
        Console.WriteLine("Computing embeddings using Ollama (nomic-embed-text)...");

        foreach(var defect in _context.Defects)
        {
            var texts = $"{defect.Title} [SEP] {defect.Description}";
            if(defect.Embedding?.Length > 0)
                continue; // Skip if embedding already exists
            var embeddings = await GetEmbeddingsFromOllama(texts);
            var blob = new byte[embeddings.Length * 4];
            Buffer.BlockCopy(embeddings, 0, blob, 0, blob.Length);
            defect.Embedding = blob;
            _context.SaveChanges();
        }

       
        Console.WriteLine("All embeddings saved!");
        return Ok(new { message = "All embeddings saved!" });
    }
    private static async Task<float[]> GetEmbeddingsFromOllama(string texts)
    {
        var request = new
        {
            model = "nomic-embed-text",
            prompt = string.Join("\n", texts)
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(OLLAMA_URL, content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(jsonResponse);

       
            var embeddings = result!.embedding.ToArray();
        

        return embeddings;
    }
}

class OllamaEmbeddingResponse
{
    public List<float> embedding { get; set; } = new();
}


