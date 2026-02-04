# Stop Conditions

Stop conditions provide fine-grained control over when multi-turn tool orchestration loops should terminate. They enable budget control, safety limits, semantic completion, and custom termination logic.

## Overview

Stop conditions are functions that receive the conversation history and return a boolean:
- **`true`** = STOP execution immediately
- **`false`** = CONTINUE to next step

They are evaluated after each step in the orchestration loop, allowing you to monitor progress and halt execution based on various criteria.

## Available Stop Conditions

### 1. StepCountIs

Stop after a specific number of conversation turns.

```csharp
public static StopCondition StepCountIs(int stepCount)
```

**Use Case**: Prevent infinite loops by limiting maximum turns.

**Example**:
```csharp
var condition = StopConditions.StepCountIs(5);
// Stops after 5 steps (steps.Count >= 5)
```

**When to use**:
- Always include as a safety limit
- Set based on expected conversation complexity
- Typical values: 5-10 for simple tasks, 20+ for complex workflows

---

### 2. HasToolCall

Stop when a specific tool is called in any step.

```csharp
public static StopCondition HasToolCall(string toolName)
```

**Use Case**: Semantic completion - stop when a "finalize", "complete", or "done" tool is called.

**Example**:
```csharp
var condition = StopConditions.HasToolCall("finalize");
// Stops when the 'finalize' tool is called
```

**When to use**:
- Implement semantic completion patterns
- Let the model decide when it's done
- Create workflow checkpoints

**Best Practice**: Create dedicated completion tools:
```csharp
var finalizeTool = Tool.Create(
    name: "finalize",
    description: "Call this when the task is complete",
    inputSchema: "{ ... }",
    execute: async (input) => "Task completed"
);

var condition = StopConditions.HasToolCall("finalize");
```

---

### 3. MaxTokensUsed

Stop when cumulative token usage exceeds a threshold.

```csharp
public static StopCondition MaxTokensUsed(int maxTokens)
```

**Use Case**: Budget control - prevent excessive token consumption.

**Example**:
```csharp
var condition = StopConditions.MaxTokensUsed(10000);
// Stops when total tokens exceed 10,000
```

**When to use**:
- Control costs in production
- Limit resource usage per conversation
- Prevent runaway token consumption

**How it works**:
- Sums `TotalTokens` from all steps
- Includes both input and output tokens
- Checks cumulative usage across all turns

---

### 4. MaxCost

Stop when cumulative cost exceeds a threshold (in USD).

```csharp
public static StopCondition MaxCost(double maxCostInDollars)
```

**Use Case**: Direct cost control based on OpenRouter pricing.

**Example**:
```csharp
var condition = StopConditions.MaxCost(0.50);
// Stops when total cost exceeds $0.50
```

**When to use**:
- Set hard cost limits per conversation
- Implement tiered pricing for users
- Prevent budget overruns

**How it works**:
- Uses `Usage.Cost` from OpenRouter responses
- Actual cost data from the API (not estimated)
- Sums across all steps

**Note**: Requires OpenRouter to return cost data in responses.

---

### 5. FinishReasonIs

Stop when a response has a specific finish reason/status.

```csharp
public static StopCondition FinishReasonIs(string reason)
```

**Use Case**: Stop on specific model behaviors or API states.

**Example**:
```csharp
var condition = StopConditions.FinishReasonIs("completed");
// Stops when status is "completed"
```

**Common finish reasons**:
- `"completed"` - Normal completion
- `"length"` - Context length limit reached
- `"stop"` - Stop sequence encountered
- `"error"` - Error occurred

**When to use**:
- Detect context length issues
- Handle specific termination conditions
- React to model-specific behaviors

---

### 6. Any (OR Logic)

Combine multiple conditions - stop when **ANY** condition is met.

```csharp
public static StopCondition Any(params StopCondition[] conditions)
```

**Use Case**: Multiple termination criteria with OR logic.

**Example**:
```csharp
var condition = StopConditions.Any(
    StopConditions.StepCountIs(10),
    StopConditions.MaxCost(1.00),
    StopConditions.HasToolCall("finalize")
);
// Stops when: step count reaches 10 OR cost exceeds $1 OR finalize is called
```

**When to use**:
- Combine safety limits with semantic completion
- Multiple budget controls (tokens AND cost)
- Flexible termination criteria

**Evaluation**:
- All conditions evaluated in parallel
- Returns true if ANY returns true
- Short-circuits on first true result

---

### 7. All (AND Logic)

Combine multiple conditions - stop only when **ALL** conditions are met.

```csharp
public static StopCondition All(params StopCondition[] conditions)
```

**Use Case**: Require multiple criteria before stopping.

**Example**:
```csharp
var condition = StopConditions.All(
    StopConditions.StepCountIs(3),
    StopConditions.HasToolCall("search")
);
// Stops only when: step count is 3 AND search was called
```

**When to use**:
- Ensure minimum steps before semantic completion
- Require multiple tools to be called
- Complex workflow checkpoints

---

### 8. Custom

Create custom stop conditions with synchronous logic.

```csharp
public static StopCondition Custom(Func<IReadOnlyList<StepResult>, bool> predicate)
```

**Use Case**: Custom logic based on step history.

**Example**:
```csharp
var condition = StopConditions.Custom(steps =>
{
    // Stop if any tool result contains "DONE"
    return steps.Any(step =>
        step.ToolResults.Any(result =>
            result.Result?.ToString()?.Contains("DONE") == true
        )
    );
});
```

**Advanced Examples**:

**Progressive tightening**:
```csharp
var condition = StopConditions.Custom(steps =>
{
    var expensiveToolUsed = steps.Any(s => 
        s.ToolCalls.Any(tc => tc.Name == "expensive_api"));
    
    // Tighter limit if expensive tool was used
    return expensiveToolUsed 
        ? steps.Count >= 3  
        : steps.Count >= 10;
});
```

**Content analysis**:
```csharp
var condition = StopConditions.Custom(steps =>
{
    // Stop if response contains completion phrase
    return steps.Any(s => 
        s.Response.Output.Any(o => 
            o.Text?.Contains("task completed", 
                StringComparison.OrdinalIgnoreCase) == true));
});
```

**Token accumulation**:
```csharp
var condition = StopConditions.Custom(steps =>
{
    // Stop if last step used >80% of total budget
    if (steps.Count == 0) return false;
    
    var totalTokens = steps.Sum(s => s.TotalTokens);
    var lastStepTokens = steps.Last().TotalTokens;
    
    return lastStepTokens > totalTokens * 0.8;
});
```

---

### 9. CustomAsync

Create custom stop conditions with asynchronous logic.

```csharp
public static StopCondition CustomAsync(Func<IReadOnlyList<StepResult>, Task<bool>> predicate)
```

**Use Case**: Async operations like external API calls, database queries, etc.

**Example**:
```csharp
var condition = StopConditions.CustomAsync(async steps =>
{
    // Check external service
    var shouldStop = await ExternalService.CheckLimitAsync();
    return shouldStop;
});
```

**Advanced Examples**:

**Rate limit checking**:
```csharp
var condition = StopConditions.CustomAsync(async steps =>
{
    // Check if we're approaching rate limits
    var usage = await RateLimitTracker.GetUsageAsync();
    return usage.RemainingRequests < 10;
});
```

**Database validation**:
```csharp
var condition = StopConditions.CustomAsync(async steps =>
{
    // Validate against database constraint
    var totalCost = steps.Sum(s => s.Cost);
    var userBudget = await db.Users.GetBudgetAsync(userId);
    return totalCost >= userBudget;
});
```

**Time-based limits**:
```csharp
var startTime = DateTime.UtcNow;
var condition = StopConditions.CustomAsync(async steps =>
{
    await Task.Delay(1); // Yield execution
    var elapsed = DateTime.UtcNow - startTime;
    return elapsed.TotalSeconds > 30; // 30 second limit
});
```

---

## StepResult API

Each step provides comprehensive information:

```csharp
public class StepResult
{
    // Response from the model
    public BetaResponsesResponse Response { get; }
    
    // Tool calls made in this step
    public List<FunctionToolCall> ToolCalls { get; }
    
    // Tool execution results
    public List<ToolExecutionResult> ToolResults { get; }
    
    // Usage information
    public ResponsesUsage? Usage { get; }
    public int TotalTokens { get; }
    public double Cost { get; }  // In USD
    
    // Completion information
    public string? FinishReason { get; }
}
```

## Common Patterns

### Comprehensive Budget Control

```csharp
var budgetConditions = StopConditions.Any(
    StopConditions.MaxTokensUsed(50000),   // Token limit
    StopConditions.MaxCost(5.00),          // Cost limit
    StopConditions.StepCountIs(20)          // Safety limit
);
```

### Semantic Completion with Safety

```csharp
var completionConditions = StopConditions.Any(
    StopConditions.HasToolCall("finalize"),  // Normal completion
    StopConditions.StepCountIs(15),          // Safety limit
    StopConditions.MaxCost(2.00)             // Budget limit
);
```

### Tiered Safety Limits

```csharp
var tieredConditions = StopConditions.Custom(steps =>
{
    var stepCount = steps.Count;
    var totalCost = steps.Sum(s => s.Cost);
    
    // Tier 1: Basic safety
    if (stepCount >= 50) return true;
    
    // Tier 2: Cost escalation
    if (stepCount >= 20 && totalCost > 1.00) return true;
    
    // Tier 3: Token efficiency check
    if (stepCount >= 10)
    {
        var avgTokens = steps.Average(s => s.TotalTokens);
        if (avgTokens > 5000) return true;
    }
    
    return false;
});
```

### Workflow Checkpoints

```csharp
var workflowConditions = StopConditions.All(
    StopConditions.HasToolCall("validate"),   // Must validate
    StopConditions.HasToolCall("process"),    // Must process
    StopConditions.HasToolCall("finalize")    // Must finalize
);
```

## Best Practices

### 1. Always Set a Maximum Step Count
```csharp
// ❌ Bad - no safety limit
var conditions = new[] { 
    StopConditions.HasToolCall("done") 
};

// ✅ Good - includes safety limit
var conditions = new[] {
    StopConditions.Any(
        StopConditions.HasToolCall("done"),
        StopConditions.StepCountIs(10)  // Safety limit
    )
};
```

### 2. Combine Budget Controls
```csharp
// ✅ Token AND cost limits
var conditions = new[] {
    StopConditions.Any(
        StopConditions.MaxTokensUsed(100000),
        StopConditions.MaxCost(5.00),
        StopConditions.StepCountIs(20)
    )
};
```

### 3. Use Semantic Completion
```csharp
// ✅ Let the model decide when it's done
var finalizeTool = Tool.Create(
    name: "task_complete",
    description: "Call this function when you have completed the task",
    ...
);

var conditions = new[] {
    StopConditions.Any(
        StopConditions.HasToolCall("task_complete"),
        StopConditions.StepCountIs(15)  // Fallback
    )
};
```

### 4. Monitor Step History
```csharp
var result = await client.CallModelAsync(...);

// Analyze why it stopped
if (result.StoppedByCondition)
{
    Console.WriteLine("Stopped by condition:");
    Console.WriteLine($"  Steps: {result.Steps.Count}");
    Console.WriteLine($"  Tokens: {result.Steps.Sum(s => s.TotalTokens)}");
    Console.WriteLine($"  Cost: ${result.Steps.Sum(s => s.Cost):F4}");
    Console.WriteLine($"  Tools: {string.Join(", ", 
        result.Steps.SelectMany(s => s.ToolCalls).Select(tc => tc.Name))}");
}
```

### 5. Test with Small Limits
```csharp
#if DEBUG
    var condition = StopConditions.StepCountIs(3);  // Quick testing
#else
    var condition = StopConditions.StepCountIs(20); // Production
#endif
```

## Integration with ModelResult

Stop conditions work seamlessly with the `ModelResult` API:

```csharp
var result = client.Beta.Responses.CallModel(
    request,
    tools: tools,
    stopConditions: new[]
    {
        StopConditions.Any(
            StopConditions.StepCountIs(10),
            StopConditions.HasToolCall("finalize")
        )
    }
);

// Get final text
var text = await result.GetTextAsync();

// Or get orchestration details
var orchestrationResult = await result.GetOrchestrationResultAsync();
Console.WriteLine($"Stopped by condition: {orchestrationResult.StoppedByCondition}");
Console.WriteLine($"Total steps: {orchestrationResult.Steps.Count}");
```

## Comparison with TypeScript SDK

The C# implementation matches the TypeScript SDK's stop condition system:

| Feature | TypeScript | C# | Notes |
|---------|-----------|-----|-------|
| `stepCountIs()` | ✅ | ✅ `StepCountIs()` | Identical |
| `hasToolCall()` | ✅ | ✅ `HasToolCall()` | Identical |
| `maxTokensUsed()` | ✅ | ✅ `MaxTokensUsed()` | Identical |
| `maxCost()` | ✅ | ✅ `MaxCost()` | Identical |
| `finishReasonIs()` | ✅ | ✅ `FinishReasonIs()` | Identical |
| OR logic | ✅ | ✅ `Any()` | Array in TS, params in C# |
| Custom conditions | ✅ | ✅ `Custom()` | Identical |
| Async custom | ✅ | ✅ `CustomAsync()` | Identical |
| AND logic | ❌ | ✅ `All()` | C# enhancement |
| Parallel evaluation | ✅ | ✅ | Both use `Task.WhenAll` |

## Related Documentation

- [Example19.StopConditions](../../Examples/Example19.StopConditions/) - Complete working examples
- [ToolOrchestrator.cs](../src/OpenRouter.SDK/Services/ToolOrchestrator.cs) - Implementation details
- [ModelResult.cs](../src/OpenRouter.SDK/Services/ModelResult.cs) - Integration with result patterns

## API Reference

Full implementation: [StopConditions.cs](../src/OpenRouter.SDK/Services/StopConditions.cs)

```csharp
public delegate Task<bool> StopCondition(IReadOnlyList<StepResult> steps);

public static class StopConditions
{
    public static StopCondition StepCountIs(int stepCount);
    public static StopCondition HasToolCall(string toolName);
    public static StopCondition MaxTokensUsed(int maxTokens);
    public static StopCondition MaxCost(double maxCostInDollars);
    public static StopCondition FinishReasonIs(string reason);
    public static StopCondition Any(params StopCondition[] conditions);
    public static StopCondition All(params StopCondition[] conditions);
    public static StopCondition Custom(Func<IReadOnlyList<StepResult>, bool> predicate);
    public static StopCondition CustomAsync(Func<IReadOnlyList<StepResult>, Task<bool>> predicate);
    public static Task<bool> IsStopConditionMetAsync(IReadOnlyList<StopCondition> conditions, IReadOnlyList<StepResult> steps);
}
```
