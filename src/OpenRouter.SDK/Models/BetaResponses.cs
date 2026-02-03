using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

/// <summary>
/// Input item for beta responses (text, image, audio, video, etc.)
/// </summary>
public class ResponsesInputItem
{
    /// <summary>
    /// Type of input (text, image_url, audio, video, etc.)
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Text content (when type is "text")
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>
    /// Image URL (when type is "image_url")
    /// </summary>
    [JsonPropertyName("image_url")]
    public object? ImageUrl { get; init; }

    /// <summary>
    /// Audio data (when type is "audio")
    /// </summary>
    [JsonPropertyName("audio")]
    public object? Audio { get; init; }

    /// <summary>
    /// Video data (when type is "video")
    /// </summary>
    [JsonPropertyName("video")]
    public object? Video { get; init; }
}

/// <summary>
/// Easy input message format for beta responses - message with role and content
/// </summary>
public class ResponsesEasyInputMessage
{
    /// <summary>
    /// Type - always "message"
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; } = "message";

    /// <summary>
    /// Role of the message (user, system, assistant, developer)
    /// </summary>
    [JsonPropertyName("role")]
    public required string Role { get; init; }

    /// <summary>
    /// Content of the message - can be a string or array of content items
    /// </summary>
    [JsonPropertyName("content")]
    public required object Content { get; init; }
}

/// <summary>
/// Function tool definition for beta responses
/// </summary>
public class ResponsesFunctionTool
{
    /// <summary>
    /// Type of tool (always "function")
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = "function";

    /// <summary>
    /// Name of the function
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Description of what the function does
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Whether strict schema validation is enabled
    /// </summary>
    [JsonPropertyName("strict")]
    public bool? Strict { get; init; }

    /// <summary>
    /// JSON schema for function parameters
    /// </summary>
    [JsonPropertyName("parameters")]
    public required Dictionary<string, object?> Parameters { get; init; }
}

/// <summary>
/// Text output configuration
/// </summary>
public class ResponseTextConfig
{
    /// <summary>
    /// Output format (e.g., "text", "json_object")
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; init; }

    /// <summary>
    /// Verbosity level
    /// </summary>
    [JsonPropertyName("verbosity")]
    public string? Verbosity { get; init; }
}

/// <summary>
/// Reasoning configuration for the response
/// </summary>
public class ResponseReasoningConfig
{
    /// <summary>
    /// Whether reasoning is enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; init; }

    /// <summary>
    /// Reasoning effort level
    /// </summary>
    [JsonPropertyName("effort")]
    public string? Effort { get; init; }
}

/// <summary>
/// Provider routing preferences
/// </summary>
public class ResponseProviderPreferences
{
    /// <summary>
    /// Whether to allow fallback providers
    /// </summary>
    [JsonPropertyName("allow_fallbacks")]
    public bool? AllowFallbacks { get; init; }

    /// <summary>
    /// Whether to require parameters support
    /// </summary>
    [JsonPropertyName("require_parameters")]
    public bool? RequireParameters { get; init; }

    /// <summary>
    /// Data collection preference
    /// </summary>
    [JsonPropertyName("data_collection")]
    public string? DataCollection { get; init; }

    /// <summary>
    /// Whether to use only ZDR endpoints
    /// </summary>
    [JsonPropertyName("zdr")]
    public bool? Zdr { get; init; }

    /// <summary>
    /// Ordered list of provider names to try
    /// </summary>
    [JsonPropertyName("order")]
    public List<string>? Order { get; init; }

    /// <summary>
    /// List of allowed providers
    /// </summary>
    [JsonPropertyName("only")]
    public List<string>? Only { get; init; }

    /// <summary>
    /// List of providers to ignore
    /// </summary>
    [JsonPropertyName("ignore")]
    public List<string>? Ignore { get; init; }
}

/// <summary>
/// Request for the Beta Responses API
/// </summary>
public class BetaResponsesRequest
{
    /// <summary>
    /// Input - can be a simple string or array of structured input items
    /// </summary>
    [JsonPropertyName("input")]
    public object? Input { get; init; }

    /// <summary>
    /// System instructions for the model
    /// </summary>
    [JsonPropertyName("instructions")]
    public string? Instructions { get; init; }

    /// <summary>
    /// Model to use
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; init; }

    /// <summary>
    /// List of fallback models
    /// </summary>
    [JsonPropertyName("models")]
    public List<string>? Models { get; init; }

    /// <summary>
    /// Available tools for the model to use
    /// </summary>
    [JsonPropertyName("tools")]
    public List<ResponsesFunctionTool>? Tools { get; init; }

    /// <summary>
    /// Tool choice configuration
    /// </summary>
    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; init; }

    /// <summary>
    /// Whether to allow parallel tool calls
    /// </summary>
    [JsonPropertyName("parallel_tool_calls")]
    public bool? ParallelToolCalls { get; init; }

    /// <summary>
    /// Maximum output tokens
    /// </summary>
    [JsonPropertyName("max_output_tokens")]
    public int? MaxOutputTokens { get; init; }

    /// <summary>
    /// Sampling temperature (0-2)
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; init; }

    /// <summary>
    /// Top-p sampling
    /// </summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; init; }

    /// <summary>
    /// Top-k sampling
    /// </summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; init; }

    /// <summary>
    /// Presence penalty
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public double? PresencePenalty { get; init; }

    /// <summary>
    /// Frequency penalty
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public double? FrequencyPenalty { get; init; }

    /// <summary>
    /// Maximum number of tool calls
    /// </summary>
    [JsonPropertyName("max_tool_calls")]
    public int? MaxToolCalls { get; init; }

    /// <summary>
    /// Text output configuration
    /// </summary>
    [JsonPropertyName("text")]
    public ResponseTextConfig? Text { get; init; }

    /// <summary>
    /// Reasoning configuration
    /// </summary>
    [JsonPropertyName("reasoning")]
    public ResponseReasoningConfig? Reasoning { get; init; }

    /// <summary>
    /// Output modalities (e.g., ["text"], ["text", "audio"])
    /// </summary>
    [JsonPropertyName("modalities")]
    public List<string>? Modalities { get; init; }

    /// <summary>
    /// Provider routing preferences
    /// </summary>
    [JsonPropertyName("provider")]
    public ResponseProviderPreferences? Provider { get; init; }

    /// <summary>
    /// Metadata for the request
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Whether to stream the response
    /// </summary>
    [JsonPropertyName("stream")]
    public bool? Stream { get; init; }

    /// <summary>
    /// Whether to store this request for training (must be false for /responses endpoint)
    /// </summary>
    [JsonPropertyName("store")]
    public bool? Store { get; init; } = false;

    /// <summary>
    /// Service tier for the request (e.g., "auto")
    /// </summary>
    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; init; } = "auto";

    /// <summary>
    /// User identifier
    /// </summary>
    [JsonPropertyName("user")]
    public string? User { get; init; }

    /// <summary>
    /// Session identifier for grouping requests
    /// </summary>
    [JsonPropertyName("session_id")]
    public string? SessionId { get; init; }
}

/// <summary>
/// Output item from beta responses
/// </summary>
public class ResponsesOutputItem
{
    /// <summary>
    /// Type of output (e.g., "text", "message", "function_call", "reasoning", "image", "audio")
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Message ID (when type is "message") or call ID (when type is "function_call")
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Role (when type is "message")
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; init; }

    /// <summary>
    /// Status (when type is "message" or "function_call")
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>
    /// Content array (when type is "message")
    /// </summary>
    [JsonPropertyName("content")]
    public List<ResponsesContentItem>? Content { get; init; }

    /// <summary>
    /// Text content (when type is "text")
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>
    /// Function name (when type is "function_call")
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Function arguments as JSON string (when type is "function_call")
    /// </summary>
    [JsonPropertyName("arguments")]
    public string? Arguments { get; init; }

    /// <summary>
    /// Call ID (when type is "function_call")
    /// </summary>
    [JsonPropertyName("call_id")]
    public string? CallId { get; init; }

    /// <summary>
    /// Function call information (when type is "function_call") - legacy nested format
    /// </summary>
    [JsonPropertyName("function_call")]
    public object? FunctionCall { get; init; }

    /// <summary>
    /// Reasoning content (when type is "reasoning")
    /// </summary>
    [JsonPropertyName("reasoning")]
    public string? Reasoning { get; init; }

    /// <summary>
    /// Image data (when type is "image")
    /// </summary>
    [JsonPropertyName("image")]
    public object? Image { get; init; }

    /// <summary>
    /// Audio data (when type is "audio")
    /// </summary>
    [JsonPropertyName("audio")]
    public object? Audio { get; init; }
}

/// <summary>
/// Content item within a message output
/// </summary>
public class ResponsesContentItem
{
    /// <summary>
    /// Type of content (e.g., "output_text")
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Text content (when type is "output_text")
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }
}

/// <summary>
/// Error field in responses
/// </summary>
public class ResponsesErrorField
{
    /// <summary>
    /// Error code
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    /// <summary>
    /// Error message
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    /// <summary>
    /// Additional error metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public object? Metadata { get; init; }
}

/// <summary>
/// Usage information for beta responses
/// </summary>
public class ResponsesUsage
{
    /// <summary>
    /// Number of prompt tokens used
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int? PromptTokens { get; init; }

    /// <summary>
    /// Number of completion tokens used
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int? CompletionTokens { get; init; }

    /// <summary>
    /// Total tokens used
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int? TotalTokens { get; init; }

    /// <summary>
    /// Cost in USD
    /// </summary>
    [JsonPropertyName("cost")]
    public double? Cost { get; init; }
}

/// <summary>
/// Non-streaming response from the Beta Responses API
/// </summary>
public class BetaResponsesResponse
{
    /// <summary>
    /// Unique identifier for the response
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Object type (always "response")
    /// </summary>
    [JsonPropertyName("object")]
    public required string Object { get; init; }

    /// <summary>
    /// Unix timestamp when created
    /// </summary>
    [JsonPropertyName("created_at")]
    public required long CreatedAt { get; init; }

    /// <summary>
    /// Model that generated the response
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// Status of the response (e.g., "completed", "failed")
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    /// <summary>
    /// Unix timestamp when completed
    /// </summary>
    [JsonPropertyName("completed_at")]
    public long? CompletedAt { get; init; }

    /// <summary>
    /// Output items from the response
    /// </summary>
    [JsonPropertyName("output")]
    public required List<ResponsesOutputItem> Output { get; init; }

    /// <summary>
    /// Combined output text (convenience field)
    /// </summary>
    [JsonPropertyName("output_text")]
    public string? OutputText { get; init; }

    /// <summary>
    /// Error information if the request failed
    /// </summary>
    [JsonPropertyName("error")]
    public ResponsesErrorField? Error { get; init; }

    /// <summary>
    /// Usage information
    /// </summary>
    [JsonPropertyName("usage")]
    public ResponsesUsage? Usage { get; init; }

    /// <summary>
    /// Metadata from the request
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Temperature used
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; init; }

    /// <summary>
    /// Top-p used
    /// </summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; init; }

    /// <summary>
    /// Maximum output tokens
    /// </summary>
    [JsonPropertyName("max_output_tokens")]
    public int? MaxOutputTokens { get; init; }

    /// <summary>
    /// Tools used in the request
    /// </summary>
    [JsonPropertyName("tools")]
    public List<ResponsesFunctionTool>? Tools { get; init; }

    /// <summary>
    /// Instructions used
    /// </summary>
    [JsonPropertyName("instructions")]
    public object? Instructions { get; init; }

    /// <summary>
    /// User identifier
    /// </summary>
    [JsonPropertyName("user")]
    public string? User { get; init; }
}

/// <summary>
/// Stream event chunk from Beta Responses API
/// </summary>
public class BetaResponsesStreamChunk
{
    /// <summary>
    /// Event type (e.g., "response.delta", "response.done")
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Response delta data
    /// </summary>
    [JsonPropertyName("response")]
    public BetaResponsesResponse? Response { get; init; }

    /// <summary>
    /// Delta output item
    /// </summary>
    [JsonPropertyName("delta")]
    public ResponsesOutputItem? Delta { get; init; }

    /// <summary>
    /// Index of the output item being updated
    /// </summary>
    [JsonPropertyName("index")]
    public int? Index { get; init; }
}
