using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Service for chat completions.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Creates a chat completion.
    /// </summary>
    /// <param name="request">The chat completion request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The chat completion response.</returns>
    Task<ChatCompletionResponse> CreateAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a streaming chat completion.
    /// </summary>
    /// <param name="request">The chat completion request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of chat completion chunks.</returns>
    IAsyncEnumerable<ChatCompletionChunk> CreateStreamAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);
}
