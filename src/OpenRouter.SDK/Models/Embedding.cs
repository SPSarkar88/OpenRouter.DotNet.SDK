using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

/// <summary>
/// Request for generating embeddings.
/// </summary>
public class EmbeddingRequest
{
    /// <summary>
    /// Gets or sets the input text or array of texts to embed.
    /// </summary>
    [JsonPropertyName("input")]
    public required object Input { get; set; } // string, string[], int[], int[][], or Input[]

    /// <summary>
    /// Gets or sets the model to use for embeddings.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// Gets or sets the encoding format for the embeddings.
    /// </summary>
    [JsonPropertyName("encoding_format")]
    public string? EncodingFormat { get; set; } // "float" or "base64"

    /// <summary>
    /// Gets or sets the number of dimensions for the embeddings.
    /// </summary>
    [JsonPropertyName("dimensions")]
    public int? Dimensions { get; set; }

    /// <summary>
    /// Gets or sets a unique identifier for the end-user.
    /// </summary>
    [JsonPropertyName("user")]
    public string? User { get; set; }

    /// <summary>
    /// Gets or sets provider routing preferences.
    /// </summary>
    [JsonPropertyName("provider")]
    public ProviderPreferences? Provider { get; set; }

    /// <summary>
    /// Gets or sets the input type.
    /// </summary>
    [JsonPropertyName("input_type")]
    public string? InputType { get; set; }
}

/// <summary>
/// Response from an embeddings request.
/// </summary>
public class EmbeddingResponse
{
    /// <summary>
    /// Gets or sets the unique identifier for the embeddings request.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the object type (should be "list").
    /// </summary>
    [JsonPropertyName("object")]
    public required string Object { get; set; }

    /// <summary>
    /// Gets or sets the list of embedding data.
    /// </summary>
    [JsonPropertyName("data")]
    public required List<EmbeddingData> Data { get; set; }

    /// <summary>
    /// Gets or sets the model used for embeddings.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// Gets or sets usage information.
    /// </summary>
    [JsonPropertyName("usage")]
    public EmbeddingUsage? Usage { get; set; }
}

/// <summary>
/// Individual embedding data.
/// </summary>
public class EmbeddingData
{
    /// <summary>
    /// Gets or sets the object type (should be "embedding").
    /// </summary>
    [JsonPropertyName("object")]
    public required string Object { get; set; }

    /// <summary>
    /// Gets or sets the embedding vector (array of floats or base64 string).
    /// </summary>
    [JsonPropertyName("embedding")]
    public required object Embedding { get; set; } // double[] or string

    /// <summary>
    /// Gets or sets the index of this embedding in the request.
    /// </summary>
    [JsonPropertyName("index")]
    public int? Index { get; set; }
}

/// <summary>
/// Usage information for embeddings.
/// </summary>
public class EmbeddingUsage
{
    /// <summary>
    /// Gets or sets the number of tokens in the prompt.
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public required int PromptTokens { get; set; }

    /// <summary>
    /// Gets or sets the total number of tokens used.
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public required int TotalTokens { get; set; }

    /// <summary>
    /// Gets or sets the cost of the request.
    /// </summary>
    [JsonPropertyName("cost")]
    public double? Cost { get; set; }
}
