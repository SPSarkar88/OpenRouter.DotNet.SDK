using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

/// <summary>
/// Pricing information for a model endpoint
/// </summary>
public class Pricing
{
    /// <summary>
    /// Cost per prompt token (as string for precision)
    /// </summary>
    [JsonPropertyName("prompt")]
    public required string Prompt { get; init; }

    /// <summary>
    /// Cost per completion token (as string for precision)
    /// </summary>
    [JsonPropertyName("completion")]
    public required string Completion { get; init; }

    /// <summary>
    /// Cost per request
    /// </summary>
    [JsonPropertyName("request")]
    public string? Request { get; init; }

    /// <summary>
    /// Cost per image
    /// </summary>
    [JsonPropertyName("image")]
    public string? Image { get; init; }

    /// <summary>
    /// Cost per image token
    /// </summary>
    [JsonPropertyName("image_token")]
    public string? ImageToken { get; init; }

    /// <summary>
    /// Cost per output image
    /// </summary>
    [JsonPropertyName("image_output")]
    public string? ImageOutput { get; init; }

    /// <summary>
    /// Cost per audio unit
    /// </summary>
    [JsonPropertyName("audio")]
    public string? Audio { get; init; }

    /// <summary>
    /// Cost per audio output unit
    /// </summary>
    [JsonPropertyName("audio_output")]
    public string? AudioOutput { get; init; }

    /// <summary>
    /// Cost for input audio cache
    /// </summary>
    [JsonPropertyName("input_audio_cache")]
    public string? InputAudioCache { get; init; }

    /// <summary>
    /// Cost for web search operations
    /// </summary>
    [JsonPropertyName("web_search")]
    public string? WebSearch { get; init; }

    /// <summary>
    /// Cost for internal reasoning tokens
    /// </summary>
    [JsonPropertyName("internal_reasoning")]
    public string? InternalReasoning { get; init; }

    /// <summary>
    /// Cost for cache read operations
    /// </summary>
    [JsonPropertyName("input_cache_read")]
    public string? InputCacheRead { get; init; }

    /// <summary>
    /// Cost for cache write operations
    /// </summary>
    [JsonPropertyName("input_cache_write")]
    public string? InputCacheWrite { get; init; }

    /// <summary>
    /// Discount percentage
    /// </summary>
    [JsonPropertyName("discount")]
    public double? Discount { get; init; }
}

/// <summary>
/// Quantization type for model endpoint
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PublicEndpointQuantization
{
    [JsonPropertyName("int4")]
    Int4,
    
    [JsonPropertyName("int8")]
    Int8,
    
    [JsonPropertyName("fp4")]
    Fp4,
    
    [JsonPropertyName("fp6")]
    Fp6,
    
    [JsonPropertyName("fp8")]
    Fp8,
    
    [JsonPropertyName("fp16")]
    Fp16,
    
    [JsonPropertyName("bf16")]
    Bf16,
    
    [JsonPropertyName("fp32")]
    Fp32,
    
    [JsonPropertyName("unknown")]
    Unknown
}

/// <summary>
/// Statistics about endpoint performance
/// </summary>
public class PercentileStats
{
    /// <summary>
    /// 50th percentile (median)
    /// </summary>
    [JsonPropertyName("p50")]
    public double? P50 { get; init; }

    /// <summary>
    /// 95th percentile
    /// </summary>
    [JsonPropertyName("p95")]
    public double? P95 { get; init; }

    /// <summary>
    /// 99th percentile
    /// </summary>
    [JsonPropertyName("p99")]
    public double? P99 { get; init; }
}

/// <summary>
/// Information about a specific model endpoint
/// </summary>
public class PublicEndpoint
{
    /// <summary>
    /// Name of the endpoint
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The unique identifier for the model (permaslug)
    /// </summary>
    [JsonPropertyName("model_id")]
    public required string ModelId { get; init; }

    /// <summary>
    /// Display name of the model
    /// </summary>
    [JsonPropertyName("model_name")]
    public required string ModelName { get; init; }

    /// <summary>
    /// Context length (max tokens)
    /// </summary>
    [JsonPropertyName("context_length")]
    public required int ContextLength { get; init; }

    /// <summary>
    /// Pricing information
    /// </summary>
    [JsonPropertyName("pricing")]
    public required Pricing Pricing { get; init; }

    /// <summary>
    /// Provider name
    /// </summary>
    [JsonPropertyName("provider_name")]
    public required string ProviderName { get; init; }

    /// <summary>
    /// Tag for the endpoint
    /// </summary>
    [JsonPropertyName("tag")]
    public required string Tag { get; init; }

    /// <summary>
    /// Quantization type
    /// </summary>
    [JsonPropertyName("quantization")]
    public PublicEndpointQuantization? Quantization { get; init; }

    /// <summary>
    /// Maximum completion tokens allowed
    /// </summary>
    [JsonPropertyName("max_completion_tokens")]
    public int? MaxCompletionTokens { get; init; }

    /// <summary>
    /// Maximum prompt tokens allowed
    /// </summary>
    [JsonPropertyName("max_prompt_tokens")]
    public int? MaxPromptTokens { get; init; }

    /// <summary>
    /// List of supported parameters
    /// </summary>
    [JsonPropertyName("supported_parameters")]
    public List<string>? SupportedParameters { get; init; }

    /// <summary>
    /// Current status of the endpoint
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>
    /// Uptime percentage in last 30 minutes
    /// </summary>
    [JsonPropertyName("uptime_last_30m")]
    public double? UptimeLast30m { get; init; }

    /// <summary>
    /// Whether the endpoint supports implicit caching
    /// </summary>
    [JsonPropertyName("supports_implicit_caching")]
    public bool SupportsImplicitCaching { get; init; }

    /// <summary>
    /// Latency percentiles in milliseconds over the last 30 minutes
    /// </summary>
    [JsonPropertyName("latency_last_30m")]
    public PercentileStats? LatencyLast30m { get; init; }

    /// <summary>
    /// Throughput percentiles over the last 30 minutes
    /// </summary>
    [JsonPropertyName("throughput_last_30m")]
    public PercentileStats? ThroughputLast30m { get; init; }
}

/// <summary>
/// Model architecture information
/// </summary>
public class Architecture
{
    /// <summary>
    /// Tokenizer type used by the model
    /// </summary>
    [JsonPropertyName("tokenizer")]
    public string? Tokenizer { get; init; }

    /// <summary>
    /// Instruction format type
    /// </summary>
    [JsonPropertyName("instruct_type")]
    public string? InstructType { get; init; }

    /// <summary>
    /// Primary modality of the model
    /// </summary>
    [JsonPropertyName("modality")]
    public string? Modality { get; init; }

    /// <summary>
    /// Supported input modalities
    /// </summary>
    [JsonPropertyName("input_modalities")]
    public List<string>? InputModalities { get; init; }

    /// <summary>
    /// Supported output modalities
    /// </summary>
    [JsonPropertyName("output_modalities")]
    public List<string>? OutputModalities { get; init; }
}

/// <summary>
/// List of available endpoints for a model
/// </summary>
public class ModelEndpointsResponse
{
    /// <summary>
    /// Unique identifier for the model
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Display name of the model
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Unix timestamp of when the model was created
    /// </summary>
    [JsonPropertyName("created")]
    public required long Created { get; init; }

    /// <summary>
    /// Description of the model
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>
    /// Model architecture information
    /// </summary>
    [JsonPropertyName("architecture")]
    public required Architecture Architecture { get; init; }

    /// <summary>
    /// List of available endpoints for this model
    /// </summary>
    [JsonPropertyName("endpoints")]
    public required List<PublicEndpoint> Endpoints { get; init; }
}

/// <summary>
/// Response containing a list of ZDR endpoints
/// </summary>
public class ZdrEndpointsResponse
{
    /// <summary>
    /// List of available ZDR endpoints
    /// </summary>
    [JsonPropertyName("data")]
    public required List<PublicEndpoint> Data { get; init; }
}
