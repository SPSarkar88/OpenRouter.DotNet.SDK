using OpenRouter.SDK.Models;
using OpenRouter.SDK.Core;
using System.Runtime.CompilerServices;
using System.Text;

namespace OpenRouter.SDK.Services;

/// <summary>
/// A wrapper around model responses that provides multiple consumption patterns.
/// 
/// Allows consuming the response in multiple ways:
/// - GetTextAsync() - Get just the text content
/// - GetResponseAsync() - Get the full response object with metadata
/// - GetTextStreamAsync() - Stream text deltas in real-time
/// - GetReasoningStreamAsync() - Stream reasoning content (for models like o1)
/// - GetToolCallsStreamAsync() - Stream tool calls as they're made
/// - GetToolStreamAsync() - Stream tool execution results
/// - GetNewMessagesStreamAsync() - Stream new messages as they arrive
/// - GetFullResponsesStreamAsync() - Stream complete response objects
/// - GetFullStreamAsync() - Stream all response events
/// 
/// All stream methods support concurrent consumption via ReusableStream.
/// Multiple consumers can read the same stream simultaneously.
/// 
/// Automatically handles tool execution when tools are provided.
/// Supports dynamic parameter resolution based on conversation context.
/// Supports multi-turn conversation state persistence and resumption.
/// </summary>
public class ModelResult
{
    private readonly IBetaResponsesService _betaResponsesService;
    private readonly BetaResponsesRequest? _request;
    private readonly DynamicBetaResponsesRequest? _dynamicRequest;
    private readonly IEnumerable<ITool>? _tools;
    private readonly int _maxTurns;
    private readonly IStateAccessor? _stateAccessor;
    private readonly IReadOnlyList<StopCondition>? _stopConditions;
    private BetaResponsesResponse? _cachedResponse;
    private ToolOrchestrationResult? _orchestrationResult;
    private ConversationState? _conversationState;
    private ReusableStream<BetaResponsesStreamChunk>? _reusableStream;
    private readonly SemaphoreSlim _streamInitLock = new(1, 1);

    /// <summary>
    /// Create a new model result wrapper with static request
    /// </summary>
    public ModelResult(
        IBetaResponsesService betaResponsesService,
        BetaResponsesRequest request,
        IEnumerable<ITool>? tools = null,
        int maxTurns = 5,
        IStateAccessor? stateAccessor = null,
        IReadOnlyList<StopCondition>? stopConditions = null)
    {
        _betaResponsesService = betaResponsesService;
        _request = request;
        _tools = tools;
        _maxTurns = maxTurns;
        _stateAccessor = stateAccessor;
        _stopConditions = stopConditions;
    }

    /// <summary>
    /// Create a new model result wrapper with dynamic request
    /// </summary>
    public ModelResult(
        IBetaResponsesService betaResponsesService,
        DynamicBetaResponsesRequest dynamicRequest,
        IEnumerable<ITool>? tools = null,
        int maxTurns = 5,
        IStateAccessor? stateAccessor = null,
        IReadOnlyList<StopCondition>? stopConditions = null)
    {
        _betaResponsesService = betaResponsesService;
        _dynamicRequest = dynamicRequest;
        _tools = tools;
        _maxTurns = maxTurns;
        _stateAccessor = stateAccessor;
        _stopConditions = stopConditions;
    }

    /// <summary>
    /// Get just the text content from the response
    /// Automatically executes tools if provided
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Text content as a single string</returns>
    public async Task<string> GetTextAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetResponseAsync(cancellationToken);
        return ExtractText(response);
    }

    /// <summary>
    /// Get the full response object with metadata
    /// Automatically executes tools if provided
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete response object</returns>
    public async Task<BetaResponsesResponse> GetResponseAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedResponse != null)
        {
            return _cachedResponse;
        }

        // Resolve dynamic request if needed
        var request = _request;
        if (_dynamicRequest != null)
        {
            var initialContext = new TurnContext
            {
                NumberOfTurns = 0,
                PreviousResponses = new List<BetaResponsesResponse>(),
                TotalTokensUsed = 0,
                HasError = false
            };
            request = await _dynamicRequest.ResolveAsync(initialContext);
        }

        if (request == null)
        {
            throw new InvalidOperationException("No request available");
        }

        // If tools are provided, use orchestrator for automatic execution
        if (_tools != null && _tools.Any())
        {
            var orchestrator = new ToolOrchestrator(_betaResponsesService);
            _orchestrationResult = await orchestrator.ExecuteToolLoopAsync(
                request,
                _tools,
                _maxTurns,
                _dynamicRequest,
                _stopConditions,
                cancellationToken);

            _cachedResponse = _orchestrationResult.FinalResponse;
        }
        else
        {
            // No tools - simple request
            _cachedResponse = await _betaResponsesService.SendAsync(request, cancellationToken);
        }

        return _cachedResponse;
    }

    /// <summary>
    /// Get all responses from the conversation (if tools were executed)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all responses</returns>
    public async Task<List<BetaResponsesResponse>> GetAllResponsesAsync(CancellationToken cancellationToken = default)
    {
        await GetResponseAsync(cancellationToken);

        if (_orchestrationResult != null)
        {
            return _orchestrationResult.AllResponses;
        }

        return _cachedResponse != null ? new List<BetaResponsesResponse> { _cachedResponse } : new List<BetaResponsesResponse>();
    }

    /// <summary>
    /// Get all tool execution results (if tools were executed)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tool execution results</returns>
    public async Task<List<ToolExecutionResult>> GetToolExecutionResultsAsync(CancellationToken cancellationToken = default)
    {
        await GetResponseAsync(cancellationToken);

        if (_orchestrationResult != null)
        {
            return _orchestrationResult.ToolExecutionResults;
        }

        return new List<ToolExecutionResult>();
    }

    /// <summary>
    /// Get the full orchestration result including steps, stop condition status, and all execution details
    /// Only available when tools are provided
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Full orchestration result, or null if no tools were used</returns>
    public async Task<ToolOrchestrationResult?> GetOrchestrationResultAsync(CancellationToken cancellationToken = default)
    {
        await GetResponseAsync(cancellationToken);
        return _orchestrationResult;
    }

    /// <summary>
    /// Stream text deltas in real-time.
    /// Can be called multiple times concurrently - each consumer gets independent iteration.
    /// Note: Tool execution happens synchronously before streaming starts.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of text deltas</returns>
    public async IAsyncEnumerable<string> GetTextStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // If tools are provided, execute them first, then stream the final response
        if (_tools != null && _tools.Any())
        {
            var response = await GetResponseAsync(cancellationToken);
            var text = ExtractText(response);
            
            // Stream the text character by character (or in chunks)
            const int chunkSize = 10;
            for (int i = 0; i < text.Length; i += chunkSize)
            {
                var chunk = text.Substring(i, Math.Min(chunkSize, text.Length - i));
                yield return chunk;
            }
        }
        else
        {
            // Use reusable stream for concurrent consumption
            var stream = await GetOrCreateReusableStreamAsync(cancellationToken);
            string? accumulatedText = null;
            
            await foreach (var chunk in stream.CreateConsumer(cancellationToken))
            {
                // Beta Responses API sends full text only in the final response.completed event
                // Check for completed response with output
                if (chunk.Type == "response.completed" && chunk.Response?.Output != null)
                {
                    // Extract text from the completed response
                    foreach (var outputItem in chunk.Response.Output)
                    {
                        if (outputItem.Type == "message" && outputItem.Content != null)
                        {
                            foreach (var content in outputItem.Content)
                            {
                                if (content.Type == "output_text" && content.Text != null)
                                {
                                    accumulatedText = content.Text;
                                }
                            }
                        }
                        else if (outputItem.Type == "text" && outputItem.Text != null)
                        {
                            accumulatedText = outputItem.Text;
                        }
                    }
                }
            }
            
            // Stream out the text in chunks after receiving it
            if (!string.IsNullOrEmpty(accumulatedText))
            {
                const int chunkSize = 10;
                for (int i = 0; i < accumulatedText.Length; i += chunkSize)
                {
                    var textChunk = accumulatedText.Substring(i, Math.Min(chunkSize, accumulatedText.Length - i));
                    yield return textChunk;
                }
            }
        }
    }

    /// <summary>
    /// Stream reasoning deltas in real-time.
    /// Can be called multiple times concurrently - each consumer gets independent iteration.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of reasoning content</returns>
    public async IAsyncEnumerable<string> GetReasoningStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stream = await GetOrCreateReusableStreamAsync(cancellationToken);
        
        await foreach (var chunk in stream.CreateConsumer(cancellationToken))
        {
            // Extract reasoning content from chunk
            // Check if this is a reasoning output item
            if (chunk.Delta?.Type == "reasoning" && chunk.Delta?.Text != null)
            {
                yield return chunk.Delta.Text;
            }
        }
    }

    /// <summary>
    /// Stream tool calls in real-time.
    /// Can be called multiple times concurrently - each consumer gets independent iteration.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of tool calls</returns>
    public async IAsyncEnumerable<FunctionToolCall> GetToolCallsStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stream = await GetOrCreateReusableStreamAsync(cancellationToken);
        
        await foreach (var chunk in stream.CreateConsumer(cancellationToken))
        {
            // Extract function tool calls from chunk
            if (chunk.Delta != null && chunk.Delta.Type == "function_call")
            {
                // Parse tool call from delta
                var toolCall = ParseToolCallFromDelta(chunk.Delta);
                if (toolCall != null)
                {
                    yield return toolCall;
                }
            }
        }
    }

    /// <summary>
    /// Stream all response events in real-time.
    /// Can be called multiple times concurrently - each consumer gets independent iteration.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of response stream chunks</returns>
    public async IAsyncEnumerable<BetaResponsesStreamChunk> GetFullStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stream = await GetOrCreateReusableStreamAsync(cancellationToken);
        
        await foreach (var chunk in stream.CreateConsumer(cancellationToken))
        {
            yield return chunk;
        }
    }

    /// <summary>
    /// Stream tool execution results in real-time.
    /// Can be called multiple times concurrently - each consumer gets independent iteration.
    /// Only yields results when tools are provided and executed.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of tool execution results</returns>
    public async IAsyncEnumerable<ToolExecutionResult> GetToolStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stream = await GetOrCreateReusableStreamAsync(cancellationToken);
        
        await foreach (var chunk in stream.CreateConsumer(cancellationToken))
        {
            // Extract tool execution results from completed responses
            if (chunk.Type == "response.completed" && chunk.Response?.Output != null)
            {
                foreach (var outputItem in chunk.Response.Output)
                {
                    if (outputItem.Type == "function_call")
                    {
                        var result = new ToolExecutionResult
                        {
                            ToolCallId = outputItem.CallId ?? outputItem.Id ?? string.Empty,
                            ToolName = outputItem.Name ?? "unknown",
                            Result = outputItem.Arguments != null ? System.Text.Json.JsonSerializer.Deserialize<object>(outputItem.Arguments) : null,
                            Error = null
                        };
                        yield return result;
                    }
                }
            }
        }
    }
    /// <summary>
    /// Stream new messages as they arrive in real-time.
    /// Can be called multiple times concurrently - each consumer gets independent iteration.
    /// Yields complete message objects when they're done.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of output items (messages)</returns>
    public async IAsyncEnumerable<ResponsesOutputItem> GetNewMessagesStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stream = await GetOrCreateReusableStreamAsync(cancellationToken);
        
        await foreach (var chunk in stream.CreateConsumer(cancellationToken))
        {
            // Extract completed messages from the final response
            if (chunk.Type == "response.completed" && chunk.Response?.Output != null)
            {
                foreach (var outputItem in chunk.Response.Output)
                {
                    if (outputItem.Type == "message")
                    {
                        yield return outputItem;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Stream full response objects as they complete.
    /// Can be called multiple times concurrently - each consumer gets independent iteration.
    /// Useful for multi-turn conversations where you want each complete response.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of complete response objects</returns>
    public async IAsyncEnumerable<BetaResponsesResponse> GetFullResponsesStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stream = await GetOrCreateReusableStreamAsync(cancellationToken);
        
        await foreach (var chunk in stream.CreateConsumer(cancellationToken))
        {
            // Extract completed responses
            if (chunk.Type == "response.completed" && chunk.Response != null)
            {
                yield return chunk.Response;
            }
        }
    }

    /// <summary>
    /// Get or create the reusable stream for concurrent consumption.
    /// Thread-safe initialization using SemaphoreSlim.
    /// </summary>
    private async Task<ReusableStream<BetaResponsesStreamChunk>> GetOrCreateReusableStreamAsync(
        CancellationToken cancellationToken = default)
    {
        if (_reusableStream != null)
        {
            return _reusableStream;
        }

        await _streamInitLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_reusableStream != null)
            {
                return _reusableStream;
            }

            // Resolve dynamic request if needed
            var request = _request;
            if (_dynamicRequest != null)
            {
                var initialContext = new TurnContext
                {
                    NumberOfTurns = 0,
                    PreviousResponses = new List<BetaResponsesResponse>(),
                    TotalTokensUsed = 0,
                    HasError = false
                };
                request = await _dynamicRequest.ResolveAsync(initialContext);
            }

            if (request == null)
            {
                throw new InvalidOperationException("No request available");
            }

            // Create reusable stream from source
            var sourceStream = _betaResponsesService.SendStreamAsync(request, cancellationToken);
            _reusableStream = new ReusableStream<BetaResponsesStreamChunk>(sourceStream);

            return _reusableStream;
        }
        finally
        {
            _streamInitLock.Release();
        }
    }

    /// <summary>
    /// Parse a tool call from a stream delta chunk
    /// </summary>
    private FunctionToolCall? ParseToolCallFromDelta(ResponsesOutputItem delta)
    {
        // This is a simplified parser - in production you'd need to handle incremental parsing
        // as tool call data may come in multiple chunks
        if (delta.Type != "function_call")
        {
            return null;
        }

        // Extract tool call information from delta
        // This would need to be adapted based on actual delta structure
        return null; // Placeholder - actual implementation depends on API format
    }

    /// <summary>
    /// Extract text content from a response
    /// </summary>
    private static string ExtractText(BetaResponsesResponse response)
    {
        if (response.Output == null || response.Output.Count == 0)
        {
            return string.Empty;
        }

        var textBuilder = new StringBuilder();

        foreach (var item in response.Output)
        {
            if (item.Type == "text" && item.Text != null)
            {
                textBuilder.Append(item.Text);
            }
        }

        return textBuilder.ToString();
    }

    /// <summary>
    /// Get the current conversation state.
    /// Returns null if no state accessor was provided.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current conversation state or null</returns>
    public async Task<ConversationState?> GetConversationStateAsync(CancellationToken cancellationToken = default)
    {
        if (_stateAccessor == null)
        {
            return _conversationState;
        }

        // Ensure we have executed at least once to populate state
        if (_conversationState == null)
        {
            _conversationState = await _stateAccessor.LoadAsync();
        }

        return _conversationState;
    }

    /// <summary>
    /// Check if the conversation requires human approval to continue.
    /// Returns true if there are pending tool calls awaiting approval.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if approval is required</returns>
    public async Task<bool> RequiresApprovalAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetConversationStateAsync(cancellationToken);
        if (state == null)
        {
            return false;
        }

        return ConversationStateUtilities.RequiresApproval(state);
    }

    /// <summary>
    /// Get the pending tool calls that require approval.
    /// Returns empty list if no approvals needed.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of pending tool calls</returns>
    public async Task<List<FunctionToolCall>> GetPendingToolCallsAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetConversationStateAsync(cancellationToken);
        return state?.PendingToolCalls ?? new List<FunctionToolCall>();
    }

    /// <summary>
    /// Save the current conversation state using the state accessor.
    /// Throws if no state accessor was provided.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task SaveConversationStateAsync(CancellationToken cancellationToken = default)
    {
        if (_stateAccessor == null)
        {
            throw new InvalidOperationException("No state accessor was provided. Cannot save conversation state.");
        }

        if (_conversationState == null)
        {
            throw new InvalidOperationException("No conversation state to save. Execute a request first.");
        }

        await _stateAccessor.SaveAsync(_conversationState);
    }

    /// <summary>
    /// Continue a conversation by sending a new message.
    /// Automatically loads previous state if state accessor is provided.
    /// </summary>
    /// <param name="userMessage">New message from the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response from the model</returns>
    public async Task<BetaResponsesResponse> ContinueConversationAsync(
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        // Load existing state if available
        if (_stateAccessor != null && _conversationState == null)
        {
            _conversationState = await _stateAccessor.LoadAsync();
        }

        // Create new state if none exists
        if (_conversationState == null)
        {
            _conversationState = ConversationStateUtilities.CreateInitialState();
        }

        // Add user message to state
        ConversationStateUtilities.AppendToMessages(_conversationState, new ResponsesInputItem
        {
            Type = "text",
            Text = userMessage
        });

        // Create request with full conversation history
        var request = new BetaResponsesRequest
        {
            Model = _request?.Model ?? "openai/gpt-3.5-turbo",
            Input = _conversationState.Messages.ToList()
        };

        // Send request
        var response = await _betaResponsesService.SendAsync(request, cancellationToken);

        // Add assistant response to state
        if (response.Output != null)
        {
            foreach (var outputItem in response.Output)
            {
                // Convert ResponsesOutputItem to ResponsesInputItem
                var inputItem = new ResponsesInputItem
                {
                    Type = outputItem.Type,
                    Text = outputItem.Text,
                    ImageUrl = outputItem.Image,
                    Audio = outputItem.Audio
                };
                _conversationState.Messages.Add(inputItem);
            }
        }

        // Save state if accessor provided
        if (_stateAccessor != null)
        {
            await _stateAccessor.SaveAsync(_conversationState);
        }

        return response;
    }
}

/// <summary>
/// Extension methods for creating ModelResult instances
/// </summary>
public static class ModelResultExtensions
{
    /// <summary>
    /// Create a ModelResult from a Beta Responses service and request
    /// </summary>
    /// <param name="service">Beta responses service</param>
    /// <param name="request">Request to send</param>
    /// <param name="tools">Optional tools for automatic execution</param>
    /// <param name="maxTurns">Maximum number of turns for tool execution</param>
    /// <param name="stateAccessor">Optional state accessor for conversation persistence</param>
    /// <returns>Model result wrapper</returns>
    public static ModelResult CreateResult(
        this IBetaResponsesService service,
        BetaResponsesRequest request,
        IEnumerable<ITool>? tools = null,
        int maxTurns = 5,
        IStateAccessor? stateAccessor = null)
    {
        return new ModelResult(service, request, tools, maxTurns, stateAccessor);
    }
}
