using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;

Console.WriteLine("===========================================");
Console.WriteLine("Example 2: Streaming Completion");
Console.WriteLine("===========================================\n");

await Example02.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example02
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 2: Streaming Completion ===");
        try
        {
            Console.Write("Streaming response: ");
            await foreach (var chunk in client.CallModelStreamAsync(
                model: "openai/gpt-3.5-turbo",
                userMessage: "Count from 1 to 10 slowly",
                maxTokens: 100
            ))
            {
                Console.Write(chunk);
            }
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error streaming: {ex.Message}");
        }
    }
}


