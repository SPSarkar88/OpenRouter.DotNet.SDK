using OpenRouter.SDK;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;
using OpenRouter.Examples.EnvConfig;

var apiKey = ExampleConfig.ApiKey;
var modelName = ExampleConfig.ModelName;

var client = new OpenRouterClient(apiKey);

Console.WriteLine("========================================");
Console.WriteLine("OpenRouter SDK - Stop Conditions Examples");
Console.WriteLine("========================================\n");

// Define tools using modern Tool<TInput, TOutput> API
var calculatorTool = new Tool<CalculatorInput, CalculatorResult>(
    name: "calculate",
    description: "Perform a mathematical calculation",
    inputSchema: JsonSchemaBuilder.CreateObjectSchema(
        new Dictionary<string, object>
        {
            ["expression"] = JsonSchemaBuilder.String("The mathematical expression to evaluate (e.g., '2 + 2', '10 * 5')")
        },
        required: new List<string> { "expression" }
    ),
    executeFunc: async (input, context) =>
    {
        Console.WriteLine($"  🔧 Calculating: {input.Expression}");
        
        // Simple evaluation
        var result = input.Expression switch
        {
            "2 + 2" => 4.0,
            "10 * 5" => 50.0,
            "100 / 10" => 10.0,
            "15 - 3" => 12.0,
            _ => 0.0
        };
        
        await Task.Delay(100); // Simulate work
        return new CalculatorResult { Result = result };
    });

var searchTool = new Tool<SearchInput, SearchResult>(
    name: "search",
    description: "Search for information",
    inputSchema: JsonSchemaBuilder.CreateObjectSchema(
        new Dictionary<string, object>
        {
            ["query"] = JsonSchemaBuilder.String("The search query")
        },
        required: new List<string> { "query" }
    ),
    executeFunc: async (input, context) =>
    {
        Console.WriteLine($"  🔍 Searching: {input.Query}");
        await Task.Delay(100);
        return new SearchResult 
        { 
            Results = $"Search results for '{input.Query}': Found 5 relevant articles" 
        };
    });

var finalizeTool = new Tool<FinalizeInput, FinalizeResult>(
    name: "finalize",
    description: "Finalize the results and end the conversation",
    inputSchema: JsonSchemaBuilder.CreateObjectSchema(
        new Dictionary<string, object>
        {
            ["summary"] = JsonSchemaBuilder.String("The final summary")
        },
        required: new List<string> { "summary" }
    ),
    executeFunc: async (input, context) =>
    {
        Console.WriteLine($"  ✅ Finalizing: {input.Summary}");
        await Task.Delay(100);
        return new FinalizeResult { Status = "Complete", Summary = input.Summary };
    });

// Run all examples
await Example1_StepCountIs();
await Example2_HasToolCall();
await Example3_MaxTokensUsed();
await Example4_MaxCost();
await Example5_FinishReasonIs();
await Example6_MultipleConditionsAny();
await Example7_MultipleConditionsAll();
await Example8_CustomCondition();

Console.WriteLine("\n========================================");
Console.WriteLine("All examples completed!");
Console.WriteLine("========================================");

async Task Example1_StepCountIs()
{
    Console.WriteLine("\n📌 Example 1: StepCountIs - Stop after 3 steps");
    Console.WriteLine("─────────────────────────────────────────────────\n");

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "Calculate 2+2, then 10*5, then 100/10. Do all calculations."
    };

    var result = client.CallModel(
        request,
        tools: new[] { calculatorTool },
        stopConditions: new[] { StopConditions.StepCountIs(3) }
    );

    var orchestration = await result.GetOrchestrationResultAsync();

    Console.WriteLine($"\n✅ Stopped after {orchestration?.Steps.Count ?? 0} steps (limit was 3)");
    Console.WriteLine($"   Stopped by condition: {orchestration?.StoppedByCondition ?? false}");
    Console.WriteLine($"   Total tokens used: {orchestration?.Steps.Sum(s => s.TotalTokens) ?? 0}");
}

async Task Example2_HasToolCall()
{
    Console.WriteLine("\n📌 Example 2: HasToolCall - Stop when 'finalize' tool is called");
    Console.WriteLine("─────────────────────────────────────────────────────────────────\n");

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "Search for 'OpenRouter API', then finalize with a summary."
    };

    var result = client.CallModel(
        request,
        tools: new ITool[] { searchTool, finalizeTool },
        stopConditions: new[] { StopConditions.HasToolCall("finalize") }
    );

    var orchestration = await result.GetOrchestrationResultAsync();

    Console.WriteLine($"\n✅ Stopped when 'finalize' was called");
    Console.WriteLine($"   Total steps: {orchestration?.Steps.Count ?? 0}");
    Console.WriteLine($"   Stopped by condition: {orchestration?.StoppedByCondition ?? false}");
}

async Task Example3_MaxTokensUsed()
{
    Console.WriteLine("\n📌 Example 3: MaxTokensUsed - Stop when total tokens exceed 1000");
    Console.WriteLine("───────────────────────────────────────────────────────────────────\n");

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "Calculate multiple expressions: 2+2, 10*5, 100/10, and explain each step."
    };

    var result = client.CallModel(
        request,
        tools: new[] { calculatorTool },
        stopConditions: new[] { StopConditions.MaxTokensUsed(1000) }
    );

    var orchestration = await result.GetOrchestrationResultAsync();

    Console.WriteLine($"\n✅ Token budget enforced");
    Console.WriteLine($"   Total steps: {orchestration?.Steps.Count ?? 0}");
    Console.WriteLine($"   Total tokens: {orchestration?.Steps.Sum(s => s.TotalTokens) ?? 0}");
    Console.WriteLine($"   Stopped by condition: {orchestration?.StoppedByCondition ?? false}");
}

async Task Example4_MaxCost()
{
    Console.WriteLine("\n📌 Example 4: MaxCost - Stop when cost exceeds $0.01");
    Console.WriteLine("──────────────────────────────────────────────────────\n");

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "Search for AI, then ML, then DL, then provide details on each."
    };

    var result = client.CallModel(
        request,
        tools: new[] { searchTool },
        stopConditions: new[] { StopConditions.MaxCost(0.01) }
    );

    var orchestration = await result.GetOrchestrationResultAsync();

    Console.WriteLine($"\n✅ Cost limit enforced");
    Console.WriteLine($"   Total steps: {orchestration?.Steps.Count ?? 0}");
    Console.WriteLine($"   Total cost: ${orchestration?.Steps.Sum(s => s.Cost) ?? 0:F6}");
    Console.WriteLine($"   Stopped by condition: {orchestration?.StoppedByCondition ?? false}");
}

async Task Example5_FinishReasonIs()
{
    Console.WriteLine("\n📌 Example 5: FinishReasonIs - Stop on specific finish reason");
    Console.WriteLine("───────────────────────────────────────────────────────────────\n");

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "Calculate 2+2"
    };

    var result = client.CallModel(
        request,
        tools: new[] { calculatorTool },
        stopConditions: new[] { StopConditions.FinishReasonIs("stop") }
    );

    var orchestration = await result.GetOrchestrationResultAsync();

    Console.WriteLine($"\n✅ Finish reason detected");
    Console.WriteLine($"   Total steps: {orchestration?.Steps.Count ?? 0}");
    Console.WriteLine($"   Stopped by condition: {orchestration?.StoppedByCondition ?? false}");
}

async Task Example6_MultipleConditionsAny()
{
    Console.WriteLine("\n📌 Example 6: Multiple conditions with Any (OR logic)");
    Console.WriteLine("─────────────────────────────────────────────────────────\n");

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "Search for OpenRouter, then calculate 10*5, then search for SDK."
    };

    var result = client.CallModel(
        request,
        tools: new ITool[] { searchTool, calculatorTool },
        stopConditions: new[]
        {
            StopConditions.Any(
                StopConditions.StepCountIs(5),
                StopConditions.MaxTokensUsed(2000),
                StopConditions.HasToolCall("finalize")
            )
        }
    );

    var orchestration = await result.GetOrchestrationResultAsync();

    Console.WriteLine($"\n✅ One of the conditions was met");
    Console.WriteLine($"   Total steps: {orchestration?.Steps.Count ?? 0}");
    Console.WriteLine($"   Total tokens: {orchestration?.Steps.Sum(s => s.TotalTokens) ?? 0}");
    Console.WriteLine($"   Stopped by condition: {orchestration?.StoppedByCondition ?? false}");
}

async Task Example7_MultipleConditionsAll()
{
    Console.WriteLine("\n📌 Example 7: Multiple conditions with All (AND logic)");
    Console.WriteLine("──────────────────────────────────────────────────────────\n");

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "Calculate 2+2"
    };

    var result = client.CallModel(
        request,
        tools: new[] { calculatorTool },
        stopConditions: new[]
        {
            StopConditions.All(
                StopConditions.StepCountIs(2),
                StopConditions.HasToolCall("calculate")
            )
        }
    );

    var orchestration = await result.GetOrchestrationResultAsync();

    Console.WriteLine($"\n✅ All conditions were met");
    Console.WriteLine($"   Total steps: {orchestration?.Steps.Count ?? 0}");
    Console.WriteLine($"   Stopped by condition: {orchestration?.StoppedByCondition ?? false}");
}

async Task Example8_CustomCondition()
{
    Console.WriteLine("\n📌 Example 8: Custom stop condition");
    Console.WriteLine("──────────────────────────────────────────\n");

    // Custom condition: stop if calculator returns result > 10
    var customCondition = StopConditions.Custom(steps =>
    {
        // Check tool results in the most recent step
        var hasLargeResult = steps
            .SelectMany(s => s.ToolResults)
            .Where(r => r.ToolName == "calculate" && r.IsSuccess && r.Result != null)
            .Select(r => r.Result as CalculatorResult)
            .Where(cr => cr != null)
            .Any(cr => cr.Result > 10);

        if (hasLargeResult)
        {
            Console.WriteLine("  ⚠️  Custom condition triggered: Result > 10");
        }

        return hasLargeResult;
    });

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "Calculate 2+2, then 10*5"
    };

    var result = client.CallModel(
        request,
        tools: new[] { calculatorTool },
        stopConditions: new[] { customCondition }
    );

    var orchestration = await result.GetOrchestrationResultAsync();

    Console.WriteLine($"\n✅ Custom condition triggered!");
    Console.WriteLine($"   Total steps: {orchestration?.Steps.Count ?? 0}");
    Console.WriteLine($"   Stopped by condition: {orchestration?.StoppedByCondition ?? false}");
}

// Tool input/output models
public record CalculatorInput
{
    public string Expression { get; set; } = "";
}

public record CalculatorResult
{
    public double Result { get; set; }
}

public record SearchInput
{
    public string Query { get; set; } = "";
}

public record SearchResult
{
    public string Results { get; set; } = "";
}

public record FinalizeInput
{
    public string Summary { get; set; } = "";
}

public record FinalizeResult
{
    public string Status { get; set; } = "";
    public string Summary { get; set; } = "";
}
