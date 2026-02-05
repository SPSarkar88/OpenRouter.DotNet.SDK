using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;

Console.WriteLine("===========================================");
Console.WriteLine("Example 15: Generation Metadata");
Console.WriteLine("===========================================\n");

await Example15.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example15
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 15: Generations Service - Get Generation Metadata ===\n");

        // Note: This requires a valid generation ID from a previous API call
        try
        {
            var generationId = "your-generation-id-here";
            var generationMetadata = await client.Generations.GetGenerationAsync(generationId);
            
            Console.WriteLine($"Generation ID: {generationMetadata.Data.Id}");
            Console.WriteLine($"Model: {generationMetadata.Data.Model}");
            Console.WriteLine($"Total Cost: ${generationMetadata.Data.TotalCost:F6}");
            Console.WriteLine($"Provider: {generationMetadata.Data.ProviderName ?? "N/A"}");
            Console.WriteLine($"Tokens (Prompt/Completion): {generationMetadata.Data.TokensPrompt}/{generationMetadata.Data.TokensCompletion}");
            Console.WriteLine($"Latency: {generationMetadata.Data.Latency}ms");
            Console.WriteLine($"Created At: {generationMetadata.Data.CreatedAt}");
            
            if (generationMetadata.Data.CacheDiscount.HasValue)
            {
                Console.WriteLine($"Cache Discount: ${generationMetadata.Data.CacheDiscount.Value:F6}");
            }
            
            if (generationMetadata.Data.Streamed.HasValue)
            {
                Console.WriteLine($"Streamed: {generationMetadata.Data.Streamed.Value}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Note: Generation metadata requires a valid generation ID from a previous request.");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}


