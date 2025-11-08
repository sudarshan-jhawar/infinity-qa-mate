using Microsoft.AspNetCore.Mvc;
using QAMate.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class AzureOpenAIController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly string _deploymentName;

    public AzureOpenAIController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiKey = configuration["AzureOpenAI:ApiKey"];
        _deploymentName = configuration["AzureOpenAI:DeploymentName"];
        var resourceName = configuration["AzureOpenAI:ResourceName"];
        _endpoint = $"https://{resourceName}.openai.azure.com/openai/deployments/";
    }

    [HttpPost("refineDefectTitle")]
    public async Task<IActionResult> GetCompletion([FromBody] OpenAIRequest request)
    {
        var requestBody = new
        {
            messages = new[]
            {
                new { role = "user", content = $"Can you help refine the given defect title and return a single result in JSON format - Defect:' {request.input} '" }
            },
            max_tokens = 300,
            temperature = 0.7
        };


        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/dep-gpt-4.1/chat/completions?api-version=2023-05-15")
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);

        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();


        return Ok(new OpenAIResponse { ResponseText = content?.Trim() });
    }

    [HttpPost("getDescription")]
    public async Task<IActionResult> GetDescription([FromBody] OpenAIRequest request)
    {

        var requestBody = new
        {
            messages = new[]
            {
              //  new { role = "user", content = $"Can you help to write a full description for given defect title and return response in json -  {request.input}" }
                 new { role = "user", content = $"Given the defect title: {request.input}, please generate a detailed defect description including clear and concise explanation of the defect. Don't ask in the last for any additional reccomodation " }

            },
            max_tokens = 300,
            temperature = 0.7
        };


        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/dep-gpt-4.1/chat/completions?api-version=2023-05-15")
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);

        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();


        return Ok(new OpenAIResponse { ResponseText = content?.Trim() });
    }

    [HttpPost("embedding")]
    public async Task<IActionResult> GetEmbedding([FromBody] OpenAIRequest request)
    {
        var requestBody = new
        {
            input = request.input
            //max_tokens = request.MaxTokens,
            //temperature = request.Temperature
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/hackathon-openai-Infinity-dep-svc8/embeddings?api-version=2023-05-15")
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        var content = doc.RootElement.GetProperty("choices")[0].GetProperty("text").GetString();

        return Ok(new OpenAIResponse { ResponseText = content?.Trim() });
    }
}