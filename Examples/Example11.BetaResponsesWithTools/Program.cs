using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using OpenRouter.SDK.Models;

Console.WriteLine("===========================================");
Console.WriteLine("Example 11: Beta Responses with Tools");
Console.WriteLine("===========================================\n");

await Example11.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example11
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 11: Beta Responses API with Tools ===");
        Console.WriteLine("Demonstrates using tools/functions with the /responses endpoint.\n");
        
        try
        {
            // Define function tool for Beta Responses API
            var tools = new List<ResponsesFunctionTool>
            {
                new ResponsesFunctionTool
                {
                    Name = "get_weather",
                    Description = "Get the current weather for a location",
                    Parameters = new Dictionary<string, object?>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["location"] = new { type = "string", description = "City name" },
                            ["units"] = new { type = "string", @enum = new[] { "celsius", "fahrenheit" }, description = "Temperature units" }
                        },
                        ["required"] = new[] { "location" }
                    }
                }
            };

            var request = new BetaResponsesRequest
            {
                Input = "What's the weather like in Tokyo?",
                Instructions = "You are a helpful weather assistant.",
                Model = ExampleConfig.ModelName,
                Tools = tools,
                Store = false,
                ServiceTier = "auto",
                MaxOutputTokens = 150
            };

            var response = await client.Beta.Responses.SendAsync(request);
            
            Console.WriteLine($"Response ID: {response.Id}");
            Console.WriteLine($"Model: {response.Model}");
            Console.WriteLine($"Status: {response.Status}");
            Console.WriteLine($"Tools provided: {tools.Count}");
            Console.WriteLine($"\nOutput:");
            
            if (response.Output?.Count > 0)
            {
                foreach (var output in response.Output)
                {
                    if (output.Type == "message" && output.Content != null)
                    {
                        foreach (var content in output.Content)
                        {
                            if (content.Type == "output_text" && !string.IsNullOrEmpty(content.Text))
                            {
                                Console.WriteLine($"  {content.Text}");
                            }
                        }
                    }
                    else if (output.Type == "text" && !string.IsNullOrEmpty(output.Text))
                    {
                        Console.WriteLine($"  {output.Text}");
                    }
                }
            }
            
            Console.WriteLine($"\n✅ Beta Responses API with tools is working!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}


