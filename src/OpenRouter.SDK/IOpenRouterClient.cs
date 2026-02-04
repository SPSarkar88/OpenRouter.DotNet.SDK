using OpenRouter.SDK.Core;
using OpenRouter.SDK.Services;

namespace OpenRouter.SDK;

/// <summary>
/// Interface for the main OpenRouter API client.
/// </summary>
public interface IOpenRouterClient
{
    /// <summary>
    /// Event raised before an HTTP request is sent.
    /// </summary>
    event Func<HttpRequestMessage, Task>? BeforeRequest;

    /// <summary>
    /// Event raised after an HTTP response is received.
    /// </summary>
    event Func<HttpResponseMessage, Task>? AfterResponse;

    /// <summary>
    /// Event raised when an HTTP request error occurs.
    /// </summary>
    event Func<Exception, HttpRequestMessage, Task>? OnError;

    /// <summary>
    /// Gets the underlying HTTP client service for advanced scenarios.
    /// </summary>
    IHttpClientService? HttpClient { get; }
    /// <summary>
    /// Gets the chat service for creating completions.
    /// </summary>
    IChatService Chat { get; }

    /// <summary>
    /// Gets the models service for retrieving available models.
    /// </summary>
    IModelsService Models { get; }

    /// <summary>
    /// Gets the embeddings service for generating embeddings.
    /// </summary>
    IEmbeddingsService Embeddings { get; }

    /// <summary>
    /// Gets the providers service for retrieving provider information.
    /// </summary>
    IProvidersService Providers { get; }

    /// <summary>
    /// Gets the endpoints service for retrieving endpoint information.
    /// </summary>
    IEndpointsService Endpoints { get; }

    /// <summary>
    /// Gets the generations service for retrieving generation metadata.
    /// </summary>
    IGenerationsService Generations { get; }

    /// <summary>
    /// Gets the OAuth service for PKCE authentication flow.
    /// </summary>
    IOAuthService OAuth { get; }

    /// <summary>
    /// Gets the API Keys service for managing API keys.
    /// Requires a provisioning key for authentication.
    /// </summary>
    IApiKeysService ApiKeys { get; }

    /// <summary>
    /// Gets the Analytics service for user activity tracking.
    /// Requires a provisioning key for authentication.
    /// </summary>
    IAnalyticsService Analytics { get; }

    /// <summary>
    /// Gets the Guardrails service for managing content moderation rules.
    /// Requires a provisioning key for authentication.
    /// </summary>
    IGuardrailsService Guardrails { get; }

    /// <summary>
    /// Gets the Beta API services (modern structured APIs)
    /// </summary>
    BetaServices Beta { get; }

    /// <summary>
    /// High-level method to call a model with simple text input.
    /// </summary>
    /// <param name="model">The model to use.</param>
    /// <param name="userMessage">The user's message.</param>
    /// <param name="systemMessage">Optional system message.</param>
    /// <param name="temperature">Optional temperature.</param>
    /// <param name="maxTokens">Optional max tokens.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The assistant's response text.</returns>
    Task<string> CallModelAsync(
        string model,
        string userMessage,
        string? systemMessage = null,
        double? temperature = null,
        int? maxTokens = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// High-level method to call a model with streaming.
    /// </summary>
    /// <param name="model">The model to use.</param>
    /// <param name="userMessage">The user's message.</param>
    /// <param name="systemMessage">Optional system message.</param>
    /// <param name="temperature">Optional temperature.</param>
    /// <param name="maxTokens">Optional max tokens.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of text chunks.</returns>
    IAsyncEnumerable<string> CallModelStreamAsync(
        string model,
        string userMessage,
        string? systemMessage = null,
        double? temperature = null,
        int? maxTokens = null,
        CancellationToken cancellationToken = default);
}
