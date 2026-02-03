using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

/// <summary>
/// Request for chat completions.
/// </summary>
public class ChatCompletionRequest
{
    /// <summary>
    /// Gets or sets the model to use for completion.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// Gets or sets the list of messages in the conversation.
    /// </summary>
    [JsonPropertyName("messages")]
    public required List<Message> Messages { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of tokens to generate.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Gets or sets the sampling temperature (0-2).
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Gets or sets the nucleus sampling parameter (0-1).
    /// </summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    /// <summary>
    /// Gets or sets the top-k sampling parameter.
    /// </summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    /// <summary>
    /// Gets or sets whether to stream the response.
    /// </summary>
    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    /// <summary>
    /// Gets or sets stop sequences to terminate generation.
    /// </summary>
    [JsonPropertyName("stop")]
    public List<string>? Stop { get; set; }

    /// <summary>
    /// Gets or sets the presence penalty (-2.0 to 2.0).
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public double? PresencePenalty { get; set; }

    /// <summary>
    /// Gets or sets the frequency penalty (-2.0 to 2.0).
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public double? FrequencyPenalty { get; set; }

    /// <summary>
    /// Gets or sets the response format.
    /// </summary>
    [JsonPropertyName("response_format")]
    public ResponseFormat? ResponseFormat { get; set; }

    /// <summary>
    /// Gets or sets the tools/functions available to the model.
    /// </summary>
    [JsonPropertyName("tools")]
    public List<ToolDefinition>? Tools { get; set; }

    /// <summary>
    /// Gets or sets how the model should use tools.
    /// </summary>
    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; set; }

    /// <summary>
    /// Gets or sets provider-specific routing preferences.
    /// </summary>
    [JsonPropertyName("provider")]
    public ProviderPreferences? Provider { get; set; }

    /// <summary>
    /// Gets or sets reasoning configuration (for models that support reasoning).
    /// </summary>
    [JsonPropertyName("reasoning")]
    public ReasoningConfig? Reasoning { get; set; }
}

/// <summary>
/// Reasoning configuration for chat completions.
/// </summary>
public class ReasoningConfig
{
    /// <summary>
    /// Gets or sets whether reasoning is enabled.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    /// <summary>
    /// Gets or sets the reasoning effort level (e.g., "low", "medium", "high").
    /// </summary>
    [JsonPropertyName("effort")]
    public string? Effort { get; set; }
}

/// <summary>
/// Response from a chat completion.
/// </summary>
public class ChatCompletionResponse
{
    /// <summary>
    /// Gets or sets the unique identifier for this completion.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the object type (should be "chat.completion").
    /// </summary>
    [JsonPropertyName("object")]
    public required string Object { get; set; }

    /// <summary>
    /// Gets or sets the Unix timestamp when the completion was created.
    /// </summary>
    [JsonPropertyName("created")]
    public required long Created { get; set; }

    /// <summary>
    /// Gets or sets the model used for completion.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// Gets or sets the list of completion choices.
    /// </summary>
    [JsonPropertyName("choices")]
    public required List<ChatChoice> Choices { get; set; }

    /// <summary>
    /// Gets or sets token usage information.
    /// </summary>
    [JsonPropertyName("usage")]
    public Usage? Usage { get; set; }

    /// <summary>
    /// Gets or sets the system fingerprint.
    /// </summary>
    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }
}

/// <summary>
/// A completion choice.
/// </summary>
public class ChatChoice
{
    /// <summary>
    /// Gets or sets the index of this choice.
    /// </summary>
    [JsonPropertyName("index")]
    public required int Index { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    [JsonPropertyName("message")]
    public required AssistantMessage Message { get; set; }

    /// <summary>
    /// Gets or sets the reason the generation stopped.
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

/// <summary>
/// Streaming response chunk.
/// </summary>
public class ChatCompletionChunk
{
    /// <summary>
    /// Gets or sets the unique identifier for this chunk.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the object type (should be "chat.completion.chunk").
    /// </summary>
    [JsonPropertyName("object")]
    public required string Object { get; set; }

    /// <summary>
    /// Gets or sets the Unix timestamp when the chunk was created.
    /// </summary>
    [JsonPropertyName("created")]
    public required long Created { get; set; }

    /// <summary>
    /// Gets or sets the model used for completion.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// Gets or sets the list of streaming choices.
    /// </summary>
    [JsonPropertyName("choices")]
    public required List<ChatStreamChoice> Choices { get; set; }
}

/// <summary>
/// A streaming choice chunk.
/// </summary>
public class ChatStreamChoice
{
    /// <summary>
    /// Gets or sets the index of this choice.
    /// </summary>
    [JsonPropertyName("index")]
    public required int Index { get; set; }

    /// <summary>
    /// Gets or sets the message delta.
    /// </summary>
    [JsonPropertyName("delta")]
    public required MessageDelta Delta { get; set; }

    /// <summary>
    /// Gets or sets the reason the generation stopped.
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

/// <summary>
/// Represents a delta (partial update) to a message in streaming responses.
/// </summary>
public class MessageDelta
{
    /// <summary>
    /// Gets or sets the role (if present in this delta).
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// Gets or sets the content delta.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets tool call deltas.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    public List<ToolCallDelta>? ToolCalls { get; set; }
}

/// <summary>
/// Represents a delta for a tool call in streaming responses.
/// </summary>
public class ToolCallDelta
{
    /// <summary>
    /// Gets or sets the index of the tool call.
    /// </summary>
    [JsonPropertyName("index")]
    public required int Index { get; set; }

    /// <summary>
    /// Gets or sets the tool call ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the type of tool call.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the function call delta.
    /// </summary>
    [JsonPropertyName("function")]
    public FunctionCallDelta? Function { get; set; }
}

/// <summary>
/// Represents a delta for a function call in streaming responses.
/// </summary>
public class FunctionCallDelta
{
    /// <summary>
    /// Gets or sets the function name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the arguments delta (partial JSON string).
    /// </summary>
    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }
}

/// <summary>
/// Token usage information.
/// </summary>
public class Usage
{
    /// <summary>
    /// Gets or sets the number of tokens in the prompt.
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public required int PromptTokens { get; set; }

    /// <summary>
    /// Gets or sets the number of tokens in the completion.
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public required int CompletionTokens { get; set; }

    /// <summary>
    /// Gets or sets the total number of tokens used.
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public required int TotalTokens { get; set; }
}

/// <summary>
/// Response format specification.
/// </summary>
public class ResponseFormat
{
    /// <summary>
    /// Gets or sets the type of response format (e.g., "json_object", "text").
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }
}

/// <summary>
/// Tool/function definition.
/// </summary>
public class ToolDefinition
{
    /// <summary>
    /// Gets or sets the type of tool (typically "function").
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; } = "function";

    /// <summary>
    /// Gets or sets the function definition.
    /// </summary>
    [JsonPropertyName("function")]
    public required FunctionDefinition Function { get; set; }
}

/// <summary>
/// Function definition for tool calls.
/// </summary>
public class FunctionDefinition
{
    /// <summary>
    /// Gets or sets the name of the function.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of what the function does.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the parameters schema (JSON Schema).
    /// </summary>
    [JsonPropertyName("parameters")]
    public object? Parameters { get; set; }
}

/// <summary>
/// Provider-specific routing preferences.
/// </summary>
public class ProviderPreferences
{
    /// <summary>
    /// Gets or sets whether to use zero-downtime routing.
    /// </summary>
    [JsonPropertyName("zdr")]
    public bool? Zdr { get; set; }

    /// <summary>
    /// Gets or sets the sorting preference for providers.
    /// </summary>
    [JsonPropertyName("sort")]
    public string? Sort { get; set; }

    /// <summary>
    /// Gets or sets allowed providers.
    /// </summary>
    [JsonPropertyName("allow_fallbacks")]
    public bool? AllowFallbacks { get; set; }
}
