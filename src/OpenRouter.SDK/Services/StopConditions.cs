using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Represents the result of a single step in a multi-turn conversation.
/// Contains response data, tool calls, execution results, and usage metrics.
/// </summary>
public class StepResult
{
    /// <summary>
    /// The response from the model for this step
    /// </summary>
    public required BetaResponsesResponse Response { get; init; }

    /// <summary>
    /// Tool calls made in this step
    /// </summary>
    public required List<FunctionToolCall> ToolCalls { get; init; }

    /// <summary>
    /// Tool execution results for this step (if tools were executed)
    /// </summary>
    public required List<ToolExecutionResult> ToolResults { get; init; }

    /// <summary>
    /// Token usage for this step
    /// </summary>
    public ResponsesUsage? Usage => Response.Usage;

    /// <summary>
    /// Finish reason for this step
    /// </summary>
    public string? FinishReason => Response.Status;

    /// <summary>
    /// Total tokens used in this step
    /// </summary>
    public int TotalTokens => Usage?.TotalTokens ?? 0;

    /// <summary>
    /// Cost for this step in USD (if available in usage metadata)
    /// </summary>
    public double Cost => Usage?.Cost ?? 0.0;
}

/// <summary>
/// A condition function that determines whether to stop tool execution.
/// Returns true to STOP execution, false to CONTINUE.
/// </summary>
/// <param name="steps">All steps executed so far</param>
/// <returns>True to stop execution, false to continue</returns>
public delegate Task<bool> StopCondition(IReadOnlyList<StepResult> steps);

/// <summary>
/// Helper functions for creating stop conditions to control tool orchestration loops.
/// Stop conditions determine when multi-turn conversations should terminate.
/// All helpers return functions that return true to STOP execution, false to CONTINUE.
/// </summary>
/// <remarks>
/// Stop conditions are evaluated after each step in the orchestration loop.
/// Multiple conditions can be combined using Any() for OR logic or All() for AND logic.
/// 
/// Common patterns:
/// - Budget control: MaxTokensUsed(), MaxCost()
/// - Safety limits: StepCountIs() to prevent infinite loops
/// - Task completion: HasToolCall() for semantic completion tools
/// - Custom logic: Custom() or CustomAsync() for complex scenarios
/// 
/// Example usage:
/// <code>
/// var stopConditions = new[]
/// {
///     StopConditions.Any(
///         StopConditions.StepCountIs(10),      // Max 10 turns
///         StopConditions.MaxCost(1.00),         // Max $1 cost
///         StopConditions.HasToolCall("finalize") // Or when finalize is called
///     )
/// };
/// </code>
/// </remarks>
public static class StopConditions
{
    /// <summary>
    /// Stop condition that checks if step count equals or exceeds a specific number
    /// </summary>
    /// <param name="stepCount">The number of steps to allow before stopping</param>
    /// <returns>StopCondition that returns true when steps.Count >= stepCount</returns>
    /// <example>
    /// <code>
    /// var condition = StopConditions.StepCountIs(5); // Stop after 5 steps
    /// </code>
    /// </example>
    public static StopCondition StepCountIs(int stepCount)
    {
        return steps => Task.FromResult(steps.Count >= stepCount);
    }

    /// <summary>
    /// Stop condition that checks if any step contains a tool call with the given name
    /// </summary>
    /// <param name="toolName">The name of the tool to check for</param>
    /// <returns>StopCondition that returns true if the tool was called in any step</returns>
    /// <example>
    /// <code>
    /// var condition = StopConditions.HasToolCall("search"); // Stop when search tool is called
    /// </code>
    /// </example>
    public static StopCondition HasToolCall(string toolName)
    {
        return steps => Task.FromResult(
            steps.Any(step => step.ToolCalls.Any(call => call.Name == toolName))
        );
    }

    /// <summary>
    /// Stop when total token usage exceeds a threshold
    /// </summary>
    /// <param name="maxTokens">Maximum total tokens to allow</param>
    /// <returns>StopCondition that returns true when token usage exceeds threshold</returns>
    /// <example>
    /// <code>
    /// var condition = StopConditions.MaxTokensUsed(10000); // Stop when total tokens exceed 10,000
    /// </code>
    /// </example>
    public static StopCondition MaxTokensUsed(int maxTokens)
    {
        return steps => Task.FromResult(
            steps.Sum(step => step.TotalTokens) >= maxTokens
        );
    }

    /// <summary>
    /// Stop when total cost exceeds a threshold
    /// OpenRouter-specific helper using cost data
    /// </summary>
    /// <param name="maxCostInDollars">Maximum cost in dollars to allow</param>
    /// <returns>StopCondition that returns true when cost exceeds threshold</returns>
    /// <example>
    /// <code>
    /// var condition = StopConditions.MaxCost(0.50); // Stop when total cost exceeds $0.50
    /// </code>
    /// </example>
    public static StopCondition MaxCost(double maxCostInDollars)
    {
        return steps => Task.FromResult(
            steps.Sum(step => step.Cost) >= maxCostInDollars
        );
    }

    /// <summary>
    /// Stop when a specific finish reason is encountered
    /// </summary>
    /// <param name="reason">The finish reason to check for</param>
    /// <returns>StopCondition that returns true when finish reason matches</returns>
    /// <example>
    /// <code>
    /// var condition = StopConditions.FinishReasonIs("length"); // Stop when context length limit is hit
    /// </code>
    /// </example>
    public static StopCondition FinishReasonIs(string reason)
    {
        return steps => Task.FromResult(
            steps.Any(step => step.FinishReason == reason)
        );
    }

    /// <summary>
    /// Create a custom stop condition from a lambda
    /// </summary>
    /// <param name="predicate">Custom predicate function</param>
    /// <returns>StopCondition</returns>
    /// <example>
    /// <code>
    /// var condition = StopConditions.Custom(steps => 
    ///     steps.Any(s => s.Response.Output.Any(o => o.Text?.Contains("DONE") == true))
    /// );
    /// </code>
    /// </example>
    public static StopCondition Custom(Func<IReadOnlyList<StepResult>, bool> predicate)
    {
        return steps => Task.FromResult(predicate(steps));
    }

    /// <summary>
    /// Create a custom async stop condition from a lambda
    /// </summary>
    /// <param name="predicate">Custom async predicate function</param>
    /// <returns>StopCondition</returns>
    /// <example>
    /// <code>
    /// var condition = StopConditions.CustomAsync(async steps =>
    /// {
    ///     var shouldStop = await CheckExternalConditionAsync();
    ///     return shouldStop;
    /// });
    /// </code>
    /// </example>
    public static StopCondition CustomAsync(Func<IReadOnlyList<StepResult>, Task<bool>> predicate)
    {
        return steps => predicate(steps);
    }

    /// <summary>
    /// Evaluates an array of stop conditions.
    /// Returns true if ANY condition returns true (OR logic)
    /// </summary>
    /// <param name="conditions">Array of stop conditions to evaluate</param>
    /// <param name="steps">Steps to evaluate against</param>
    /// <returns>True if any condition is met, false otherwise</returns>
    public static async Task<bool> IsStopConditionMetAsync(
        IReadOnlyList<StopCondition> conditions,
        IReadOnlyList<StepResult> steps)
    {
        if (conditions == null || conditions.Count == 0)
        {
            return false;
        }

        // Evaluate all conditions in parallel
        var tasks = conditions.Select(condition => condition(steps)).ToList();
        var results = await Task.WhenAll(tasks);

        // Return true if ANY condition is true (OR logic)
        return results.Any(result => result);
    }

    /// <summary>
    /// Combine multiple stop conditions with OR logic
    /// Returns a single condition that stops when ANY of the conditions are met
    /// </summary>
    /// <param name="conditions">Conditions to combine</param>
    /// <returns>Combined stop condition</returns>
    /// <example>
    /// <code>
    /// var combined = StopConditions.Any(
    ///     StopConditions.StepCountIs(5),
    ///     StopConditions.HasToolCall("search"),
    ///     StopConditions.MaxTokensUsed(10000)
    /// );
    /// </code>
    /// </example>
    public static StopCondition Any(params StopCondition[] conditions)
    {
        return async steps => await IsStopConditionMetAsync(conditions, steps);
    }

    /// <summary>
    /// Combine multiple stop conditions with AND logic
    /// Returns a single condition that stops only when ALL conditions are met
    /// </summary>
    /// <param name="conditions">Conditions to combine</param>
    /// <returns>Combined stop condition</returns>
    /// <example>
    /// <code>
    /// var combined = StopConditions.All(
    ///     StopConditions.StepCountIs(3),
    ///     StopConditions.HasToolCall("search")
    /// );
    /// </code>
    /// </example>
    public static StopCondition All(params StopCondition[] conditions)
    {
        return async steps =>
        {
            var tasks = conditions.Select(condition => condition(steps)).ToList();
            var results = await Task.WhenAll(tasks);
            return results.All(result => result);
        };
    }
}
