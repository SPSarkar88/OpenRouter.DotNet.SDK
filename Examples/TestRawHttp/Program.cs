using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
