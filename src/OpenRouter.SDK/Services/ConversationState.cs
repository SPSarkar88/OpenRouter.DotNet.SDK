using OpenRouter.SDK.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Services;

/// <summary>
/// JSON converter for ConversationStatus enum that uses snake_case
/// </summary>
internal class ConversationStatusJsonConverter : JsonConverter<ConversationStatus>
{
    public override ConversationStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "complete" => ConversationStatus.Complete,
            "interrupted" => ConversationStatus.Interrupted,
            "awaiting_approval" => ConversationStatus.AwaitingApproval,
            "in_progress" => ConversationStatus.InProgress,
            _ => throw new JsonException($"Unknown conversation status: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, ConversationStatus value, JsonSerializerOptions options)
    {
        var status = value switch
        {
            ConversationStatus.Complete => "complete",
            ConversationStatus.Interrupted => "interrupted",
            ConversationStatus.AwaitingApproval => "awaiting_approval",
            ConversationStatus.InProgress => "in_progress",
            _ => throw new JsonException($"Unknown conversation status: {value}")
        };
        writer.WriteStringValue(status);
    }
}

/// <summary>
/// Status of a conversation
/// </summary>
[JsonConverter(typeof(ConversationStatusJsonConverter))]
public enum ConversationStatus
{
    /// <summary>
    /// Conversation has finished successfully
    /// </summary>
    Complete,
    
    /// <summary>
    /// Conversation was interrupted
    /// </summary>
    Interrupted,
    
    /// <summary>
    /// Conversation is awaiting human approval for tool execution
    /// </summary>
    AwaitingApproval,
    
    /// <summary>
    /// Conversation is in progress
    /// </summary>
    InProgress
}

/// <summary>
/// State for multi-turn conversations with persistence support
/// Tracks message history, tool calls, and conversation metadata
/// </summary>
public class ConversationState
{
    /// <summary>
    /// Unique identifier for this conversation
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    /// <summary>
    /// Full message history for the conversation
    /// </summary>
    [JsonPropertyName("messages")]
    public List<ResponsesInputItem> Messages { get; set; } = new();
    
    /// <summary>
    /// Previous response ID for chaining (OpenRouter server-side optimization)
    /// </summary>
    [JsonPropertyName("previous_response_id")]
    public string? PreviousResponseId { get; set; }
    
    /// <summary>
    /// Tool calls awaiting human approval
    /// </summary>
    [JsonPropertyName("pending_tool_calls")]
    public List<FunctionToolCall>? PendingToolCalls { get; set; }
    
    /// <summary>
    /// Tool results executed but not yet sent to the model
    /// </summary>
    [JsonPropertyName("unsent_tool_results")]
    public List<ToolExecutionResult>? UnsentToolResults { get; set; }
    
    /// <summary>
    /// Partial response data captured during interruption
    /// </summary>
    [JsonPropertyName("partial_response")]
    public BetaResponsesResponse? PartialResponse { get; set; }
    
    /// <summary>
    /// Signal from a new request to interrupt this conversation
    /// </summary>
    [JsonPropertyName("interrupted_by")]
    public string? InterruptedBy { get; set; }
    
    /// <summary>
    /// Current status of the conversation
    /// </summary>
    [JsonPropertyName("status")]
    public ConversationStatus Status { get; set; } = ConversationStatus.InProgress;
    
    /// <summary>
    /// Creation timestamp (Unix milliseconds)
    /// </summary>
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }
    
    /// <summary>
    /// Last update timestamp (Unix milliseconds)
    /// </summary>
    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }
    
    /// <summary>
    /// Custom metadata that can be passed through the conversation
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// State accessor interface for loading and saving conversation state
/// Enables any storage backend (memory, Redis, database, etc.)
/// </summary>
public interface IStateAccessor
{
    /// <summary>
    /// Load the current conversation state, or null if none exists
    /// </summary>
    Task<ConversationState?> LoadAsync();
    
    /// <summary>
    /// Save the conversation state
    /// </summary>
    Task SaveAsync(ConversationState state);
}

/// <summary>
/// In-memory implementation of IStateAccessor for simple use cases
/// </summary>
public class InMemoryStateAccessor : IStateAccessor
{
    private ConversationState? _state;
    
    /// <summary>
    /// Load the conversation state from memory
    /// </summary>
    public Task<ConversationState?> LoadAsync()
    {
        return Task.FromResult(_state);
    }
    
    /// <summary>
    /// Save the conversation state to memory
    /// </summary>
    public Task SaveAsync(ConversationState state)
    {
        _state = state;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Dictionary-based state accessor for managing multiple conversations
/// Maps conversation IDs to their states
/// </summary>
public class DictionaryStateAccessor : IStateAccessor
{
    private readonly Dictionary<string, ConversationState> _states;
    private readonly string _conversationId;
    
    /// <summary>
    /// Creates a new dictionary state accessor for a specific conversation
    /// </summary>
    /// <param name="conversationId">The ID of the conversation to manage</param>
    public DictionaryStateAccessor(string conversationId)
    {
        _states = new Dictionary<string, ConversationState>();
        _conversationId = conversationId;
    }
    
    /// <summary>
    /// Creates a new dictionary state accessor with a shared state dictionary
    /// </summary>
    /// <param name="sharedStates">Shared dictionary for storing multiple conversations</param>
    /// <param name="conversationId">The ID of the conversation to manage</param>
    private DictionaryStateAccessor(Dictionary<string, ConversationState> sharedStates, string conversationId)
    {
        _states = sharedStates;
        _conversationId = conversationId;
    }
    
    /// <summary>
    /// Create a new state accessor that shares state dictionary with other accessors
    /// </summary>
    public static DictionaryStateAccessor Create(Dictionary<string, ConversationState> sharedStates, string conversationId)
    {
        return new DictionaryStateAccessor(sharedStates, conversationId);
    }
    
    /// <summary>
    /// Load the conversation state for this conversation ID
    /// </summary>
    public Task<ConversationState?> LoadAsync()
    {
        _states.TryGetValue(_conversationId, out var state);
        return Task.FromResult(state);
    }
    
    /// <summary>
    /// Save the conversation state for this conversation ID
    /// </summary>
    public Task SaveAsync(ConversationState state)
    {
        _states[_conversationId] = state;
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Get all conversation states
    /// </summary>
    public IReadOnlyDictionary<string, ConversationState> GetAllStates()
    {
        return _states;
    }
    
    /// <summary>
    /// Clear all conversation states
    /// </summary>
    public void Clear()
    {
        _states.Clear();
    }
}

/// <summary>
/// Utility methods for working with conversation state
/// </summary>
public static class ConversationStateUtilities
{
    /// <summary>
    /// Generate a unique ID for a conversation
    /// </summary>
    public static string GenerateConversationId()
    {
        return $"conv_{Guid.NewGuid()}";
    }
    
    /// <summary>
    /// Create an initial conversation state
    /// </summary>
    /// <param name="id">Optional custom ID, generates one if not provided</param>
    public static ConversationState CreateInitialState(string? id = null)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return new ConversationState
        {
            Id = id ?? GenerateConversationId(),
            Messages = new List<ResponsesInputItem>(),
            Status = ConversationStatus.InProgress,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
    
    /// <summary>
    /// Update a conversation state with new values
    /// Automatically updates the UpdatedAt timestamp
    /// </summary>
    public static ConversationState UpdateState(ConversationState state, Action<ConversationState> updater)
    {
        updater(state);
        state.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return state;
    }
    
    /// <summary>
    /// Append new items to the message history
    /// </summary>
    public static void AppendToMessages(ConversationState state, params ResponsesInputItem[] newItems)
    {
        state.Messages.AddRange(newItems);
        state.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
    
    /// <summary>
    /// Append new items to the message history
    /// </summary>
    public static void AppendToMessages(ConversationState state, IEnumerable<ResponsesInputItem> newItems)
    {
        state.Messages.AddRange(newItems);
        state.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
    
    /// <summary>
    /// Check if the conversation is complete
    /// </summary>
    public static bool IsComplete(ConversationState state)
    {
        return state.Status == ConversationStatus.Complete;
    }
    
    /// <summary>
    /// Check if the conversation requires approval
    /// </summary>
    public static bool RequiresApproval(ConversationState state)
    {
        return state.Status == ConversationStatus.AwaitingApproval 
            || (state.PendingToolCalls?.Count ?? 0) > 0;
    }
    
    /// <summary>
    /// Check if the conversation was interrupted
    /// </summary>
    public static bool IsInterrupted(ConversationState state)
    {
        return state.Status == ConversationStatus.Interrupted;
    }
    
    /// <summary>
    /// Mark the conversation as complete
    /// </summary>
    public static void MarkComplete(ConversationState state)
    {
        state.Status = ConversationStatus.Complete;
        state.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
    
    /// <summary>
    /// Mark the conversation as interrupted
    /// </summary>
    public static void MarkInterrupted(ConversationState state, string? reason = null)
    {
        state.Status = ConversationStatus.Interrupted;
        state.InterruptedBy = reason;
        state.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
    
    /// <summary>
    /// Mark the conversation as awaiting approval
    /// </summary>
    public static void MarkAwaitingApproval(ConversationState state, List<FunctionToolCall> pendingCalls)
    {
        state.Status = ConversationStatus.AwaitingApproval;
        state.PendingToolCalls = pendingCalls;
        state.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
