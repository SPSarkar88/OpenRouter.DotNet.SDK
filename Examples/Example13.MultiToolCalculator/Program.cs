using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using OpenRouter.SDK.Models;

Console.WriteLine("===========================================");
Console.WriteLine("Example 13: Multi-Tool Calculator");
Console.WriteLine("===========================================\n");

await Example13.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example13
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 13: Tool System - Multi-Tool Calculator ===\n");

        try
        {
            // Define calculator function tools using OpenRouter's function calling
            var tools = new List<ToolDefinition>
            {
                new ToolDefinition
                {
                    Type = "function",
                    Function = new FunctionDefinition
                    {
                        Name = "add",
                        Description = "Add two numbers together",
                        Parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                x = new { type = "number", description = "First number" },
                                y = new { type = "number", description = "Second number" }
                            },
                            required = new[] { "x", "y" }
                        }
                    }
                },
                new ToolDefinition
                {
                    Type = "function",
                    Function = new FunctionDefinition
                    {
                        Name = "multiply",
                        Description = "Multiply two numbers together",
                        Parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                x = new { type = "number", description = "First number" },
                                y = new { type = "number", description = "Second number" }
                            },
                            required = new[] { "x", "y" }
                        }
                    }
                }
            };

            // First request - let the model decide which tools to call
            var request = new ChatCompletionRequest
            {
                Model = ExampleConfig.ModelName,
                Messages = new List<Message>
                {
                    new SystemMessage { Content = "You are a calculator assistant. Use the provided tools to solve math problems step by step." },
                    new UserMessage { Content = "What is (5 + 3) * 2?" }
                },
                Tools = tools,
                ToolChoice = "auto"
            };

            Console.WriteLine("Sending initial request...");
            var response = await client.Chat.CreateAsync(request);

            // Process tool calls in a loop until we get a final answer
            var messages = new List<Message>
            {
                new SystemMessage { Content = "You are a calculator assistant. Use the provided tools to solve math problems step by step." },
                new UserMessage { Content = "What is (5 + 3) * 2?" }
            };

            int maxIterations = 5;
            int iteration = 0;
            
            while (iteration < maxIterations)
            {
                iteration++;
                
                // Add assistant's response with tool calls
                if (response.Choices?[0].Message != null)
                {
                    messages.Add(response.Choices[0].Message);
                }

                // Check if there are tool calls to execute
                if (response.Choices?[0].Message?.ToolCalls != null && response.Choices[0].Message.ToolCalls.Count > 0)
                {
                    Console.WriteLine($"\nIteration {iteration}: Executing {response.Choices[0].Message.ToolCalls.Count} tool call(s)");
                    
                    foreach (var toolCall in response.Choices[0].Message.ToolCalls)
                    {
                        Console.WriteLine($"  Tool: {toolCall.Function?.Name}");
                        
                        var args = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(
                            toolCall.Function?.Arguments ?? "{}");
                        
                        double result = toolCall.Function?.Name switch
                        {
                            "add" => args!["x"] + args["y"],
                            "multiply" => args!["x"] * args["y"],
                            _ => 0
                        };

                        Console.WriteLine($"    {args!["x"]} {(toolCall.Function?.Name == "add" ? "+" : "*")} {args["y"]} = {result}");

                        // Add tool response
                        messages.Add(new ToolMessage
                        {
                            ToolCallId = toolCall.Id,
                            Content = result.ToString()
                        });
                    }

                    // Send follow-up request with tool results
                    Console.WriteLine("  Sending follow-up request...");
                    var followUpRequest = new ChatCompletionRequest
                    {
                        Model = ExampleConfig.ModelName,
                        Messages = messages,
                        Tools = tools
                    };

                    response = await client.Chat.CreateAsync(followUpRequest);
                }
                else
                {
                    // No more tool calls - we have the final answer
                    Console.WriteLine($"\nFinal Answer: {response.Choices?[0].Message?.Content}");
                    Console.WriteLine($"Total tokens used: {response.Usage?.TotalTokens}");
                    Console.WriteLine($"Completed in {iteration} iteration(s)");
                    break;
                }
            }
            
            if (iteration >= maxIterations)
            {
                Console.WriteLine("\nReached maximum iterations without final answer");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error with calculator: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}

public class CalcInput
{
    public double X { get; set; }
    public double Y { get; set; }
}

public class CalcOutput
{
    public double Result { get; set; }
}


