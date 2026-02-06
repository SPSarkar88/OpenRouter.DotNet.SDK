using System.Net.Http.Json;

var apiKey = "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";
using var client = new HttpClient();
client.BaseAddress = new Uri("https://openrouter.ai/api/v1");
client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

var request = new { model = "openai/gpt-3.5-turbo", messages = new[] { new { role = "user", content = "Hello" } } };

Console.WriteLine("Testing API call...");
var response = await client.PostAsJsonAsync("/chat/completions", request);
Console.WriteLine($"Status: {response.StatusCode}");
Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType}");
var content = await response.Content.ReadAsStringAsync();
Console.WriteLine($"Response length: {content.Length}");
Console.WriteLine($"First 300 chars: {content.Substring(0, Math.Min(300, content.Length))}");
