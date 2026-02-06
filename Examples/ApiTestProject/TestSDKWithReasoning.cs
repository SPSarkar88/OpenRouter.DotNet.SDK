using OpenRouter.SDK;
using OpenRouter.SDK.Models;

namespace ApiTestProject;

public class TestSDKWithReasoning
{
    public static async Task RunAsync()
    {
        var apiKey = "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";
        
        Console.WriteLine("Testing OpenRouter SDK (with RestSharp) - Reasoning Enabled");
        Console.WriteLine("=".PadRight(60, '='));
        
        var openRouter = new OpenRouterClient(apiKey);
        
        try
        {
            Console.WriteLine("Sending request with reasoning enabled...\n");
            
            var request = new ChatCompletionRequest
            {
                Model = "arcee-ai/trinity-large-preview:free",
                Messages = new List<Message>
                {
                    new UserMessage { Role = "user", Content = "What are the three most important principles of good software architecture?" }
                },
                Reasoning = new ReasoningConfig
                {
                    Enabled = true
                }
            };
            
            var response = await openRouter.Chat.CreateAsync(request);
            
            Console.WriteLine("✅ SUCCESS! Received response from OpenRouter SDK\n");
            Console.WriteLine($"Model: {response.Model}");
            Console.WriteLine($"ID: {response.Id}");
            Console.WriteLine($"Tokens Used: {response.Usage?.TotalTokens ?? 0}");
            
            if (response.Choices?.Count > 0)
            {
                var message = response.Choices[0].Message;
                Console.WriteLine($"\nAssistant Response:\n{message.Content}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
}
