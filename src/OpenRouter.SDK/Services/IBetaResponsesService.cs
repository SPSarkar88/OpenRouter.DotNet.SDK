using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Service for the Beta Responses API - modern structured response interface
/// </summary>
public interface IBetaResponsesService
{
    /// <summary>
    /// Send a request to the Beta Responses API (non-streaming)
    /// </summary>
    /// <param name="request">The responses request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from the API</returns>
    Task<BetaResponsesResponse> SendAsync(BetaResponsesRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a streaming request to the Beta Responses API
    /// </summary>
    /// <param name="request">The responses request with stream enabled</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream of response chunks</returns>
    IAsyncEnumerable<BetaResponsesStreamChunk> SendStreamAsync(BetaResponsesRequest request, CancellationToken cancellationToken = default);
}
