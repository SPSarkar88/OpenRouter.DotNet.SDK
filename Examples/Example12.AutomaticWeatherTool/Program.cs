using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using OpenRouter.SDK.Models;

Console.WriteLine("===========================================");
Console.WriteLine("Example 12: Automatic Weather Tool Execution");
Console.WriteLine("===========================================\n");

await Example12.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example12
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 12: Tool System - Automatic Weather Tool Execution ===\n");

        try
        {
            // Define a weather tool with execute function
            var weatherToolSchema = JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["location"] = JsonSchemaBuilder.String("The city to get weather for"),
                    ["units"] = JsonSchemaBuilder.String("Temperature units", new List<string> { "celsius", "fahrenheit" })
                },
                new List<string> { "location" });

            var weatherTool = new Tool<WeatherToolInput, WeatherToolOutput>(
                name: "get_weather",
                description: "Get the current weather for a specific location",
                inputSchema: weatherToolSchema,
                executeFunc: async (input, context) =>
                {
                    Console.WriteLine($"  [Tool Executing] Getting weather for {input.Location}...");
                    
                    // Simulate API call
                    await Task.Delay(100);
                    
                    return new WeatherToolOutput
                    {
                        Location = input.Location,
                        Temperature = 72,
                        Condition = "Sunny",
                        Units = input.Units ?? "fahrenheit"
                    };
                });

            // Use Beta Responses API with proper input format (string instead of array)
            var request = new BetaResponsesRequest
            {
                Input = "What's the weather like in San Francisco?",
                Instructions = "You are a helpful weather assistant.",
                Model = ExampleConfig.ModelName,
                Store = false,
                ServiceTier = "auto"
            };

            var result = client.CallModel(
                request: request,
                tools: new[] { weatherTool },
                maxTurns: 5);

            // Get just the text (tool will execute automatically)
            var text = await result.GetTextAsync();
            Console.WriteLine($"Final response: {text}");

            // Get tool execution results
            var toolResults = await result.GetToolExecutionResultsAsync();
            Console.WriteLine($"\nTool executions: {toolResults.Count}");
            foreach (var tr in toolResults)
            {
                Console.WriteLine($"  - {tr.ToolName}: {(tr.IsSuccess ? "Success" : "Failed")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error with tool execution: {ex.Message}");
        }
    }
}

public class WeatherToolInput
{
    public required string Location { get; set; }
    public string? Units { get; set; }
}

public class WeatherToolOutput
{
    public required string Location { get; set; }
    public int Temperature { get; set; }
    public required string Condition { get; set; }
    public required string Units { get; set; }
}


