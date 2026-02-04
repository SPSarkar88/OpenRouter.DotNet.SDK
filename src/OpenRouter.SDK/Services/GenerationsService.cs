using OpenRouter.SDK.Core;
using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Service for retrieving generation metadata and usage information
/// </summary>
public interface IGenerationsService
{
    /// <summary>
    /// Get request and usage metadata for a generation
    /// </summary>
    /// <param name="generationId">The unique identifier of the generation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generation metadata including usage and cost information</returns>
    Task<GenerationResponse> GetGenerationAsync(string generationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of the Generations service
/// </summary>
public class GenerationsService : IGenerationsService
{
    private readonly IHttpClientService _httpClient;
    /// <summary>
    /// Constructor for GenerationsService
    /// </summary>
    /// <param name="httpClient">HTTP client service for making requests</param>
    public GenerationsService(IHttpClientService httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<GenerationResponse> GetGenerationAsync(string generationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(generationId))
        {
            throw new ArgumentException("Generation ID cannot be null or empty", nameof(generationId));
        }

        var response = await _httpClient.GetAsync<GenerationResponse>(
            $"/generation?id={Uri.EscapeDataString(generationId)}",
            cancellationToken: cancellationToken
        );

        return response;
    }
}
