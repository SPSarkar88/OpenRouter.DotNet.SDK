using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

/// <summary>
/// Represents a model available on OpenRouter.
/// </summary>
public class Model
{
    /// <summary>
    /// Gets or sets the model identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the human-readable name of the model.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the model description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the context length (maximum tokens).
    /// </summary>
    [JsonPropertyName("context_length")]
    public int? ContextLength { get; set; }

    /// <summary>
    /// Gets or sets the pricing information.
    /// </summary>
    [JsonPropertyName("pricing")]
    public ModelPricing? Pricing { get; set; }

    /// <summary>
    /// Gets or sets the model's architecture.
    /// </summary>
    [JsonPropertyName("architecture")]
    public ModelArchitecture? Architecture { get; set; }

    /// <summary>
    /// Gets or sets the top provider for this model.
    /// </summary>
    [JsonPropertyName("top_provider")]
    public ModelProvider? TopProvider { get; set; }

    /// <summary>
    /// Gets or sets per-request limits.
    /// </summary>
    [JsonPropertyName("per_request_limits")]
    public PerRequestLimits? PerRequestLimits { get; set; }
}

/// <summary>
/// Model pricing information.
/// </summary>
public class ModelPricing
{
    /// <summary>
    /// Gets or sets the cost per 1M prompt tokens.
    /// </summary>
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    /// <summary>
    /// Gets or sets the cost per 1M completion tokens.
    /// </summary>
    [JsonPropertyName("completion")]
    public string? Completion { get; set; }

    /// <summary>
    /// Gets or sets the cost per image.
    /// </summary>
    [JsonPropertyName("image")]
    public string? Image { get; set; }

    /// <summary>
    /// Gets or sets the cost per request.
    /// </summary>
    [JsonPropertyName("request")]
    public string? Request { get; set; }
}

/// <summary>
/// Model architecture information.
/// </summary>
public class ModelArchitecture
{
    /// <summary>
    /// Gets or sets the architecture modality (e.g., "text", "multimodal").
    /// </summary>
    [JsonPropertyName("modality")]
    public string? Modality { get; set; }

    /// <summary>
    /// Gets or sets the tokenizer used.
    /// </summary>
    [JsonPropertyName("tokenizer")]
    public string? Tokenizer { get; set; }

    /// <summary>
    /// Gets or sets the instruct type.
    /// </summary>
    [JsonPropertyName("instruct_type")]
    public string? InstructType { get; set; }
}

/// <summary>
/// Information about a model provider.
/// </summary>
public class ModelProvider
{
    /// <summary>
    /// Gets or sets the provider name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the maximum completion tokens.
    /// </summary>
    [JsonPropertyName("max_completion_tokens")]
    public int? MaxCompletionTokens { get; set; }

    /// <summary>
    /// Gets or sets whether the provider is a moderated service.
    /// </summary>
    [JsonPropertyName("is_moderated")]
    public bool? IsModerated { get; set; }
}

/// <summary>
/// Per-request limits for a model.
/// </summary>
public class PerRequestLimits
{
    /// <summary>
    /// Gets or sets the maximum prompt tokens allowed per request.
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int? PromptTokens { get; set; }

    /// <summary>
    /// Gets or sets the maximum completion tokens allowed per request.
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int? CompletionTokens { get; set; }
}

/// <summary>
/// Response containing a list of available models.
/// </summary>
public class ModelsResponse
{
    /// <summary>
    /// Gets or sets the list of available models.
    /// </summary>
    [JsonPropertyName("data")]
    public required List<Model> Data { get; set; }
}
