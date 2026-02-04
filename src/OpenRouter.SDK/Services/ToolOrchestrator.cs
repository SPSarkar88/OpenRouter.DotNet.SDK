using OpenRouter.SDK.Models;
using System.Text.Json;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Result of the tool orchestration loop
/// Contains the final response, all responses, and tool execution results
/// </summary>
public class ToolOrchestrationResult
{
    /// <summary>
    /// Final response from the model after tool execution completes
    /// </summary>
    public required BetaResponsesResponse FinalResponse { get; init; }

    /// <summary>
    /// All responses from the model during the conversation
    /// </summary>
    public required List<BetaResponsesResponse> AllResponses { get; init; }

    /// <summary>
    /// All tool execution results
    /// </summary>
    public required List<ToolExecutionResult> ToolExecutionResults { get; init; }

    /// <summary>
    /// Final conversation input after all tool executions
    /// </summary>
    public required List<ResponsesInputItem> ConversationInput { get; init; }

    /// <summary>
    /// All steps executed during the orchestration
    /// </summary>
    public required List<StepResult> Steps { get; init; }

    /// <summary>
    /// Whether execution stopped due to a stop condition being met
    /// </summary>
    public required bool StoppedByCondition { get; init; }
}

/// <summary>
/// Service for orchestrating multi-turn conversations with automatic tool execution
/// Manages the loop of: request -> tool calls -> execute -> send results -> repeat
/// </summary>
public class ToolOrchestrator
{
    private readonly IBetaResponsesService _betaResponsesService;

    /// <summary>
    /// Create a new tool orchestrator
    /// </summary>
    /// <param name="betaResponsesService">Beta responses service for API calls</param>
    public ToolOrchestrator(IBetaResponsesService betaResponsesService)
    {
        _betaResponsesService = betaResponsesService;
    }

    /// <summary>
    /// Convert ITool instances to ResponsesFunctionTool format for Beta Responses API
    /// </summary>
    private static List<ResponsesFunctionTool> ConvertToResponsesFunctionTools(IEnumerable<ITool> tools)
    {
        return tools.Select(tool =>
        {
            // Parse the input schema to a Dictionary
            var parametersDict = JsonSerializer.Deserialize<Dictionary<string, object?>>(tool.InputSchema.GetRawText())
                ?? new Dictionary<string, object?>();

            return new ResponsesFunctionTool
            {
                Name = tool.Name,
                Description = tool.Description,
                Parameters = parametersDict
            };
        }).ToList();
    }

    /// <summary>
    /// Execute the tool loop for automatic tool execution
    /// </summary>
    /// <param name="initialRequest">Initial request to send to the model</param>
    /// <param name="tools">Tools available for execution</param>
    /// <param name="maxTurns">Maximum number of turns (default 5)</param>
    /// <param name="dynamicRequest">Optional dynamic request for async parameter resolution</param>
    /// <param name="stopConditions">Optional stop conditions to check after each step</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Orchestration result with final response and all execution data</returns>
    public async Task<ToolOrchestrationResult> ExecuteToolLoopAsync(
        BetaResponsesRequest initialRequest,
        IEnumerable<ITool> tools,
        int maxTurns = 5,
        DynamicBetaResponsesRequest? dynamicRequest = null,
        IReadOnlyList<StopCondition>? stopConditions = null,
        CancellationToken cancellationToken = default)
    {
        var toolsList = tools.ToList();
        var allResponses = new List<BetaResponsesResponse>();
        var toolExecutionResults = new List<ToolExecutionResult>();
        var steps = new List<StepResult>();
        var stoppedByCondition = false;
        
        // Convert Input to a workable list format
        var conversationInput = new List<ResponsesInputItem>();
        if (initialRequest.Input != null)
        {
            // Just store the initial input for now - we'll need to track messages as ResponsesInputItem for tool results
            // The API accepts the input as-is, but we need a list to add tool results
        }
        var currentRound = 0;

        // Convert tools to ResponsesFunctionTool format for Beta Responses API
        var responsesFunctionTools = ConvertToResponsesFunctionTools(toolsList);

        // Resolve dynamic parameters for the initial request (turn 0)
        var resolvedInitialRequest = initialRequest;
        if (dynamicRequest != null)
        {
            var initialContext = new TurnContext
            {
                NumberOfTurns = 0,
                PreviousResponses = new List<BetaResponsesResponse>(),
                TotalTokensUsed = 0,
                HasError = false
            };
            resolvedInitialRequest = await dynamicRequest.ResolveAsync(initialContext);
        }

        // Create initial request with tools - use the original Input as-is
        var currentRequest = new BetaResponsesRequest
        {
            Model = resolvedInitialRequest.Model,
            Input = resolvedInitialRequest.Input, // Use original Input format
            Instructions = resolvedInitialRequest.Instructions,
            Tools = responsesFunctionTools, // Include tools in ResponsesFunctionTool format
            ToolChoice = resolvedInitialRequest.ToolChoice, // Include tool choice setting
            Temperature = resolvedInitialRequest.Temperature,
            MaxOutputTokens = resolvedInitialRequest.MaxOutputTokens,
            TopP = resolvedInitialRequest.TopP,
            TopK = resolvedInitialRequest.TopK,
            FrequencyPenalty = resolvedInitialRequest.FrequencyPenalty,
            PresencePenalty = resolvedInitialRequest.PresencePenalty,
            Reasoning = resolvedInitialRequest.Reasoning,
            Metadata = resolvedInitialRequest.Metadata,
            Store = resolvedInitialRequest.Store ?? false, // Required for /responses endpoint
            ServiceTier = resolvedInitialRequest.ServiceTier ?? "auto" // Required for /responses endpoint
        };

        // Initial request
        var currentResponse = await _betaResponsesService.SendAsync(currentRequest, cancellationToken);
        allResponses.Add(currentResponse);

        // Create initial step
        var initialToolCalls = ExtractToolCalls(currentResponse);
        var initialStep = new StepResult
        {
            Response = currentResponse,
            ToolCalls = initialToolCalls,
            ToolResults = new List<ToolExecutionResult>()
        };
        steps.Add(initialStep);

        // Check stop conditions after initial response
        if (stopConditions != null && await StopConditions.IsStopConditionMetAsync(stopConditions, steps))
        {
            stoppedByCondition = true;
            return new ToolOrchestrationResult
            {
                FinalResponse = currentResponse,
                AllResponses = allResponses,
                ToolExecutionResults = toolExecutionResults,
                ConversationInput = conversationInput,
                Steps = steps,
                StoppedByCondition = stoppedByCondition
            };
        }

        // Loop until no more tool calls or max turns reached
        while (currentRound < maxTurns && ResponseHasToolCalls(currentResponse))
        {
            currentRound++;

            // Extract tool calls from response
            var toolCalls = ExtractToolCalls(currentResponse);

            if (toolCalls.Count == 0)
            {
                break;
            }

            // Check if any tools have execute functions
            var hasExecutableTools = toolCalls.Any(tc =>
            {
                var tool = ToolExecutor.FindToolByName(toolsList, tc.Name);
                return tool?.HasExecuteFunction == true;
            });

            // If no executable tools, return (manual execution mode)
            if (!hasExecutableTools)
            {
                break;
            }

            // Execute all tool calls in parallel
            var toolCallTasks = toolCalls.Select(async toolCall =>
            {
                var tool = ToolExecutor.FindToolByName(toolsList, toolCall.Name);

                if (tool == null)
                {
                    return new ToolExecutionResult
                    {
                        ToolCallId = toolCall.Id,
                        ToolName = toolCall.Name,
                        Result = null,
                        Error = new InvalidOperationException($"Tool '{toolCall.Name}' not found in tool definitions")
                    };
                }

                if (!tool.HasExecuteFunction)
                {
                    return null; // Skip manual tools
                }

                // Build turn context with enhanced information
                var turnContext = new TurnContext
                {
                    ToolCall = toolCall,
                    NumberOfTurns = currentRound,
                    TurnRequest = currentRequest,
                    PreviousResponses = allResponses,
                    TotalTokensUsed = allResponses.Sum(r => r.Usage?.TotalTokens ?? 0),
                    HasError = toolExecutionResults.Any(r => !r.IsSuccess)
                };

                // Execute the tool
                return await ToolExecutor.ExecuteToolAsync(tool, toolCall, turnContext);
            }).ToList();

            // Wait for all tool executions to complete
            var settledResults = await Task.WhenAll(toolCallTasks);

            // Process results and filter out nulls (manual tools)
            var roundResults = settledResults.Where(r => r != null).Cast<ToolExecutionResult>().ToList();
            toolExecutionResults.AddRange(roundResults);

            // Build tool result outputs for next request  
            var toolResultOutputs = roundResults.Select(result => new ResponsesInputItem
            {
                Type = "text", // Use text type for tool results since ResponsesInputItem doesn't have call_id/output
                Text = result.IsSuccess
                    ? $"Tool {result.ToolName} result: {System.Text.Json.JsonSerializer.Serialize(result.Result)}"
                    : $"Tool {result.ToolName} error: {result.Error?.Message ?? "Unknown error"}"
            }).ToList();

            // NOTE: Beta Responses API handles conversation tracking server-side
            // We don't need to manually build conversation history like Chat API
            /*
            // Add model's response output to conversation
            if (currentResponse.Output != null)
            {
                foreach (var outputItem in currentResponse.Output)
                {
                    // Convert output items to input items
                    var inputItem = new ResponsesInputItem
                    {
                        Type = outputItem.Type,
                        Text = outputItem.Text
                    };
                    conversationInput.Add(inputItem);
                }
            }

            // Add tool results to conversation
            conversationInput.AddRange(toolResultOutputs);
            */


            // Resolve dynamic parameters for this turn if dynamic request is provided
            BetaResponsesRequest baseRequestForTurn;
            if (dynamicRequest != null)
            {
                var turnContext = new TurnContext
                {
                    NumberOfTurns = currentRound,
                    TurnRequest = currentRequest,
                    PreviousResponses = allResponses,
                    TotalTokensUsed = allResponses.Sum(r => r.Usage?.TotalTokens ?? 0),
                    HasError = toolExecutionResults.Any(r => !r.IsSuccess)
                };
                baseRequestForTurn = await dynamicRequest.ResolveAsync(turnContext);
            }
            else
            {
                baseRequestForTurn = currentRequest;
            }

            // Send next turn request
            currentRequest = new BetaResponsesRequest
            {
                Model = baseRequestForTurn.Model,
                Input = baseRequestForTurn.Input, // Use original Input - API handles conversation tracking
                Instructions = baseRequestForTurn.Instructions,
                Tools = responsesFunctionTools, // Include tools in ResponsesFunctionTool format
                ToolChoice = baseRequestForTurn.ToolChoice, // Include tool choice setting
                Temperature = baseRequestForTurn.Temperature,
                MaxOutputTokens = baseRequestForTurn.MaxOutputTokens,
                TopP = baseRequestForTurn.TopP,
                TopK = baseRequestForTurn.TopK,
                FrequencyPenalty = baseRequestForTurn.FrequencyPenalty,
                PresencePenalty = baseRequestForTurn.PresencePenalty,
                Reasoning = baseRequestForTurn.Reasoning,
                Metadata = currentRequest.Metadata,
                Store = baseRequestForTurn.Store ?? false, // Required for /responses endpoint
                ServiceTier = baseRequestForTurn.ServiceTier ?? "auto" // Required for /responses endpoint
            };

            currentResponse = await _betaResponsesService.SendAsync(currentRequest, cancellationToken);
            allResponses.Add(currentResponse);

            // Create step for this turn
            var turnToolCalls = ExtractToolCalls(currentResponse);
            var step = new StepResult
            {
                Response = currentResponse,
                ToolCalls = turnToolCalls,
                ToolResults = roundResults
            };
            steps.Add(step);

            // Check stop conditions
            if (stopConditions != null && await StopConditions.IsStopConditionMetAsync(stopConditions, steps))
            {
                stoppedByCondition = true;
                break;
            }
        }

        return new ToolOrchestrationResult
        {
            FinalResponse = currentResponse,
            AllResponses = allResponses,
            ToolExecutionResults = toolExecutionResults,
            ConversationInput = conversationInput,
            Steps = steps,
            StoppedByCondition = stoppedByCondition
        };
    }

    /// <summary>
    /// Check if response has tool calls
    /// </summary>
    private static bool ResponseHasToolCalls(BetaResponsesResponse response)
    {
        if (response.Output == null || response.Output.Count == 0)
        {
            return false;
        }

        return response.Output.Any(item => item.Type == "function_call");
    }

    /// <summary>
    /// Extract tool calls from response
    /// </summary>
    private static List<FunctionToolCall> ExtractToolCalls(BetaResponsesResponse response)
    {
        var toolCalls = new List<FunctionToolCall>();

        if (response.Output == null)
        {
            return toolCalls;
        }

        foreach (var item in response.Output)
        {
            if (item.Type == "function_call")
            {
                // Try the new flat format first (direct properties on the item)
                if (!string.IsNullOrEmpty(item.Name) && item.Arguments != null)
                {
                    var callId = item.CallId ?? item.Id ?? "";
                    
                    toolCalls.Add(new FunctionToolCall
                    {
                        Id = callId,
                        Name = item.Name,
                        Arguments = item.Arguments,
                        Type = "function_call",
                        CallId = callId
                    });
                }
                // Fall back to nested format if available
                else if (item.FunctionCall != null)
                {
                    try
                    {
                        var fcJson = System.Text.Json.JsonSerializer.Serialize(item.FunctionCall);
                        using var doc = System.Text.Json.JsonDocument.Parse(fcJson);
                        var root = doc.RootElement;

                        var id = root.TryGetProperty("call_id", out var callIdProp) ? callIdProp.GetString() : null;
                        var name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                        var arguments = root.TryGetProperty("arguments", out var argsProp) ? argsProp.GetString() : null;

                        if (id != null && name != null && arguments != null)
                        {
                            toolCalls.Add(new FunctionToolCall
                            {
                                Id = id,
                                Name = name,
                                Arguments = arguments,
                                Type = "function_call",
                                CallId = id
                            });
                        }
                    }
                    catch
                    {
                        // Skip malformed function calls
                    }
                }
            }
        }

        return toolCalls;
    }
}
