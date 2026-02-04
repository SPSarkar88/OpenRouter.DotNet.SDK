using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using OpenRouter.SDK.Models;

Console.WriteLine("===========================================");
Console.WriteLine("Example 3: Using Chat Service Directly");
Console.WriteLine("===========================================\n");

await Example03.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example03
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 3: Using Chat Service Directly ===");
        try
        {
            var chatRequest = new ChatCompletionRequest
            {
                Model = ExampleConfig.ModelName,
                Messages = new List<Message>
                {
                    new SystemMessage { Content = "You are a helpful assistant." },
                    new UserMessage { Content = "What is 2+2?" }
                }
            };

            var chatResponse = await client.Chat.CreateAsync(chatRequest);
            
            Console.WriteLine($"Model: {chatResponse.Model}");
            if (chatResponse.Choices?.Count > 0)
            {
                var message = chatResponse.Choices[0].Message;
                Console.WriteLine($"Response: {message.Content}");
            }
            
            if (chatResponse.Usage != null)
            {
                Console.WriteLine($"Tokens used: {chatResponse.Usage.TotalTokens}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}


