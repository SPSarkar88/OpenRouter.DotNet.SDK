using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;

Console.WriteLine("===========================================");
Console.WriteLine("Example 7: List Providers");
Console.WriteLine("===========================================\n");

await Example07.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example07
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 7: List Providers ===");
        try
        {
            var providersResponse = await client.Providers.ListAsync();
            
            Console.WriteLine($"Found {providersResponse.Data.Count} providers:");
            foreach (var provider in providersResponse.Data.Take(5))
            {
                Console.WriteLine($"- {provider.Name} ({provider.Slug})");
                if (!string.IsNullOrEmpty(provider.StatusPageUrl))
                {
                    Console.WriteLine($"  Status: {provider.StatusPageUrl}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing providers: {ex.Message}");
        }
    }
}


