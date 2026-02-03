using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

/// <summary>
/// Type of API used for the generation
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ApiType>))]
public enum ApiType
{
    /// <summary>
    /// Completions API
    /// </summary>
    [JsonPropertyName("completions")]
    Completions,
    
    /// <summary>
    /// Embeddings API
    /// </summary>
    [JsonPropertyName("embeddings")]
    Embeddings
}

/// <summary>
/// Generation data with request and usage metadata
/// </summary>
public class GenerationData
{
    /// <summary>
    /// Unique identifier for the generation
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Upstream provider's identifier for this generation
    /// </summary>
    [JsonPropertyName("upstream_id")]
    public string? UpstreamId { get; init; }

    /// <summary>
    /// Total cost of the generation in USD
    /// </summary>
    [JsonPropertyName("total_cost")]
    public required double TotalCost { get; init; }

    /// <summary>
    /// Discount applied due to caching
    /// </summary>
    [JsonPropertyName("cache_discount")]
    public double? CacheDiscount { get; init; }

    /// <summary>
    /// Cost charged by the upstream provider
    /// </summary>
    [JsonPropertyName("upstream_inference_cost")]
    public double? UpstreamInferenceCost { get; init; }

    /// <summary>
    /// ISO 8601 timestamp of when the generation was created
    /// </summary>
    [JsonPropertyName("created_at")]
    public required string CreatedAt { get; init; }

    /// <summary>
    /// Model used for the generation
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// ID of the app that made the request
    /// </summary>
    [JsonPropertyName("app_id")]
    public int? AppId { get; init; }

    /// <summary>
    /// Whether the response was streamed
    /// </summary>
    [JsonPropertyName("streamed")]
    public bool? Streamed { get; init; }

    /// <summary>
    /// Whether the generation was cancelled
    /// </summary>
    [JsonPropertyName("cancelled")]
    public bool? Cancelled { get; init; }

    /// <summary>
    /// Name of the provider that served the request
    /// </summary>
    [JsonPropertyName("provider_name")]
    public string? ProviderName { get; init; }

    /// <summary>
    /// Total latency in milliseconds
    /// </summary>
    [JsonPropertyName("latency")]
    public double? Latency { get; init; }

    /// <summary>
    /// Moderation latency in milliseconds
    /// </summary>
    [JsonPropertyName("moderation_latency")]
    public double? ModerationLatency { get; init; }

    /// <summary>
    /// Time taken for generation in milliseconds
    /// </summary>
    [JsonPropertyName("generation_time")]
    public double? GenerationTime { get; init; }

    /// <summary>
    /// Reason the generation finished
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; init; }

    /// <summary>
    /// Number of tokens in the prompt
    /// </summary>
    [JsonPropertyName("tokens_prompt")]
    public int? TokensPrompt { get; init; }

    /// <summary>
    /// Number of tokens in the completion
    /// </summary>
    [JsonPropertyName("tokens_completion")]
    public int? TokensCompletion { get; init; }

    /// <summary>
    /// Native prompt tokens as reported by provider
    /// </summary>
    [JsonPropertyName("native_tokens_prompt")]
    public int? NativeTokensPrompt { get; init; }

    /// <summary>
    /// Native completion tokens as reported by provider
    /// </summary>
    [JsonPropertyName("native_tokens_completion")]
    public int? NativeTokensCompletion { get; init; }

    /// <summary>
    /// Native completion image tokens as reported by provider
    /// </summary>
    [JsonPropertyName("native_tokens_completion_images")]
    public int? NativeTokensCompletionImages { get; init; }

    /// <summary>
    /// Native reasoning tokens as reported by provider
    /// </summary>
    [JsonPropertyName("native_tokens_reasoning")]
    public int? NativeTokensReasoning { get; init; }

    /// <summary>
    /// Native cached tokens as reported by provider
    /// </summary>
    [JsonPropertyName("native_tokens_cached")]
    public int? NativeTokensCached { get; init; }

    /// <summary>
    /// Number of media items in the prompt
    /// </summary>
    [JsonPropertyName("num_media_prompt")]
    public int? NumMediaPrompt { get; init; }

    /// <summary>
    /// Number of audio inputs in the prompt
    /// </summary>
    [JsonPropertyName("num_input_audio_prompt")]
    public int? NumInputAudioPrompt { get; init; }

    /// <summary>
    /// Number of media items in the completion
    /// </summary>
    [JsonPropertyName("num_media_completion")]
    public int? NumMediaCompletion { get; init; }

    /// <summary>
    /// Number of search results included
    /// </summary>
    [JsonPropertyName("num_search_results")]
    public int? NumSearchResults { get; init; }

    /// <summary>
    /// Origin URL of the request
    /// </summary>
    [JsonPropertyName("origin")]
    public required string Origin { get; init; }

    /// <summary>
    /// Usage amount in USD
    /// </summary>
    [JsonPropertyName("usage")]
    public required double Usage { get; init; }

    /// <summary>
    /// Whether this used bring-your-own-key
    /// </summary>
    [JsonPropertyName("is_byok")]
    public required bool IsByok { get; init; }

    /// <summary>
    /// Native finish reason as reported by provider
    /// </summary>
    [JsonPropertyName("native_finish_reason")]
    public string? NativeFinishReason { get; init; }

    /// <summary>
    /// External user identifier
    /// </summary>
    [JsonPropertyName("external_user")]
    public string? ExternalUser { get; init; }

    /// <summary>
    /// Type of API used for the generation
    /// </summary>
    [JsonPropertyName("api_type")]
    public ApiType? ApiType { get; init; }

    /// <summary>
    /// Router used for the request (e.g., openrouter/auto)
    /// </summary>
    [JsonPropertyName("router")]
    public string? Router { get; init; }
}

/// <summary>
/// Response containing generation metadata
/// </summary>
public class GenerationResponse
{
    /// <summary>
    /// Generation data with request and usage metadata
    /// </summary>
    [JsonPropertyName("data")]
    public required GenerationData Data { get; init; }
}
