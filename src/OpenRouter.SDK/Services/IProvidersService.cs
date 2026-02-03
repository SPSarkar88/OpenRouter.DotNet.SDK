using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Service for managing provider information
/// </summary>
public interface IProvidersService
{
    /// <summary>
    /// List all available AI providers on OpenRouter
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing list of providers</returns>
    Task<ProvidersResponse> ListAsync(CancellationToken cancellationToken = default);
}
