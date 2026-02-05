using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using OpenRouter.SDK.Models;
using System.Text.Json;

Console.WriteLine("===========================================");
Console.WriteLine("Example 14: Response Consumption Patterns");
Console.WriteLine("===========================================\n");

await Example14.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example14
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;
        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 14: Different Ways to Consume AI Responses ===\n");
        Console.WriteLine("This example shows practical patterns for using AI responses in your application:\n");

        // ========================================
        // PATTERN 1: Simple text-only response
        // Use case: Chatbot, Q&A, text generation
        // ========================================
        Console.WriteLine("PATTERN 1 - Get just the text (for chatbots, Q&A):");
        Console.WriteLine("---------------------------------------------------");
        var simpleRequest = new ChatCompletionRequest
        {
            Model = ExampleConfig.ModelName,
            Messages = new List<Message>
            {
                new UserMessage { Content = "Write a one-sentence greeting" }
            },
            MaxTokens = 50
        };

        var response1 = await client.Chat.CreateAsync(simpleRequest);
        var text = response1.Choices?[0].Message?.Content ?? "";
        Console.WriteLine($"Text: {text}");
        Console.WriteLine($"Use in app: chatResponse.Text = \"{text.Substring(0, Math.Min(30, text.Length))}...\"\n");

        // ========================================
        // PATTERN 2: Response with metadata
        // Use case: Monitoring, logging, cost tracking
        // ========================================
        Console.WriteLine("PATTERN 2 - Get metadata (for logging, cost tracking):");
        Console.WriteLine("------------------------------------------------------");
        var response2 = await client.Chat.CreateAsync(simpleRequest);
        
        Console.WriteLine($"Model used: {response2.Model}");
        Console.WriteLine($"Tokens consumed: {response2.Usage?.TotalTokens ?? 0}");
        Console.WriteLine($"  - Prompt: {response2.Usage?.PromptTokens ?? 0}");
        Console.WriteLine($"  - Completion: {response2.Usage?.CompletionTokens ?? 0}");
        Console.WriteLine($"Response ID: {response2.Id}");
        Console.WriteLine("Use in app: Log tokens for billing, track which model was used\n");

        // ========================================
        // PATTERN 3: Streaming response
        // Use case: Real-time UI updates, typewriter effect
        // ========================================
        Console.WriteLine("PATTERN 3 - Streaming (for real-time UI updates):");
        Console.WriteLine("-------------------------------------------------");
        var streamRequest = new ChatCompletionRequest
        {
            Model = ExampleConfig.ModelName,
            Messages = new List<Message>
            {
                new UserMessage { Content = "Count from 1 to 5" }
            },
            MaxTokens = 50,
            Stream = true
        };

        Console.Write("Streamed output: ");
        await foreach (var chunk in client.Chat.CreateStreamAsync(streamRequest))
        {
            var content = chunk.Choices?[0].Delta?.Content;
            if (!string.IsNullOrEmpty(content))
            {
                Console.Write(content); // Display each chunk as it arrives
            }
        }
        Console.WriteLine("\nUse in app: Update UI progressively as tokens arrive (typewriter effect)\n");

        // ========================================
        // PATTERN 4: Function calling detection
        // Use case: Tool orchestration, API integration
        // ========================================
        Console.WriteLine("PATTERN 4 - Detect function calls (for tool orchestration):");
        Console.WriteLine("-----------------------------------------------------------");
        var tools = new List<ToolDefinition>
        {
            new ToolDefinition
            {
                Type = "function",
                Function = new FunctionDefinition
                {
                    Name = "get_weather",
                    Description = "Get current weather for a location",
                    Parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            city = new { type = "string", description = "City name" }
                        },
                        required = new[] { "city" }
                    }
                }
            }
        };

        var toolRequest = new ChatCompletionRequest
        {
            Model = ExampleConfig.ModelName,
            Messages = new List<Message>
            {
                new UserMessage { Content = "What's the weather in Paris?" }
            },
            Tools = tools,
            MaxTokens = 100
        };

        var response4 = await client.Chat.CreateAsync(toolRequest);
        var message = response4.Choices?[0].Message;

        if (message?.ToolCalls != null && message.ToolCalls.Count > 0)
        {
            var toolCall = message.ToolCalls[0];
            Console.WriteLine($"AI wants to call: {toolCall.Function?.Name}");
            Console.WriteLine($"With arguments: {toolCall.Function?.Arguments}");
            
            // Parse the arguments
            var args = JsonSerializer.Deserialize<Dictionary<string, object>>(
                toolCall.Function?.Arguments ?? "{}");
            
            Console.WriteLine("\nUse in app:");
            Console.WriteLine($"  if (toolCall.Name == \"get_weather\")");
            Console.WriteLine($"      var city = args[\"city\"];");
            Console.WriteLine($"      var weather = await WeatherService.GetAsync(city);");
            Console.WriteLine($"      return weather;\n");
        }
        else
        {
            Console.WriteLine($"Direct response: {message?.Content}");
            Console.WriteLine("(Model chose to answer directly instead of calling function)\n");
        }

        // ========================================
        // PATTERN 5: Error handling
        // Use case: Production robustness
        // ========================================
        Console.WriteLine("PATTERN 5 - Error handling (for production apps):");
        Console.WriteLine("-------------------------------------------------");
        Console.WriteLine("Best practices:");
        Console.WriteLine("  • Wrap API calls in try-catch blocks");
        Console.WriteLine("  • Check for null responses and empty choices");
        Console.WriteLine("  • Handle rate limits (429 errors)");
        Console.WriteLine("  • Log errors with context for debugging");
        Console.WriteLine("  • Provide fallback responses to users\n");

        Console.WriteLine("Example code pattern:");
        Console.WriteLine(@"  try {
      var response = await client.Chat.CreateAsync(request);
      return response.Choices?[0].Message?.Content ?? ""No response"";
  }
  catch (RateLimitException ex) {
      logger.LogWarning($""Rate limited, retry after {ex.RetryAfter}"");
      await Task.Delay(ex.RetryAfter);
      // retry logic
  }
  catch (Exception ex) {
      logger.LogError(ex, ""AI request failed"");
      return ""I apologize, I'm having trouble responding right now."";
  }");

        Console.WriteLine("\n========================================");
        Console.WriteLine("SUMMARY - When to use each pattern:");
        Console.WriteLine("========================================");
        Console.WriteLine("1. Simple text: Chatbots, FAQs, content generation");
        Console.WriteLine("2. With metadata: Billing, monitoring, A/B testing models");
        Console.WriteLine("3. Streaming: Chat UI, long responses, user engagement");
        Console.WriteLine("4. Function calls: API integration, database queries, actions");
        Console.WriteLine("5. Error handling: All production applications");
    }
}


