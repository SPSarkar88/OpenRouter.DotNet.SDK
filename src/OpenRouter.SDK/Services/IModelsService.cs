using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Service for retrieving available models.
/// </summary>
public interface IModelsService
{
    /// <summary>
    /// Gets the list of available models.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The models response.</returns>
    Task<ModelsResponse> GetModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of available models.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of available models.</returns>
    Task<ModelsCountResponse> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of models filtered by user preferences.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The models response filtered for the user.</returns>
    Task<ModelsResponse> GetModelsForUserAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Response containing the count of models
/// </summary>
public class ModelsCountResponse
{
    /// <summary>
    /// Total number of available models
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("count")]
    public int Count { get; init; }
}

