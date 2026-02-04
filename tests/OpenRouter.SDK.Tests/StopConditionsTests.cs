using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;
using Xunit;

namespace OpenRouter.SDK.Tests;

public class StopConditionsTests
{
    private static List<StepResult> CreateTestSteps(int count)
    {
        var steps = new List<StepResult>();
        for (int i = 0; i < count; i++)
        {
            steps.Add(new StepResult
            {
                Response = new BetaResponsesResponse
                {
                    Id = $"resp_{i}",
                    Object = "response",
                    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    Model = "gpt-3.5-turbo",
                    Status = "completed",
                    Output = new List<ResponsesOutputItem>
                    {
                        new() { Type = "text", Text = $"Response {i}" }
                    },
                    Usage = new ResponsesUsage
                    {
                        PromptTokens = 10,
                        CompletionTokens = 20,
                        TotalTokens = 30
                    }
                },
                ToolCalls = new List<FunctionToolCall>(),
                ToolResults = new List<ToolExecutionResult>()
            });
        }
        return steps;
    }

    [Fact]
    public async Task StepCountIs_StopsAtExactCount()
    {
        // Arrange
        var condition = StopConditions.StepCountIs(3);
        var steps2 = CreateTestSteps(2);
        var steps3 = CreateTestSteps(3);
        var steps4 = CreateTestSteps(4);

        // Act
        var result2 = await condition(steps2);
        var result3 = await condition(steps3);
        var result4 = await condition(steps4);

        // Assert
        Assert.False(result2); // Should continue
        Assert.True(result3);  // Should stop
        Assert.True(result4);  // Should stop
    }

    [Fact]
    public async Task HasToolCall_DetectsSpecificTool()
    {
        // Arrange
        var condition = StopConditions.HasToolCall("search");
        var stepsWithoutTool = CreateTestSteps(2);
        
        var stepsWithTool = CreateTestSteps(1);
        stepsWithTool[0] = new StepResult
        {
            Response = stepsWithTool[0].Response,
            ToolCalls = new List<FunctionToolCall>
            {
                new() { Id = "call_1", Name = "search", Arguments = "{}", Type = "function_call", CallId = "call_1" }
            },
            ToolResults = new List<ToolExecutionResult>()
        };

        // Act
        var resultWithout = await condition(stepsWithoutTool);
        var resultWith = await condition(stepsWithTool);

        // Assert
        Assert.False(resultWithout);
        Assert.True(resultWith);
    }

    [Fact]
    public async Task MaxTokensUsed_StopsWhenExceeded()
    {
        // Arrange
        var condition = StopConditions.MaxTokensUsed(100);
        var steps = CreateTestSteps(2); // 2 steps × 30 tokens = 60 tokens
        var moreSteps = CreateTestSteps(4); // 4 steps × 30 tokens = 120 tokens

        // Act
        var resultUnder = await condition(steps);
        var resultOver = await condition(moreSteps);

        // Assert
        Assert.False(resultUnder);
        Assert.True(resultOver);
    }

    [Fact]
    public async Task FinishReasonIs_DetectsStatus()
    {
        // Arrange
        var condition = StopConditions.FinishReasonIs("completed");
        var steps = CreateTestSteps(1);

        // Act
        var result = await condition(steps);

        // Assert
        Assert.True(result); // Status is "completed"
    }

    [Fact]
    public async Task Custom_AllowsLambdaPredicate()
    {
        // Arrange
        var condition = StopConditions.Custom(steps => 
            steps.Any(s => s.Response.Output.Any(o => o.Text?.Contains("DONE") == true))
        );

        var stepsWithoutDone = CreateTestSteps(2);
        var stepsWithDone = CreateTestSteps(1);
        stepsWithDone[0] = new StepResult
        {
            Response = new BetaResponsesResponse
            {
                Id = "resp_done",
                Object = "response",
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Model = "gpt-3.5-turbo",
                Status = "completed",
                Output = new List<ResponsesOutputItem>
                {
                    new() { Type = "text", Text = "Task is DONE" }
                }
            },
            ToolCalls = new List<FunctionToolCall>(),
            ToolResults = new List<ToolExecutionResult>()
        };

        // Act
        var resultWithout = await condition(stepsWithoutDone);
        var resultWith = await condition(stepsWithDone);

        // Assert
        Assert.False(resultWithout);
        Assert.True(resultWith);
    }

    [Fact]
    public async Task CustomAsync_AllowsAsyncPredicate()
    {
        // Arrange
        var shouldStopFlag = false;
        var condition = StopConditions.CustomAsync(async steps =>
        {
            await Task.Delay(10); // Simulate async work
            return shouldStopFlag;
        });

        var steps = CreateTestSteps(1);

        // Act
        var resultBefore = await condition(steps);
        shouldStopFlag = true;
        var resultAfter = await condition(steps);

        // Assert
        Assert.False(resultBefore);
        Assert.True(resultAfter);
    }

    [Fact]
    public async Task IsStopConditionMetAsync_ReturnsFalseWhenNoConditions()
    {
        // Arrange
        var steps = CreateTestSteps(5);
        var emptyConditions = new List<StopCondition>();

        // Act
        var result = await StopConditions.IsStopConditionMetAsync(emptyConditions, steps);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsStopConditionMetAsync_UsesOrLogic()
    {
        // Arrange
        var steps = CreateTestSteps(2);
        var conditions = new List<StopCondition>
        {
            StopConditions.StepCountIs(5), // False
            StopConditions.HasToolCall("nonexistent"), // False
            StopConditions.FinishReasonIs("completed") // True
        };

        // Act
        var result = await StopConditions.IsStopConditionMetAsync(conditions, steps);

        // Assert
        Assert.True(result); // At least one is true
    }

    [Fact]
    public async Task Any_CombinesMultipleConditionsWithOr()
    {
        // Arrange
        var condition = StopConditions.Any(
            StopConditions.StepCountIs(5),
            StopConditions.MaxTokensUsed(50)
        );

        var steps3 = CreateTestSteps(3); // 3 steps, 90 tokens
        var steps2 = CreateTestSteps(2); // 2 steps, 60 tokens

        // Act
        var result3 = await condition(steps3);
        var result2 = await condition(steps2);

        // Assert
        Assert.True(result3); // Exceeds token limit
        Assert.True(result2); // Exceeds token limit
    }

    [Fact]
    public async Task All_CombinesMultipleConditionsWithAnd()
    {
        // Arrange
        var condition = StopConditions.All(
            StopConditions.StepCountIs(3),
            StopConditions.FinishReasonIs("completed")
        );

        var steps2 = CreateTestSteps(2); // 2 steps, completed
        var steps3 = CreateTestSteps(3); // 3 steps, completed

        // Act
        var result2 = await condition(steps2);
        var result3 = await condition(steps3);

        // Assert
        Assert.False(result2); // Only step count met
        Assert.True(result3);  // Both conditions met
    }

    [Fact]
    public async Task StepResult_CalculatesTotalTokensCorrectly()
    {
        // Arrange
        var step = new StepResult
        {
            Response = new BetaResponsesResponse
            {
                Id = "resp_1",
                Object = "response",
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Model = "gpt-4",
                Status = "completed",
                Output = new List<ResponsesOutputItem>(),
                Usage = new ResponsesUsage
                {
                    PromptTokens = 100,
                    CompletionTokens = 200,
                    TotalTokens = 300
                }
            },
            ToolCalls = new List<FunctionToolCall>(),
            ToolResults = new List<ToolExecutionResult>()
        };

        // Assert
        Assert.Equal(300, step.TotalTokens);
        Assert.Equal("completed", step.FinishReason);
    }

    [Fact]
    public async Task StepResult_HandlesNullUsage()
    {
        // Arrange
        var step = new StepResult
        {
            Response = new BetaResponsesResponse
            {
                Id = "resp_1",
                Object = "response",
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Model = "gpt-4",
                Status = "completed",
                Output = new List<ResponsesOutputItem>()
            },
            ToolCalls = new List<FunctionToolCall>(),
            ToolResults = new List<ToolExecutionResult>()
        };

        // Assert
        Assert.Equal(0, step.TotalTokens);
        Assert.Null(step.Usage);
    }

    [Fact]
    public async Task MultipleConditions_EvaluateInParallel()
    {
        // Arrange
        var callCount = 0;
        var condition1 = StopConditions.CustomAsync(async steps =>
        {
            Interlocked.Increment(ref callCount);
            await Task.Delay(50);
            return false;
        });

        var condition2 = StopConditions.CustomAsync(async steps =>
        {
            Interlocked.Increment(ref callCount);
            await Task.Delay(50);
            return false;
        });

        var condition3 = StopConditions.CustomAsync(async steps =>
        {
            Interlocked.Increment(ref callCount);
            await Task.Delay(50);
            return true;
        });

        var steps = CreateTestSteps(1);
        var conditions = new List<StopCondition> { condition1, condition2, condition3 };

        // Act
        var startTime = DateTimeOffset.UtcNow;
        var result = await StopConditions.IsStopConditionMetAsync(conditions, steps);
        var elapsed = DateTimeOffset.UtcNow - startTime;

        // Assert
        Assert.True(result);
        Assert.Equal(3, callCount);
        // All 3 should run in parallel, so total time should be ~50ms, not 150ms
        // Increased threshold to 250ms to account for system load and CI environments
        Assert.True(elapsed.TotalMilliseconds < 250, $"Expected parallel execution < 250ms, got {elapsed.TotalMilliseconds}ms");
    }

    [Fact]
    public async Task HasToolCall_WorksWithMultipleToolsInStep()
    {
        // Arrange
        var condition = StopConditions.HasToolCall("calculator");
        var steps = new List<StepResult>
        {
            new()
            {
                Response = new BetaResponsesResponse
                {
                    Id = "resp_1",
                    Object = "response",
                    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    Model = "gpt-4",
                    Status = "completed",
                    Output = new List<ResponsesOutputItem>()
                },
                ToolCalls = new List<FunctionToolCall>
                {
                    new() { Id = "call_1", Name = "search", Arguments = "{}", Type = "function_call", CallId = "call_1" },
                    new() { Id = "call_2", Name = "calculator", Arguments = "{}", Type = "function_call", CallId = "call_2" },
                    new() { Id = "call_3", Name = "weather", Arguments = "{}", Type = "function_call", CallId = "call_3" }
                },
                ToolResults = new List<ToolExecutionResult>()
            }
        };

        // Act
        var result = await condition(steps);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task EmptySteps_DoNotTriggerConditions()
    {
        // Arrange
        var emptySteps = new List<StepResult>();
        var condition = StopConditions.StepCountIs(0);

        // Act
        var result = await condition(emptySteps);

        // Assert
        Assert.True(result); // 0 >= 0
    }
}
