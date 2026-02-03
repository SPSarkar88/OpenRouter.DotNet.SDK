using OpenRouter.SDK.Core;
using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;
/// <summary>
/// Implementation of endpoints service.
/// </summary>
public class EndpointsService : IEndpointsService
{
    private readonly IHttpClientService _httpClient;
    /// <summary>
    /// Constructor for EndpointsService
    /// </summary>
    /// <param name="httpClient"></param>
    public EndpointsService(IHttpClientService httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }
    /// <summary>
    /// Lists the endpoints for a specific model.
    /// </summary>
    /// <param name="author">The author of the model</param>
    /// <param name="slug">The slug identifier of the model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing model endpoints</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<ModelEndpointsResponse> ListAsync(
        string author, 
        string slug, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be null or empty", nameof(author));
        
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be null or empty", nameof(slug));

        var response = await _httpClient.GetAsync<ModelEndpointsResponse>(
            $"/models/{author}/{slug}/endpoints",
            cancellationToken: cancellationToken
        );

        return response;
    }
    /// <summary>
    /// Lists the ZDR endpoints.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing ZDR endpoints</returns>
    public async Task<ZdrEndpointsResponse> ListZdrEndpointsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync<ZdrEndpointsResponse>(
            "/endpoints/zdr",
            cancellationToken: cancellationToken
        );

        return response;
    }
}
