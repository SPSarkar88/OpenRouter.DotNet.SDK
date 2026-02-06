using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApiTestProject;

public class TestMinimal
{
    public static async Task RunAsync()
    {
        var apiKey = "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";
        
        Console.WriteLine("Minimal HTTP test - matching TypeScript exactly");
        Console.WriteLine("=".PadRight(60, '='));
        
        using var handler = new HttpClientHandler();
        using var client = new HttpClient(handler);
        client.BaseAddress = new Uri("https://openrouter.ai/api/v1");
        client.DefaultRequestHeaders.ExpectContinue = false;
        
        // Use AuthenticationHeaderValue exactly like the SDK
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        
        // Match TypeScript SDK User-Agent - DON'T set it, let HttpClient use default
        //client.DefaultRequestHeaders.Add("User-Agent", "OpenRouter-SDK-CSharp/0.1.0");
        
        var requestBody = @"{
  ""model"": ""arcee-ai/trinity-large-preview:free"",
  ""messages"": [
    {
      ""role"": ""user"",
      ""content"": ""What are the three most important principles of good software architecture?""
    }
  ],
  ""stream"": false
}";
        
        Console.WriteLine($"Request Body: {requestBody}\n");
        
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        
        try
        {
            Console.WriteLine("Sending request...");
            var response = await client.PostAsync("/chat/completions", content);
            
            Console.WriteLine($"\nResponse Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType}");
            
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\nResponse Body (first 1000 chars):\n{responseBody.Substring(0, Math.Min(1000, responseBody.Length))}");
            
            if (responseBody.Contains("\"id\":"))
            {
                Console.WriteLine("\n✅ SUCCESS! Received JSON response!");
            }
            else
            {
                Console.WriteLine("\n❌ FAILED! Received HTML instead of JSON");
                Console.WriteLine("\nResponse Headers:");
                foreach (var header in response.Headers)
                {
                    Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
}
