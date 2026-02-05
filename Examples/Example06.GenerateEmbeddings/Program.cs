using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using OpenRouter.SDK.Models;

Console.WriteLine("===========================================");
Console.WriteLine("Example 6: Generate Embeddings");
Console.WriteLine("===========================================\n");

await Example06.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example06
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 6: Generate Embeddings ===");
        try
        {
            var embeddingsRequest = new EmbeddingRequest
            {
                Model = "openai/text-embedding-ada-002",
                Input = new List<string>
                {
                    "The quick brown fox jumps over the lazy dog",
                    "Machine learning is a subset of artificial intelligence"
                }
            };

            var embeddingResponse = await client.Embeddings.GenerateAsync(embeddingsRequest);
            
            Console.WriteLine($"Model: {embeddingResponse.Model}");
            Console.WriteLine($"Generated {embeddingResponse.Data.Count} embeddings:");
            
            foreach (var embedding in embeddingResponse.Data)
            {
                if (embedding.Embedding is double[] embeddingVector)
                {
                    Console.WriteLine($"- Index {embedding.Index}: {embeddingVector.Length} dimensions");
                    Console.WriteLine($"  First 5 values: [{string.Join(", ", embeddingVector.Take(5).Select(v => v.ToString("F4")))}...]");
                }
                else
                {
                    Console.WriteLine($"- Index {embedding.Index}: Embedding data (base64 or other format)");
                }
            }
            
            if (embeddingResponse.Usage != null)
            {
                Console.WriteLine($"Tokens used: {embeddingResponse.Usage.TotalTokens}");
                if (embeddingResponse.Usage.Cost.HasValue)
                {
                    Console.WriteLine($"Cost: ${embeddingResponse.Usage.Cost:F6}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating embeddings: {ex.Message}");
        }
    }
}


