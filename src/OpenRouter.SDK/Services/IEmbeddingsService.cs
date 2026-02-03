using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Service for generating embeddings.
/// </summary>
public interface IEmbeddingsService
{
    /// <summary>
    /// Generates embeddings for the given input.
    /// </summary>
    /// <param name="request">The embedding request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The embedding response.</returns>
    Task<EmbeddingResponse> GenerateAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all available embeddings models.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The list of available embeddings models.</returns>
    Task<ModelsResponse> ListModelsAsync(
        CancellationToken cancellationToken = default);
}
