using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

/// <summary>
/// Activity data for a specific endpoint on a specific date
/// </summary>
public class ActivityItem
{
    /// <summary>
    /// Date of the activity (YYYY-MM-DD format)
    /// </summary>
    [JsonPropertyName("date")]
    public required string Date { get; init; }

    /// <summary>
    /// Model slug (e.g., "openai/gpt-4.1")
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// Model permaslug (e.g., "openai/gpt-4.1-2025-04-14")
    /// </summary>
    [JsonPropertyName("model_permaslug")]
    public required string ModelPermaslug { get; init; }

    /// <summary>
    /// Unique identifier for the endpoint
    /// </summary>
    [JsonPropertyName("endpoint_id")]
    public required string EndpointId { get; init; }

    /// <summary>
    /// Name of the provider serving this endpoint
    /// </summary>
    [JsonPropertyName("provider_name")]
    public required string ProviderName { get; init; }

    /// <summary>
    /// Total cost in USD (OpenRouter credits spent)
    /// </summary>
    [JsonPropertyName("usage")]
    public required double Usage { get; init; }

    /// <summary>
    /// BYOK inference cost in USD (external credits spent)
    /// </summary>
    [JsonPropertyName("byok_usage_inference")]
    public required double ByokUsageInference { get; init; }

    /// <summary>
    /// Number of requests made
    /// </summary>
    [JsonPropertyName("requests")]
    public required int Requests { get; init; }

    /// <summary>
    /// Total prompt tokens used
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public required int PromptTokens { get; init; }

    /// <summary>
    /// Total completion tokens generated
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public required int CompletionTokens { get; init; }

    /// <summary>
    /// Total reasoning tokens used
    /// </summary>
    [JsonPropertyName("reasoning_tokens")]
    public required int ReasoningTokens { get; init; }
}

/// <summary>
/// Response containing user activity data grouped by endpoint
/// </summary>
public class GetUserActivityResponse
{
    /// <summary>
    /// List of activity items
    /// </summary>
    [JsonPropertyName("data")]
    public required List<ActivityItem> Data { get; init; }
}
