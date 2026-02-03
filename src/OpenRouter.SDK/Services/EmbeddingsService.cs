using OpenRouter.SDK.Core;
using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Implementation of embeddings service.
/// </summary>
public class EmbeddingsService : IEmbeddingsService
{
    private readonly IHttpClientService _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingsService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client service.</param>
    public EmbeddingsService(IHttpClientService httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc/>
    public async Task<EmbeddingResponse> GenerateAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var response = await _httpClient.PostJsonAsync<EmbeddingRequest, EmbeddingResponse>(
            "/embeddings",
            request,
            null,
            cancellationToken);

        return response;
    }

    /// <inheritdoc/>
    public async Task<ModelsResponse> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync<ModelsResponse>(
            "/embeddings/models",
            null,
            cancellationToken);

        return response;
    }
}
