using OpenRouter.SDK;
using OpenRouter.SDK.Models;
using OpenRouter.Examples.EnvConfig;
using System.Text.Json;

var apiKey = ExampleConfig.ApiKey;
var modelName = ExampleConfig.ModelName;
var client = new OpenRouterClient(apiKey);

Console.WriteLine("Testing Beta Responses Streaming...\n");

var request = new BetaResponsesRequest
{
    Model = modelName,
    Input = "Say hello",
    Stream = true
};

Console.WriteLine("Sending streaming request...\n");

await foreach (var chunk in client.Beta.Responses.SendStreamAsync(request))
{
    Console.WriteLine($"Event Type: {chunk.Type}");
    Console.WriteLine($"Index: {chunk.Index}");
    
    if (chunk.Delta != null)
    {
        Console.WriteLine($"Delta Type: {chunk.Delta.Type}");
        Console.WriteLine($"Delta Text: {chunk.Delta.Text ?? "(null)"}");
        Console.WriteLine($"Delta Content Count: {chunk.Delta.Content?.Count ?? 0}");
        
        if (chunk.Delta.Content != null)
        {
            foreach (var content in chunk.Delta.Content)
            {
                Console.WriteLine($"  Content Type: {content.Type}");
                Console.WriteLine($"  Content Text: {content.Text ?? "(null)"}");
            }
        }
    }
    
    if (chunk.Response != null)
    {
        Console.WriteLine($"Response ID: {chunk.Response.Id}");
        Console.WriteLine($"Response Status: {chunk.Response.Status}");
    }
    
    Console.WriteLine(JsonSerializer.Serialize(chunk, new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine("---");
}

Console.WriteLine("\nDone!");
