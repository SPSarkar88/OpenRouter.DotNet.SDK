using OpenRouter.SDK.Core;
using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Implementation of models service.
/// </summary>
public class ModelsService : IModelsService
{
    private readonly IHttpClientService _httpClient;
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelsService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client service.</param>
    public ModelsService(IHttpClientService httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc/>
    public async Task<ModelsResponse> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync<ModelsResponse>(
            "/models",
            null,
            cancellationToken);

        return response;
    }

    /// <inheritdoc/>
    public async Task<ModelsCountResponse> GetCountAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync<ModelsCountResponse>(
            "/models/count",
            null,
            cancellationToken);

        return response;
    }

    /// <inheritdoc/>
    public async Task<ModelsResponse> GetModelsForUserAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync<ModelsResponse>(
            "/models/user",
            null,
            cancellationToken);

        return response;
    }
}

