using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Service for managing endpoint information
/// </summary>
public interface IEndpointsService
{
    /// <summary>
    /// List all endpoints for a specific model
    /// </summary>
    /// <param name="author">Model author (e.g., "openai", "anthropic")</param>
    /// <param name="slug">Model slug (e.g., "gpt-4", "claude-3-opus")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing model endpoint information</returns>
    Task<ModelEndpointsResponse> ListAsync(string author, string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Preview the impact of ZDR (Zero-Downtime Routing) on available endpoints
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing list of ZDR endpoints</returns>
    Task<ZdrEndpointsResponse> ListZdrEndpointsAsync(CancellationToken cancellationToken = default);
}
