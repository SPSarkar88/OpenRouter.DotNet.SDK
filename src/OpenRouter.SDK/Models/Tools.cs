using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

/// <summary>
/// Tool type enum for function tools
/// </summary>
public enum ToolType
{
    /// <summary>
    /// Function tool type
    /// </summary>
    Function
}

/// <summary>
/// Turn context passed to tool execute functions and parameter resolvers
/// Contains information about the current conversation state
/// </summary>
public class TurnContext
{
    /// <summary>
    /// The specific tool call being executed (only available during tool execution)
    /// </summary>
    public FunctionToolCall? ToolCall { get; init; }

    /// <summary>
    /// Number of tool execution turns so far (1-indexed: first turn = 1, 0 = initial request)
    /// </summary>
    public int NumberOfTurns { get; init; }

    /// <summary>
    /// The full request being sent to the API (only available during tool execution)
    /// </summary>
    public BetaResponsesRequest? TurnRequest { get; init; }

    /// <summary>
    /// All responses from previous turns in this conversation
    /// </summary>
    public List<BetaResponsesResponse>? PreviousResponses { get; init; }

    /// <summary>
    /// Total tokens used across all turns so far
    /// </summary>
    public int? TotalTokensUsed { get; init; }

    /// <summary>
    /// Whether any errors have occurred in previous turns
    /// </summary>
    public bool HasError { get; init; }

    /// <summary>
    /// Custom metadata that can be passed through the conversation
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Function tool call from API response (used in tool orchestration)
/// </summary>
public class FunctionToolCall
{
    /// <summary>
    /// Unique identifier for the tool call
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Name of the tool to call
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Arguments for the tool call as JSON
    /// </summary>
    [JsonPropertyName("arguments")]
    public required string Arguments { get; init; }

    /// <summary>
    /// Type of the tool call (always "function_call")
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = "function_call";

    /// <summary>
    /// Call ID (same as Id)
    /// </summary>
    [JsonPropertyName("call_id")]
    public string? CallId { get; init; }

    /// <summary>
    /// Status of the tool call
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }
}

/// <summary>
/// Result of tool execution
/// </summary>
public class ToolExecutionResult
{
    /// <summary>
    /// Tool call ID
    /// </summary>
    public required string ToolCallId { get; init; }

    /// <summary>
    /// Tool name
    /// </summary>
    public required string ToolName { get; init; }

    /// <summary>
    /// Result of the tool execution (can be any JSON-serializable object)
    /// </summary>
    public object? Result { get; init; }

    /// <summary>
    /// Error if tool execution failed
    /// </summary>
    public Exception? Error { get; init; }

    /// <summary>
    /// Whether the execution was successful
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => Error == null;
}

/// <summary>
/// Base interface for all tools
/// </summary>
public interface ITool
{
    /// <summary>
    /// Tool type (always Function)
    /// </summary>
    ToolType Type { get; }

    /// <summary>
    /// Name of the tool
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description of what the tool does
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// JSON Schema for the tool's input parameters
    /// </summary>
    JsonElement InputSchema { get; }

    /// <summary>
    /// Whether this tool has an execute function
    /// </summary>
    bool HasExecuteFunction { get; }

    /// <summary>
    /// Whether this tool requires approval before execution
    /// </summary>
    bool RequiresApproval { get; }

    /// <summary>
    /// Convert tool to API format for sending to OpenRouter
    /// </summary>
    object ToApiFormat();
}

/// <summary>
/// Tool with executable function
/// </summary>
/// <typeparam name="TInput">Type of the input parameters</typeparam>
/// <typeparam name="TOutput">Type of the output result</typeparam>
public interface ITool<TInput, TOutput> : ITool where TInput : class
{
    /// <summary>
    /// Execute the tool with the given parameters
    /// </summary>
    /// <param name="input">Input parameters (deserialized from JSON)</param>
    /// <param name="context">Turn context with conversation state</param>
    /// <returns>Tool execution result</returns>
    Task<TOutput> ExecuteAsync(TInput input, TurnContext? context = null);
}

/// <summary>
/// Builder for creating tools with fluent API
/// </summary>
/// <typeparam name="TInput">Type of the input parameters</typeparam>
/// <typeparam name="TOutput">Type of the output result</typeparam>
public class Tool<TInput, TOutput> : ITool<TInput, TOutput> where TInput : class
{
    private readonly Func<TInput, TurnContext?, Task<TOutput>> _executeFunc;
    private readonly JsonElement _inputSchema;
    
    /// <inheritdoc/>
    public ToolType Type => ToolType.Function;
    
    /// <inheritdoc/>
    public string Name { get; init; }
    
    /// <inheritdoc/>
    public string? Description { get; init; }
    
    /// <inheritdoc/>
    public JsonElement InputSchema => _inputSchema;
    
    /// <inheritdoc/>
    public bool HasExecuteFunction => true;
    
    /// <inheritdoc/>
    public bool RequiresApproval { get; init; }

    /// <summary>
    /// Create a new tool
    /// </summary>
    /// <param name="name">Name of the tool</param>
    /// <param name="description">Description of the tool</param>
    /// <param name="inputSchema">JSON Schema for input parameters</param>
    /// <param name="executeFunc">Function to execute the tool</param>
    /// <param name="requiresApproval">Whether the tool requires approval before execution</param>
    public Tool(
        string name,
        string? description,
        JsonElement inputSchema,
        Func<TInput, TurnContext?, Task<TOutput>> executeFunc,
        bool requiresApproval = false)
    {
        Name = name;
        Description = description;
        _inputSchema = inputSchema;
        _executeFunc = executeFunc;
        RequiresApproval = requiresApproval;
    }

    /// <inheritdoc/>
    public async Task<TOutput> ExecuteAsync(TInput input, TurnContext? context = null)
    {
        return await _executeFunc(input, context);
    }

    /// <inheritdoc/>
    public object ToApiFormat()
    {
        // Parse JsonElement to Dictionary for API compatibility
        var parametersDict = JsonSerializer.Deserialize<Dictionary<string, object?>>(_inputSchema.GetRawText()) 
            ?? new Dictionary<string, object?>();

        return new
        {
            type = "function",
            name = Name,
            description = Description,
            parameters = parametersDict
        };
    }
}

/// <summary>
/// Manual tool without execute function - requires manual handling by developer
/// </summary>
public class ManualTool : ITool
{
    private readonly JsonElement _inputSchema;
    
    /// <inheritdoc/>
    public ToolType Type => ToolType.Function;
    
    /// <inheritdoc/>
    public string Name { get; init; }
    
    /// <inheritdoc/>
    public string? Description { get; init; }
    
    /// <inheritdoc/>
    public JsonElement InputSchema => _inputSchema;
    
    /// <inheritdoc/>
    public bool HasExecuteFunction => false;
    
    /// <inheritdoc/>
    public bool RequiresApproval { get; init; }

    /// <summary>
    /// Create a new manual tool
    /// </summary>
    /// <param name="name">Name of the tool</param>
    /// <param name="description">Description of the tool</param>
    /// <param name="inputSchema">JSON Schema for input parameters</param>
    /// <param name="requiresApproval">Whether the tool requires approval before execution</param>
    public ManualTool(
        string name,
        string? description,
        JsonElement inputSchema,
        bool requiresApproval = false)
    {
        Name = name;
        Description = description;
        _inputSchema = inputSchema;
        RequiresApproval = requiresApproval;
    }

    /// <inheritdoc/>
    public object ToApiFormat()
    {
        // Parse JsonElement to Dictionary for API compatibility
        var parametersDict = JsonSerializer.Deserialize<Dictionary<string, object?>>(_inputSchema.GetRawText()) 
            ?? new Dictionary<string, object?>();

        return new
        {
            type = "function",
            name = Name,
            description = Description,
            parameters = parametersDict
        };
    }
}

/// <summary>
/// Helper class for building JSON schemas for tool parameters
/// Provides a fluent API for creating JSON Schema objects
/// </summary>
public static class JsonSchemaBuilder
{
    /// <summary>
    /// Create a JSON schema for an object with properties
    /// </summary>
    /// <param name="properties">Dictionary of property names to their schemas</param>
    /// <param name="required">List of required property names</param>
    /// <returns>JSON schema as JsonElement</returns>
    public static JsonElement CreateObjectSchema(
        Dictionary<string, object> properties,
        List<string>? required = null)
    {
        var schema = new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = properties
        };

        if (required != null && required.Count > 0)
        {
            schema["required"] = required;
        }

        var json = JsonSerializer.Serialize(schema);
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    /// <summary>
    /// Create a string property schema
    /// </summary>
    /// <param name="description">Description of the property</param>
    /// <param name="enumValues">Optional list of allowed values</param>
    /// <returns>Property schema</returns>
    public static Dictionary<string, object> String(string? description = null, List<string>? enumValues = null)
    {
        var schema = new Dictionary<string, object> { ["type"] = "string" };
        if (description != null) schema["description"] = description;
        if (enumValues != null && enumValues.Count > 0) schema["enum"] = enumValues;
        return schema;
    }

    /// <summary>
    /// Create a number property schema
    /// </summary>
    /// <param name="description">Description of the property</param>
    /// <param name="minimum">Minimum value</param>
    /// <param name="maximum">Maximum value</param>
    /// <returns>Property schema</returns>
    public static Dictionary<string, object> Number(string? description = null, double? minimum = null, double? maximum = null)
    {
        var schema = new Dictionary<string, object> { ["type"] = "number" };
        if (description != null) schema["description"] = description;
        if (minimum.HasValue) schema["minimum"] = minimum.Value;
        if (maximum.HasValue) schema["maximum"] = maximum.Value;
        return schema;
    }

    /// <summary>
    /// Create an integer property schema
    /// </summary>
    /// <param name="description">Description of the property</param>
    /// <param name="minimum">Minimum value</param>
    /// <param name="maximum">Maximum value</param>
    /// <returns>Property schema</returns>
    public static Dictionary<string, object> Integer(string? description = null, int? minimum = null, int? maximum = null)
    {
        var schema = new Dictionary<string, object> { ["type"] = "integer" };
        if (description != null) schema["description"] = description;
        if (minimum.HasValue) schema["minimum"] = minimum.Value;
        if (maximum.HasValue) schema["maximum"] = maximum.Value;
        return schema;
    }

    /// <summary>
    /// Create a boolean property schema
    /// </summary>
    /// <param name="description">Description of the property</param>
    /// <returns>Property schema</returns>
    public static Dictionary<string, object> Boolean(string? description = null)
    {
        var schema = new Dictionary<string, object> { ["type"] = "boolean" };
        if (description != null) schema["description"] = description;
        return schema;
    }

    /// <summary>
    /// Create an array property schema
    /// </summary>
    /// <param name="items">Schema for array items</param>
    /// <param name="description">Description of the property</param>
    /// <returns>Property schema</returns>
    public static Dictionary<string, object> Array(Dictionary<string, object> items, string? description = null)
    {
        var schema = new Dictionary<string, object> 
        { 
            ["type"] = "array",
            ["items"] = items
        };
        if (description != null) schema["description"] = description;
        return schema;
    }
}
