using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApiTestProject;

public class TestRawRequest
{
    public static async Task RunAsync()
    {
        var apiKey = "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";
        
        Console.WriteLine("Testing direct HTTP request to OpenRouter API");
        Console.WriteLine("=".PadRight(60, '='));
        
        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://openrouter.ai/api/v1");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        client.DefaultRequestHeaders.Add("User-Agent", "OpenRouter-SDK-CSharp/0.1.0");
        
        var requestBody = @"{
  ""model"": ""arcee-ai/trinity-large-preview:free"",
  ""messages"": [
    {
      ""role"": ""user"",
      ""content"": ""test""
    }
  ],
  ""reasoning"": {
    ""enabled"": true
  }
}";
        
        Console.WriteLine($"\nRequest URL: https://openrouter.ai/api/v1/chat/completions");
        Console.WriteLine($"Authorization: Bearer {apiKey.Substring(0, 20)}...");
        Console.WriteLine($"\nRequest Headers:");
        Console.WriteLine($"  Authorization: Bearer {apiKey}");
        Console.WriteLine($"  User-Agent: OpenRouter-SDK-CSharp/0.1.0");
        Console.WriteLine($"  Content-Type: application/json; charset=utf-8");
        Console.WriteLine($"Body: {requestBody}\n");
        
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        
        try
        {
            var response = await client.PostAsync("/chat/completions", content);
            
            Console.WriteLine($"Response Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType}");
            
            Console.WriteLine($"\nAll Response Headers:");
            foreach (var header in response.Headers)
            {
                Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            }
            foreach (var header in response.Content.Headers)
            {
                Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            }
            
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\nResponse Body (first 500 chars):");
            Console.WriteLine(responseBody.Length > 500 ? responseBody.Substring(0, 500) : responseBody);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"\n\nFull Response:");
                Console.WriteLine(responseBody);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
}
