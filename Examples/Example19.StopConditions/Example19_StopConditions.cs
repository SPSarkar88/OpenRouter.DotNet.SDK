using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;

namespace OpenRouter.Examples.EnvConfig;

/// <summary>
/// Example 19: Stop Conditions - Advanced Control Flow for Tool Execution
/// 
/// Demonstrates how to use stop conditions to control when tool execution should halt.
/// Stop conditions provide fine-grained control over the tool orchestration loop.
/// </summary>
public static class Example19_StopConditions
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n=== Example 19: Stop Conditions ===\n");

        // Get API key from environment
        var apiKey = ExampleConfig.ApiKey;
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Please set your API key");
            return;
        }

        var client = new OpenRouterClient(apiKey);

        // Define tools
        var searchTool = new Tool<SearchInput, SearchResult>(
            name: "search",
            description: "Search the web for information",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["query"] = JsonSchemaBuilder.String("Search query")
                },
                required: new List<string> { "query" }
            ),
            executeFunc: async (input, context) =>
            {
                Console.WriteLine($"Searching for: {input.Query}");
                await Task.Delay(100); // Simulate API call
                return new SearchResult
                {
                    Results = $"Search results for '{input.Query}': Found 42 results"
                };
            }
        );

        var calculatorTool = new Tool<CalculatorInput, CalculatorResult>(
            name: "calculator",
            description: "Perform mathematical calculations",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["expression"] = JsonSchemaBuilder.String("Mathematical expression")
                },
                required: new List<string> { "expression" }
            ),
            executeFunc: async (input, context) =>
            {
                Console.WriteLine($"Calculating: {input.Expression}");
                await Task.Delay(100);
                return new CalculatorResult { Result = 42.0 };
            }
        );

        // Example 1: Step count limit
        Console.WriteLine("Example 1: Stop after 3 steps");
        await DemoStepCountLimit(client, searchTool, calculatorTool);

        // Example 2: Stop when specific tool is called
        Console.WriteLine("\nExample 2: Stop when search tool is called");
        await DemoToolDetection(client, searchTool, calculatorTool);

        // Example 3: Token budget limit
        Console.WriteLine("\nExample 3: Stop when token budget exceeded");
        await DemoTokenLimit(client, searchTool);

        // Example 4: Custom stop condition
        Console.WriteLine("\nExample 4: Custom stop condition (output contains 'DONE')");
        await DemoCustomCondition(client, searchTool);

        // Example 5: Multiple conditions with OR logic
        Console.WriteLine("\nExample 5: Multiple conditions (OR logic)");
        await DemoMultipleConditions(client, searchTool, calculatorTool);

        // Example 6: Async stop condition
        Console.WriteLine("\nExample 6: Async stop condition");
        await DemoAsyncCondition(client, searchTool);
    }

    private static async Task DemoStepCountLimit(
        OpenRouterClient client,
        ITool searchTool,
        ITool calculatorTool)
    {
        var request = new BetaResponsesRequest
        {
            Model = ExampleConfig.ModelName,
            Input = "Search for 'AI' then calculate 2+2, then search for 'ML'",
            Store = false,
            ServiceTier = "auto"
        };

        var result = client.CallModel(
            request,
            tools: new[] { searchTool, calculatorTool },
            maxTurns: 10,
            stopConditions: new List<StopCondition>
            {
                StopConditions.StepCountIs(3) // Stop after 3 steps
            }
        );

        var orchestrationResult = await result.GetOrchestrationResultAsync();
        
        Console.WriteLine($"Steps executed: {orchestrationResult?.Steps.Count ?? 0}");
        Console.WriteLine($"Stopped by condition: {orchestrationResult?.StoppedByCondition ?? false}");
        Console.WriteLine($"Tools executed: {orchestrationResult?.ToolExecutionResults.Count ?? 0}");
    }

    private static async Task DemoToolDetection(
        OpenRouterClient client,
        ITool searchTool,
        ITool calculatorTool)
    {
        var request = new BetaResponsesRequest
        {
            Model = ExampleConfig.ModelName,
            Input = "Calculate 10+5, then search for 'OpenRouter', then calculate 20*2",
            Store = false,
            ServiceTier = "auto"
        };

        var result = client.CallModel(
            request,
            tools: new[] { searchTool, calculatorTool },
            maxTurns: 10,
            stopConditions: new List<StopCondition>
            {
                StopConditions.HasToolCall("search") // Stop when search is called
            }
        );

        var orchestrationResult = await result.GetOrchestrationResultAsync();
        
        var searchCalled = orchestrationResult?.Steps
            .Any(s => s.ToolCalls.Any(tc => tc.Name == "search")) ?? false;
        
        Console.WriteLine($"Search tool called: {searchCalled}");
        Console.WriteLine($"Stopped by condition: {orchestrationResult?.StoppedByCondition ?? false}");
    }

    private static async Task DemoTokenLimit(OpenRouterClient client, ITool searchTool)
    {
        var request = new BetaResponsesRequest
        {
            Model = ExampleConfig.ModelName,
            Input = "Search for information about AI",
            Store = false,
            ServiceTier = "auto"
        };

        var result = client.CallModel(
            request,
            tools: new[] { searchTool },
            maxTurns: 10,
            stopConditions: new List<StopCondition>
            {
                StopConditions.MaxTokensUsed(100) // Stop when 100 tokens used
            }
        );

        var orchestrationResult = await result.GetOrchestrationResultAsync();
        var totalTokens = orchestrationResult?.Steps.Sum(s => s.TotalTokens) ?? 0;
        
        Console.WriteLine($"Total tokens used: {totalTokens}");
        Console.WriteLine($"Stopped by condition: {orchestrationResult?.StoppedByCondition ?? false}");
    }

    private static async Task DemoCustomCondition(OpenRouterClient client, ITool searchTool)
    {
        var request = new BetaResponsesRequest
        {
            Model = ExampleConfig.ModelName,
            Input = "Search for AI information. When you're done, say DONE.",
            Store = false,
            ServiceTier = "auto"
        };

        // Custom condition: stop when output contains "DONE"
        var doneCondition = StopConditions.Custom(steps =>
            steps.Any(s => s.Response.Output.Any(o => 
                o.Text?.Contains("DONE", StringComparison.OrdinalIgnoreCase) == true))
        );

        var result = client.CallModel(
            request,
            tools: new[] { searchTool },
            maxTurns: 10,
            stopConditions: new List<StopCondition> { doneCondition }
        );

        var orchestrationResult = await result.GetOrchestrationResultAsync();
        
        Console.WriteLine($"Steps executed: {orchestrationResult?.Steps.Count ?? 0}");
        Console.WriteLine($"Stopped by condition: {orchestrationResult?.StoppedByCondition ?? false}");
        
        var lastOutput = orchestrationResult?.Steps.LastOrDefault()?.Response.Output
            .FirstOrDefault(o => o.Type == "text")?.Text;
        Console.WriteLine($"Last output: {lastOutput}");
    }

    private static async Task DemoMultipleConditions(
        OpenRouterClient client,
        ITool searchTool,
        ITool calculatorTool)
    {
        var request = new BetaResponsesRequest
        {
            Model = ExampleConfig.ModelName,
            Input = "Do some calculations and searches",
            Store = false,
            ServiceTier = "auto"
        };

        // Stop when EITHER condition is met
        var combinedCondition = StopConditions.Any(
            StopConditions.StepCountIs(5),
            StopConditions.MaxTokensUsed(200),
            StopConditions.HasToolCall("search")
        );

        var result = client.CallModel(
            request,
            tools: new[] { searchTool, calculatorTool },
            maxTurns: 10,
            stopConditions: new List<StopCondition> { combinedCondition }
        );

        var orchestrationResult = await result.GetOrchestrationResultAsync();
        
        Console.WriteLine($"Steps: {orchestrationResult?.Steps.Count ?? 0}");
        Console.WriteLine($"Tokens: {orchestrationResult?.Steps.Sum(s => s.TotalTokens) ?? 0}");
        Console.WriteLine($"Search called: {orchestrationResult?.Steps.Any(s => s.ToolCalls.Any(tc => tc.Name == "search")) ?? false}");
        Console.WriteLine($"Stopped by condition: {orchestrationResult?.StoppedByCondition ?? false}");
    }

    private static async Task DemoAsyncCondition(OpenRouterClient client, ITool searchTool)
    {
        var request = new BetaResponsesRequest
        {
            Model = ExampleConfig.ModelName,
            Input = "Search for various topics",
            Store = false,
            ServiceTier = "auto"
        };

        // Async condition: check external service
        var asyncCondition = StopConditions.CustomAsync(async steps =>
        {
            // Simulate checking external service
            await Task.Delay(10);
            
            // Stop if we've executed more than 2 search calls
            var searchCount = steps.Sum(s => s.ToolCalls.Count(tc => tc.Name == "search"));
            return searchCount >= 2;
        });

        var result = client.CallModel(
            request,
            tools: new[] { searchTool },
            maxTurns: 10,
            stopConditions: new List<StopCondition> { asyncCondition }
        );

        var orchestrationResult = await result.GetOrchestrationResultAsync();
        var searchCount = orchestrationResult?.Steps.Sum(s => s.ToolCalls.Count(tc => tc.Name == "search")) ?? 0;
        
        Console.WriteLine($"Search calls: {searchCount}");
        Console.WriteLine($"Stopped by condition: {orchestrationResult?.StoppedByCondition ?? false}");
    }
}

// Tool input/output types
public class SearchInput
{
    public required string Query { get; init; }
}

public class SearchResult
{
    public required string Results { get; init; }
}

public class CalculatorInput
{
    public required string Expression { get; init; }
}

public class CalculatorResult
{
    public required double Result { get; init; }
}



