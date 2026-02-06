using System;
using RestSharp;
using System.Threading.Tasks;

namespace ApiTestProject;

public class TestRestSharp
{
    public static async Task RunAsync()
    {
        var apiKey = "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";
        
        Console.WriteLine("Testing with RestSharp HTTP client");
        Console.WriteLine("=".PadRight(60, '='));
        
        var options = new RestClientOptions("https://openrouter.ai/api/v1");
        var client = new RestClient(options);
        
        var request = new RestRequest("/chat/completions", Method.Post);
        request.AddHeader("Authorization", $"Bearer {apiKey}");
        request.AddHeader("Content-Type", "application/json");
        
        var body = new
        {
            model = "arcee-ai/trinity-large-preview:free",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = "What are the three most important principles of good software architecture?"
                }
            },
            stream = false
        };
        
        request.AddJsonBody(body);
        
        Console.WriteLine("Sending request...\n");
        
        try
        {
            var response = await client.ExecuteAsync(request);
            
            Console.WriteLine($"Response Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"Content-Type: {response.ContentType}");
            Console.WriteLine($"Content Length: {response.Content?.Length ?? 0}\n");
            
            if (response.Content != null)
            {
                if (response.Content.Contains("\"id\":"))
                {
                    Console.WriteLine("✅ SUCCESS! Received JSON response!");
                    Console.WriteLine($"\nResponse (first 500 chars):\n{response.Content.Substring(0, Math.Min(500, response.Content.Length))}");
                }
                else
                {
                    Console.WriteLine("❌ FAILED! Received HTML instead of JSON");
                    Console.WriteLine($"\nResponse (first 500 chars):\n{response.Content.Substring(0, Math.Min(500, response.Content.Length))}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
        }
    }
}
