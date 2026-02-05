using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;

Console.WriteLine("===========================================");
Console.WriteLine("Example 4: List Available Models");
Console.WriteLine("===========================================\n");

await Example04.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example04
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 4: List Available Models ===");
        try
        {
            var modelsResponse = await client.Models.GetModelsAsync();
            
            Console.WriteLine($"Found {modelsResponse.Data.Count} models:");
            foreach (var model in modelsResponse.Data.Take(10))
            {
                Console.WriteLine($"- {model.Id}");
                if (!string.IsNullOrEmpty(model.Name))
                {
                    Console.WriteLine($"  Name: {model.Name}");
                }
                if (model.ContextLength.HasValue)
                {
                    Console.WriteLine($"  Context: {model.ContextLength:N0} tokens");
                }
                if (model.Pricing != null)
                {
                    Console.WriteLine($"  Pricing: ${model.Pricing.Prompt}/token (prompt), ${model.Pricing.Completion}/token (completion)");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing models: {ex.Message}");
        }
    }
}


