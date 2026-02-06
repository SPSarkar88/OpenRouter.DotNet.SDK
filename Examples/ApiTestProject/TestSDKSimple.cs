using OpenRouter.SDK;
using OpenRouter.SDK.Models;

namespace ApiTestProject;

public class TestSDKSimple
{
    public static async Task RunAsync()
    {
        var apiKey = "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";
        
        Console.WriteLine("Testing OpenRouter SDK (with RestSharp) - Simple Chat");
        Console.WriteLine("=".PadRight(60, '='));
        
        var openRouter = new OpenRouterClient(apiKey);
        
        try
        {
            Console.WriteLine("Sending simple chat request...\n");
            
            var request = new ChatCompletionRequest
            {
                Model = "openai/gpt-3.5-turbo",
                Messages = new List<Message>
                {
                    new UserMessage { Role = "user", Content = "Say hello!" }
                }
            };
            
            var response = await openRouter.Chat.CreateAsync(request);
            
            Console.WriteLine("✅ SUCCESS! Received response from OpenRouter SDK\n");
            Console.WriteLine($"Model: {response.Model}");
            Console.WriteLine($"ID: {response.Id}");
            
            if (response.Choices?.Count > 0)
            {
                var message = response.Choices[0].Message;
                Console.WriteLine($"\nAssistant: {message.Content}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
}
