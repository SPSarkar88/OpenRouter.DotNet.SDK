using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ApiTestProject;

public class TestApiCall
{
    public static async Task RunAsync()
    {
        var apiKey = "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";
        var client = new HttpClient();
        client.BaseAddress = new Uri("https://openrouter.ai/api/v1");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        client.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/openrouter/sdk-csharp");
        client.DefaultRequestHeaders.Add("X-Title", "OpenRouter C# SDK Test");

        var request = new
        {
            model = "openai/gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "user", content = "Say hello" }
            }
        };

        Console.WriteLine("Sending request to: " + client.BaseAddress + "chat/completions");
        Console.WriteLine("Authorization: Bearer " + apiKey.Substring(0, 20) + "...");

        try
        {
            var response = await client.PostAsJsonAsync("/chat/completions", request);
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType}");
            
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response (first 500 chars):");
            Console.WriteLine(content.Substring(0, Math.Min(500, content.Length)));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
