using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using OpenRouter.SDK.Models;

Console.WriteLine("===========================================");
Console.WriteLine("Example 10: Beta Responses API");
Console.WriteLine("===========================================\n");

await Example10.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example10
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 10: Beta Responses API ===");
        Console.WriteLine("Testing /responses endpoint with TypeScript-compatible format...\n");
        
        try
        {
            var betaRequest = new BetaResponsesRequest
            {
                Input = "What is the capital of France?",
                Instructions = "You are a helpful geography teacher. Be concise and educational.",
                Model = ExampleConfig.ModelName,
                Store = false,
                ServiceTier = "auto",
                MaxOutputTokens = 150,
                Temperature = 0.7
            };

            Console.WriteLine("Sending request to /responses endpoint...");
            var betaResponse = await client.Beta.Responses.SendAsync(betaRequest);
            
            Console.WriteLine($"✅ Success! Response ID: {betaResponse.Id}");
            Console.WriteLine($"Model: {betaResponse.Model}");
            Console.WriteLine($"Status: {betaResponse.Status}");
            Console.WriteLine($"Output items count: {betaResponse.Output?.Count ?? 0}");
            Console.WriteLine($"\nOutput:");
            
            if (betaResponse.Output?.Count > 0)
            {
                foreach (var output in betaResponse.Output)
                {
                    Console.WriteLine($"  Type: {output.Type}");
                    
                    if (output.Type == "message" && output.Content != null)
                    {
                        foreach (var content in output.Content)
                        {
                            if (content.Type == "output_text" && !string.IsNullOrEmpty(content.Text))
                            {
                                Console.WriteLine($"  Text: {content.Text}");
                            }
                        }
                    }
                    else if (output.Type == "text" && !string.IsNullOrEmpty(output.Text))
                    {
                        Console.WriteLine($"  Text: {output.Text}");
                    }
                }
            }
            else
            {
                Console.WriteLine("  (No output items returned)");
            }
            
            Console.WriteLine($"\n✅ /responses endpoint is working in C#!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error with /responses endpoint: {ex.Message}");
            Console.WriteLine("\nFalling back to chat completions...\n");
            
            // Fallback to regular chat completions API
            var chatRequest = new ChatCompletionRequest
            {
                Model = ExampleConfig.ModelName,
                Messages = new List<Message>
                {
                    new SystemMessage { Content = "You are a helpful geography teacher. Be concise and educational." },
                    new UserMessage { Content = "What is the capital of France? Provide a brief explanation." }
                },
                MaxTokens = 150,
                Temperature = 0.7
            };

            var response = await client.Chat.CreateAsync(chatRequest);
            
            Console.WriteLine($"Response ID: {response.Id}");
            Console.WriteLine($"Model: {response.Model}");
            Console.WriteLine($"\nResponse:");
            
            if (response.Choices?.Count > 0)
            {
                Console.WriteLine(response.Choices[0].Message?.Content);
            }
            
            if (response.Usage != null)
            {
                Console.WriteLine($"\nTokens: {response.Usage.TotalTokens}");
            }
        }
    }
}


