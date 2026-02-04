using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;

Console.WriteLine("===========================================");
Console.WriteLine("Example 1: Simple Text Completion");
Console.WriteLine("===========================================\n");

await Example01.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example01
{
    public static async Task RunAsync()
    {
        // Get API key from .env file or environment variable
        var apiKey = ExampleConfig.ApiKey;
        var client = new OpenRouterClient(apiKey);
        Console.WriteLine("=== Example 1: Simple Text Completion ===");

        try
        {
            var request = new OpenRouter.SDK.Models.ChatCompletionRequest
            {
                Model = ExampleConfig.ModelName,
                Messages = new List<OpenRouter.SDK.Models.Message>
                {
                    new OpenRouter.SDK.Models.UserMessage 
                    {  
                        Role = "user",
                        Content = "How many r's are in the word 'strawberry'?" 
                    }
                },
                Reasoning = new OpenRouter.SDK.Models.ReasoningConfig
                {
                    Enabled = true
                }
            };

            Console.WriteLine("Sending request (matches your curl command)...\n");
            
            var response = await client.Chat.CreateAsync(request);
            
            Console.WriteLine($"Response:\n{response.Choices[0].Message.Content}");
        }
        catch (System.Text.Json.JsonException jsonEx)
        {
            Console.WriteLine($"JSON Error: {jsonEx.Message}");
            Console.WriteLine($"\nThis usually means the API returned HTML instead of JSON.");
            Console.WriteLine($"Common causes:");
            Console.WriteLine($"  1. Invalid API key");
            Console.WriteLine($"  2. API key doesn't have proper permissions");
            Console.WriteLine($"  3. Network/firewall blocking the request");
            Console.WriteLine($"\nPlease verify your API key at: https://openrouter.ai/keys");
        }
        catch (OpenRouter.SDK.Exceptions.UnauthorizedException)
        {
            Console.WriteLine("ERROR: Unauthorized - Your API key is invalid!");
            Console.WriteLine("Please get a valid API key from: https://openrouter.ai/keys");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
            }
        }
    }
}

