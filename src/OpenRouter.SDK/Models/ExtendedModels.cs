using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

#region Plugin Models

/// <summary>
/// Base class for chat plugins
/// </summary>
public abstract class ChatPlugin
{
    /// <summary>
    /// Plugin identifier
    /// </summary>
    [JsonPropertyName("id")]
    public abstract string Id { get; }

    /// <summary>
    /// Whether the plugin is enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
}

/// <summary>
/// Web search plugin for real-time web results
/// </summary>
public class WebSearchPlugin : ChatPlugin
{
    /// <inheritdoc />
    public override string Id => "web";

    /// <summary>
    /// Maximum number of search results to return
    /// </summary>
    [JsonPropertyName("max_results")]
    public int? MaxResults { get; set; }

    /// <summary>
    /// Custom search prompt for the web search
    /// </summary>
    [JsonPropertyName("search_prompt")]
    public string? SearchPrompt { get; set; }

    /// <summary>
    /// Search engine to use ("native" or "exa")
    /// </summary>
    [JsonPropertyName("engine")]
    public string? Engine { get; set; }
}

/// <summary>
/// Auto-router plugin for intelligent model selection
/// </summary>
public class AutoRouterPlugin : ChatPlugin
{
    /// <inheritdoc />
    public override string Id => "auto-router";

    /// <summary>
    /// List of allowed models for auto-routing
    /// </summary>
    [JsonPropertyName("allowed_models")]
    public List<string>? AllowedModels { get; set; }
}

/// <summary>
/// File parser plugin for document processing
/// </summary>
public class FileParserPlugin : ChatPlugin
{
    /// <inheritdoc />
    public override string Id => "file-parser";
}

/// <summary>
/// Moderation plugin for content filtering
/// </summary>
public class ModerationPlugin : ChatPlugin
{
    /// <inheritdoc />
    public override string Id => "moderation";

    /// <summary>
    /// Whether to block flagged content
    /// </summary>
    [JsonPropertyName("block")]
    public bool? Block { get; set; }

    /// <summary>
    /// Custom categories to check
    /// </summary>
    [JsonPropertyName("categories")]
    public List<string>? Categories { get; set; }
}

#endregion

#region Extended Provider Preferences

/// <summary>
/// Extended provider preferences with full feature support
/// </summary>
public class ExtendedProviderPreferences
{
    /// <summary>
    /// Whether to allow fallback to other providers
    /// </summary>
    [JsonPropertyName("allow_fallbacks")]
    public bool? AllowFallbacks { get; set; }

    /// <summary>
    /// Whether to require parameter support from providers
    /// </summary>
    [JsonPropertyName("require_parameters")]
    public bool? RequireParameters { get; set; }

    /// <summary>
    /// Data collection preference ("allow" or "deny")
    /// </summary>
    [JsonPropertyName("data_collection")]
    public string? DataCollection { get; set; }

    /// <summary>
    /// Whether to use only Zero Data Retention endpoints
    /// </summary>
    [JsonPropertyName("zdr")]
    public bool? Zdr { get; set; }

    /// <summary>
    /// Whether to enforce distillable text output
    /// </summary>
    [JsonPropertyName("enforce_distillable_text")]
    public bool? EnforceDistillableText { get; set; }

    /// <summary>
    /// Ordered list of providers to try
    /// </summary>
    [JsonPropertyName("order")]
    public List<string>? Order { get; set; }

    /// <summary>
    /// List of allowed providers (exclusive filter)
    /// </summary>
    [JsonPropertyName("only")]
    public List<string>? Only { get; set; }

    /// <summary>
    /// List of providers to ignore/exclude
    /// </summary>
    [JsonPropertyName("ignore")]
    public List<string>? Ignore { get; set; }

    /// <summary>
    /// Allowed quantization levels (e.g., "int4", "int8", "fp8", "fp16", "bf16")
    /// </summary>
    [JsonPropertyName("quantizations")]
    public List<string>? Quantizations { get; set; }

    /// <summary>
    /// Sorting strategy for provider selection
    /// </summary>
    [JsonPropertyName("sort")]
    public ProviderSort? Sort { get; set; }

    /// <summary>
    /// Maximum price configuration
    /// </summary>
    [JsonPropertyName("max_price")]
    public MaxPriceConfig? MaxPrice { get; set; }

    /// <summary>
    /// Preferred minimum throughput (tokens per second)
    /// Can be a number or "auto"
    /// </summary>
    [JsonPropertyName("preferred_min_throughput")]
    public object? PreferredMinThroughput { get; set; }

    /// <summary>
    /// Preferred maximum latency (milliseconds)
    /// Can be a number or "auto"
    /// </summary>
    [JsonPropertyName("preferred_max_latency")]
    public object? PreferredMaxLatency { get; set; }
}

/// <summary>
/// Provider sorting options
/// </summary>
public class ProviderSort
{
    /// <summary>
    /// Sort by field (e.g., "price", "throughput", "latency")
    /// </summary>
    [JsonPropertyName("by")]
    public string? By { get; set; }

    /// <summary>
    /// Sort order ("asc" or "desc")
    /// </summary>
    [JsonPropertyName("order")]
    public string? Order { get; set; }
}

/// <summary>
/// Maximum price configuration
/// </summary>
public class MaxPriceConfig
{
    /// <summary>
    /// Maximum price per prompt token (in USD per million tokens)
    /// </summary>
    [JsonPropertyName("prompt")]
    public decimal? Prompt { get; set; }

    /// <summary>
    /// Maximum price per completion token (in USD per million tokens)
    /// </summary>
    [JsonPropertyName("completion")]
    public decimal? Completion { get; set; }

    /// <summary>
    /// Maximum price per request (in USD)
    /// </summary>
    [JsonPropertyName("request")]
    public decimal? Request { get; set; }

    /// <summary>
    /// Maximum price per image (in USD)
    /// </summary>
    [JsonPropertyName("image")]
    public decimal? Image { get; set; }
}

#endregion

#region Extended Chat Request

/// <summary>
/// Extended chat completion request with full feature support
/// Includes all parameters from the TypeScript SDK
/// </summary>
public class ExtendedChatCompletionRequest
{
    /// <summary>
    /// Model to use for completion
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// List of fallback models (used with route: "fallback")
    /// </summary>
    [JsonPropertyName("models")]
    public List<string>? Models { get; set; }

    /// <summary>
    /// Routing strategy ("fallback" or for sorting)
    /// </summary>
    [JsonPropertyName("route")]
    public string? Route { get; set; }

    /// <summary>
    /// Conversation messages
    /// </summary>
    [JsonPropertyName("messages")]
    public required List<Message> Messages { get; set; }

    /// <summary>
    /// Maximum tokens to generate
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Maximum completion tokens (alternative to max_tokens)
    /// </summary>
    [JsonPropertyName("max_completion_tokens")]
    public int? MaxCompletionTokens { get; set; }

    /// <summary>
    /// Sampling temperature (0-2)
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Top-p (nucleus) sampling
    /// </summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    /// <summary>
    /// Top-k sampling
    /// </summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    /// <summary>
    /// Whether to stream the response
    /// </summary>
    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    /// <summary>
    /// Stream options configuration
    /// </summary>
    [JsonPropertyName("stream_options")]
    public StreamOptions? StreamOptions { get; set; }

    /// <summary>
    /// Stop sequences
    /// </summary>
    [JsonPropertyName("stop")]
    public object? Stop { get; set; }

    /// <summary>
    /// Presence penalty (-2 to 2)
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public double? PresencePenalty { get; set; }

    /// <summary>
    /// Frequency penalty (-2 to 2)
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public double? FrequencyPenalty { get; set; }

    /// <summary>
    /// Response format configuration
    /// </summary>
    [JsonPropertyName("response_format")]
    public ResponseFormat? ResponseFormat { get; set; }

    /// <summary>
    /// Tool definitions for function calling
    /// </summary>
    [JsonPropertyName("tools")]
    public List<ToolDefinition>? Tools { get; set; }

    /// <summary>
    /// Tool choice strategy
    /// </summary>
    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; set; }

    /// <summary>
    /// Extended provider preferences
    /// </summary>
    [JsonPropertyName("provider")]
    public ExtendedProviderPreferences? Provider { get; set; }

    /// <summary>
    /// Reasoning configuration
    /// </summary>
    [JsonPropertyName("reasoning")]
    public ReasoningConfig? Reasoning { get; set; }

    /// <summary>
    /// Plugins to enable
    /// </summary>
    [JsonPropertyName("plugins")]
    public List<ChatPlugin>? Plugins { get; set; }

    /// <summary>
    /// Logit bias for token probabilities
    /// </summary>
    [JsonPropertyName("logit_bias")]
    public Dictionary<string, int>? LogitBias { get; set; }

    /// <summary>
    /// Whether to return log probabilities
    /// </summary>
    [JsonPropertyName("logprobs")]
    public bool? Logprobs { get; set; }

    /// <summary>
    /// Number of top log probabilities to return
    /// </summary>
    [JsonPropertyName("top_logprobs")]
    public int? TopLogprobs { get; set; }

    /// <summary>
    /// Seed for deterministic generation
    /// </summary>
    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    /// <summary>
    /// User identifier for tracking
    /// </summary>
    [JsonPropertyName("user")]
    public string? User { get; set; }

    /// <summary>
    /// Session ID for request grouping
    /// </summary>
    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }

    /// <summary>
    /// Output modalities (e.g., ["text"], ["text", "audio"])
    /// </summary>
    [JsonPropertyName("modalities")]
    public List<string>? Modalities { get; set; }

    /// <summary>
    /// Request metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Debug configuration
    /// </summary>
    [JsonPropertyName("debug")]
    public DebugConfig? Debug { get; set; }

    /// <summary>
    /// Image configuration for vision models
    /// </summary>
    [JsonPropertyName("image_config")]
    public Dictionary<string, object>? ImageConfig { get; set; }
}

/// <summary>
/// Stream options configuration
/// </summary>
public class StreamOptions
{
    /// <summary>
    /// Whether to include usage in the stream
    /// </summary>
    [JsonPropertyName("include_usage")]
    public bool? IncludeUsage { get; set; }
}

/// <summary>
/// Debug configuration for request tracing
/// </summary>
public class DebugConfig
{
    /// <summary>
    /// Whether to echo the upstream request body
    /// </summary>
    [JsonPropertyName("echo_upstream_body")]
    public bool? EchoUpstreamBody { get; set; }
}

#endregion
