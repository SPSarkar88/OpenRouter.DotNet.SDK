using OpenRouter.SDK.Core;
using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;
/// <summary>
/// Implementation of providers service.
/// </summary>
public class ProvidersService : IProvidersService
{
    private readonly IHttpClientService _httpClient;
    /// <summary>
    /// Constructor for ProvidersService
    /// </summary>
    /// <param name="httpClient"></param>
    public ProvidersService(IHttpClientService httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }
    /// <inheritdoc/>
    public async Task<ProvidersResponse> ListAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync<ProvidersResponse>(
            "/providers",
            cancellationToken: cancellationToken
        );

        return response;
    }
}
