using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;

Console.WriteLine("===========================================");
Console.WriteLine("Example 8: List Endpoints for a Model");
Console.WriteLine("===========================================\n");

await Example08.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example08
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 8: List Endpoints for a Model ===");
        try
        {
            var endpointsResponse = await client.Endpoints.ListAsync("openai", "gpt-3.5-turbo");
            
            Console.WriteLine($"Model: {endpointsResponse.Name}");
            Console.WriteLine($"Description: {endpointsResponse.Description}");
            Console.WriteLine($"Architecture:");
            Console.WriteLine($"  Tokenizer: {endpointsResponse.Architecture.Tokenizer}");
            Console.WriteLine($"  Modality: {endpointsResponse.Architecture.Modality}");
            Console.WriteLine($"\nAvailable Endpoints: {endpointsResponse.Endpoints.Count}");
            
            foreach (var endpoint in endpointsResponse.Endpoints.Take(3))
            {
                Console.WriteLine($"\n- {endpoint.Name}");
                Console.WriteLine($"  Provider: {endpoint.ProviderName}");
                Console.WriteLine($"  Context Length: {endpoint.ContextLength:N0} tokens");
                Console.WriteLine($"  Pricing:");
                Console.WriteLine($"    Prompt: ${endpoint.Pricing.Prompt}/token");
                Console.WriteLine($"    Completion: ${endpoint.Pricing.Completion}/token");
                
                if (endpoint.UptimeLast30m.HasValue)
                {
                    Console.WriteLine($"  Uptime (30m): {endpoint.UptimeLast30m.Value * 100:F2}%");
                }
                
                if (endpoint.LatencyLast30m != null && endpoint.LatencyLast30m.P50.HasValue)
                {
                    Console.WriteLine($"  Latency (median): {endpoint.LatencyLast30m.P50.Value:F1}ms");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing endpoints: {ex.Message}");
        }
    }
}


