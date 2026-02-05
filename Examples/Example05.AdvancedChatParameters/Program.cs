using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using OpenRouter.SDK.Models;

Console.WriteLine("===========================================");
Console.WriteLine("Example 5: Advanced Chat with Custom Parameters");
Console.WriteLine("===========================================\n");

await Example05.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example05
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 5: Advanced Chat with Custom Parameters ===");
        try
        {
            var advancedRequest = new ChatCompletionRequest
            {
                Model = ExampleConfig.ModelName,
                Messages = new List<Message>
                {
                    new SystemMessage { Content = "You are a creative storyteller." },
                    new UserMessage { Content = "Tell me a very short story about a robot." }
                },
                Temperature = 0.8,
                TopP = 0.9,
                MaxTokens = 150,
                FrequencyPenalty = 0.5,
                PresencePenalty = 0.3
            };

            var advancedResponse = await client.Chat.CreateAsync(advancedRequest);
            
            Console.WriteLine($"Model: {advancedResponse.Model}");
            if (advancedResponse.Choices?.Count > 0)
            {
                Console.WriteLine($"Story: {advancedResponse.Choices[0].Message.Content}");
            }
            
            if (advancedResponse.Usage != null)
            {
                Console.WriteLine($"\nTokens used: {advancedResponse.Usage.TotalTokens}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}


