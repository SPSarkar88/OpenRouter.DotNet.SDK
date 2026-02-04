using OpenRouter.SDK.Core;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;

namespace OpenRouter.SDK;

/// <summary>
/// Main client for interacting with the OpenRouter API.
/// </summary>
public class OpenRouterClient : IOpenRouterClient
{
    private readonly IChatService _chat;
    private readonly IModelsService _models;
    private readonly IEmbeddingsService _embeddings;
    private readonly IProvidersService _providers;
    private readonly IEndpointsService _endpoints;
    private readonly IGenerationsService _generations;
    private readonly IOAuthService _oauth;
    private readonly IApiKeysService _apiKeys;
    private readonly IAnalyticsService _analytics;
    private readonly IGuardrailsService _guardrails;
    private readonly ICreditsService _credits;
    private readonly BetaServices _beta;
    private readonly IHttpClientService? _httpClientService;

    /// <summary>
    /// Event raised before an HTTP request is sent.
    /// Allows inspection and modification of the request.
    /// </summary>
    public event Func<HttpRequestMessage, Task>? BeforeRequest;

    /// <summary>
    /// Event raised after an HTTP response is received.
    /// Allows inspection of the response and original request.
    /// </summary>
    public event Func<HttpResponseMessage, Task>? AfterResponse;

    /// <summary>
    /// Event raised when an HTTP request error occurs.
    /// Allows custom error handling and logging.
    /// </summary>
    public event Func<Exception, HttpRequestMessage, Task>? OnError;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenRouterClient"/> class.
    /// This constructor is for direct instantiation (legacy mode).
    /// For ASP.NET Core / DI scenarios, use the constructor with injected services.
    /// </summary>
    /// <param name="apiKey">The OpenRouter API key.</param>
    /// <param name="configure">Optional configuration action.</param>
    public OpenRouterClient(string apiKey, Action<OpenRouterOptions>? configure = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or whitespace.", nameof(apiKey));

        var options = new OpenRouterOptions
        {
            ApiKey = apiKey
        };

        configure?.Invoke(options);

        IHttpClientService httpClient = new HttpClientService(options);
        _httpClientService = httpClient;

        // Wire up events to HTTP client hooks
        SetupHttpClientHooks(httpClient);

        // Direct instantiation of services
        _chat = new ChatService(httpClient);
        _models = new ModelsService(httpClient);
        _embeddings = new EmbeddingsService(httpClient);
        _providers = new ProvidersService(httpClient);
        _endpoints = new EndpointsService(httpClient);
        _generations = new GenerationsService(httpClient);
        _oauth = new OAuthService(httpClient);
        _apiKeys = new ApiKeysService(httpClient);
        _analytics = new AnalyticsService(httpClient);
        _guardrails = new GuardrailsService(httpClient);
        _credits = new CreditsService(httpClient);
        _beta = new BetaServices(httpClient);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenRouterClient"/> class with dependency injection.
    /// This constructor is used by ASP.NET Core's DI container.
    /// </summary>
    /// <param name="chatService">The chat service.</param>
    /// <param name="modelsService">The models service.</param>
    /// <param name="embeddingsService">The embeddings service.</param>
    /// <param name="providersService">The providers service.</param>
    /// <param name="endpointsService">The endpoints service.</param>
    /// <param name="generationsService">The generations service.</param>
    /// <param name="oauthService">The OAuth service.</param>
    /// <param name="apiKeysService">The API keys service.</param>
    /// <param name="analyticsService">The analytics service.</param>
    /// <param name="guardrailsService">The guardrails service.</param>
    /// <param name="betaServices">The beta services.</param>
    public OpenRouterClient(
        IChatService chatService,
        IModelsService modelsService,
        IEmbeddingsService embeddingsService,
        IProvidersService providersService,
        IEndpointsService endpointsService,
        IGenerationsService generationsService,
        IOAuthService oauthService,
        IApiKeysService apiKeysService,
        IAnalyticsService analyticsService,
        IGuardrailsService guardrailsService,
        BetaServices betaServices)
    {
        _chat = chatService ?? throw new ArgumentNullException(nameof(chatService));
        _models = modelsService ?? throw new ArgumentNullException(nameof(modelsService));
        _embeddings = embeddingsService ?? throw new ArgumentNullException(nameof(embeddingsService));
        _providers = providersService ?? throw new ArgumentNullException(nameof(providersService));
        _endpoints = endpointsService ?? throw new ArgumentNullException(nameof(endpointsService));
        _generations = generationsService ?? throw new ArgumentNullException(nameof(generationsService));
        _oauth = oauthService ?? throw new ArgumentNullException(nameof(oauthService));
        _apiKeys = apiKeysService ?? throw new ArgumentNullException(nameof(apiKeysService));
        _analytics = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _guardrails = guardrailsService ?? throw new ArgumentNullException(nameof(guardrailsService));
        _beta = betaServices ?? throw new ArgumentNullException(nameof(betaServices));
    }

    /// <summary>
    /// Gets the chat service for creating completions.
    /// </summary>
    public IChatService Chat => _chat;

    /// <summary>
    /// Gets the models service for retrieving available models.
    /// </summary>
    public IModelsService Models => _models;

    /// <summary>
    /// Gets the embeddings service for generating embeddings.
    /// </summary>
    public IEmbeddingsService Embeddings => _embeddings;

    /// <summary>
    /// Gets the providers service for retrieving provider information.
    /// </summary>
    public IProvidersService Providers => _providers;

    /// <summary>
    /// Gets the endpoints service for retrieving endpoint information.
    /// </summary>
    public IEndpointsService Endpoints => _endpoints;

    /// <summary>
    /// Gets the generations service for retrieving generation metadata.
    /// </summary>
    public IGenerationsService Generations => _generations;

    /// <summary>
    /// Gets the OAuth service for PKCE authentication flow.
    /// </summary>
    public IOAuthService OAuth => _oauth;

    /// <summary>
    /// Gets the API Keys service for managing API keys.
    /// Requires a provisioning key for authentication.
    /// </summary>
    public IApiKeysService ApiKeys => _apiKeys;

    /// <summary>
    /// Gets the Analytics service for user activity tracking.
    /// Requires a provisioning key for authentication.
    /// </summary>
    public IAnalyticsService Analytics => _analytics;

    /// <summary>
    /// Gets the Guardrails service for managing content moderation rules.
    /// Requires a provisioning key for authentication.
    /// </summary>
    public IGuardrailsService Guardrails => _guardrails;

    /// <summary>
    /// Gets the Credits service for credit balance and payments.
    /// Requires a provisioning key for authentication.
    /// </summary>
    public ICreditsService Credits => _credits;

    /// <summary>
    /// Gets the Beta API services (modern structured APIs)
    /// </summary>
    public BetaServices Beta => _beta;

    /// <summary>
    /// Gets the underlying HTTP client service for advanced scenarios.
    /// Allows direct access to HTTP client hooks and configuration.
    /// </summary>
    public IHttpClientService? HttpClient => _httpClientService;

    /// <summary>
    /// Sets up the HTTP client hooks to wire up to the public events.
    /// </summary>
    private void SetupHttpClientHooks(IHttpClientService httpClient)
    {
        // Wire BeforeRequest event
        httpClient.AddBeforeRequestHook(async (request) =>
        {
            if (BeforeRequest != null)
            {
                await BeforeRequest.Invoke(request);
            }
            return request;
        });

        // Wire AfterResponse event
        httpClient.AddResponseHook(async (response, request) =>
        {
            if (AfterResponse != null)
            {
                await AfterResponse.Invoke(response);
            }
        });

        // Wire OnError event
        httpClient.AddErrorHook(async (exception, request) =>
        {
            if (OnError != null)
            {
                await OnError.Invoke(exception, request);
            }
        });
    }

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
    public async Task<string> CallModelAsync(
        string model,
        string userMessage,
        string? systemMessage = null,
        double? temperature = null,
        int? maxTokens = null,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<Message>();

        if (!string.IsNullOrWhiteSpace(systemMessage))
        {
            messages.Add(new SystemMessage { Content = systemMessage });
        }

        messages.Add(new UserMessage { Content = userMessage });

        var request = new ChatCompletionRequest
        {
            Model = model,
            Messages = messages,
            Temperature = temperature,
            MaxTokens = maxTokens
        };

        var response = await Chat.CreateAsync(request, cancellationToken);

        return response.Choices.FirstOrDefault()?.Message.Content ?? string.Empty;
    }

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
    public async IAsyncEnumerable<string> CallModelStreamAsync(
        string model,
        string userMessage,
        string? systemMessage = null,
        double? temperature = null,
        int? maxTokens = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = new List<Message>();

        if (!string.IsNullOrWhiteSpace(systemMessage))
        {
            messages.Add(new SystemMessage { Content = systemMessage });
        }

        messages.Add(new UserMessage { Content = userMessage });

        var request = new ChatCompletionRequest
        {
            Model = model,
            Messages = messages,
            Temperature = temperature,
            MaxTokens = maxTokens
        };

        await foreach (var chunk in Chat.CreateStreamAsync(request, cancellationToken))
        {
            var content = chunk.Choices.FirstOrDefault()?.Delta.Content;
            if (!string.IsNullOrEmpty(content))
            {
                yield return content;
            }
        }
    }

    /// <summary>
    /// Call a model with automatic tool execution using the Beta Responses API.
    /// This is the recommended high-level method for working with tools.
    /// 
    /// Example:
    /// <code>
    /// var result = client.CallModel(new BetaResponsesRequest
    /// {
    ///     Model = "openai/gpt-4",
    ///     Input = new List&lt;ResponsesInputItem&gt; 
    ///     { 
    ///         new() { Type = "text", Text = "What's the weather in San Francisco?" }
    ///     }
    /// }, tools);
    /// 
    /// var text = await result.GetTextAsync();
    /// </code>
    /// </summary>
    /// <param name="request">The request to send to the model</param>
    /// <param name="tools">Optional tools for automatic execution</param>
    /// <param name="maxTurns">Maximum number of turns for tool execution (default 5)</param>
    /// <param name="stateAccessor">Optional state accessor for conversation persistence</param>
    /// <param name="stopConditions">Optional stop conditions to check after each step</param>
    /// <returns>ModelResult wrapper with multiple consumption patterns</returns>
    public ModelResult CallModel(
        BetaResponsesRequest request,
        IEnumerable<ITool>? tools = null,
        int maxTurns = 5,
        IStateAccessor? stateAccessor = null,
        IReadOnlyList<StopCondition>? stopConditions = null)
    {
        return new ModelResult(Beta.Responses, request, tools, maxTurns, stateAccessor, stopConditions);
    }

    /// <summary>
    /// Call a model with simple text input and tools using the Beta Responses API.
    /// 
    /// Example:
    /// <code>
    /// var result = client.CallModelWithTools(
    ///     model: "openai/gpt-4",
    ///     userMessage: "What's the weather in San Francisco?",
    ///     tools: weatherTool);
    /// 
    /// var text = await result.GetTextAsync();
    /// </code>
    /// </summary>
    /// <param name="model">The model to use</param>
    /// <param name="userMessage">The user's message</param>
    /// <param name="tools">Tools for automatic execution</param>
    /// <param name="systemMessage">Optional system message/instructions</param>
    /// <param name="temperature">Optional temperature</param>
    /// <param name="maxOutputTokens">Optional max output tokens</param>
    /// <param name="maxTurns">Maximum number of turns for tool execution (default 5)</param>
    /// <param name="stateAccessor">Optional state accessor for conversation persistence</param>
    /// <param name="stopConditions">Optional stop conditions to check after each step</param>
    /// <returns>ModelResult wrapper with multiple consumption patterns</returns>
    public ModelResult CallModelWithTools(
        string model,
        string userMessage,
        IEnumerable<ITool> tools,
        string? systemMessage = null,
        double? temperature = null,
        int? maxOutputTokens = null,
        int maxTurns = 5,
        IStateAccessor? stateAccessor = null,
        IReadOnlyList<StopCondition>? stopConditions = null)
    {
        var input = new List<ResponsesInputItem>
        {
            new() { Type = "text", Text = userMessage }
        };

        var request = new BetaResponsesRequest
        {
            Model = model,
            Input = input,
            Instructions = systemMessage,
            Temperature = temperature,
            MaxOutputTokens = maxOutputTokens
        };

        return new ModelResult(Beta.Responses, request, tools, maxTurns, stateAccessor, stopConditions);
    }

    /// <summary>
    /// Call a model with dynamic parameter resolution based on conversation context.
    /// Parameters can be static values or functions that resolve based on turn context.
    /// 
    /// Example:
    /// <code>
    /// var result = client.CallModelDynamic(new DynamicBetaResponsesRequest
    /// {
    ///     // Switch to GPT-4 after 3 turns
    ///     Model = new DynamicParameter&lt;string&gt;(ctx => 
    ///         ctx.NumberOfTurns > 3 ? "openai/gpt-4" : "openai/gpt-3.5-turbo"),
    ///     
    ///     // Adjust temperature based on context
    ///     Temperature = new DynamicParameter&lt;double?&gt;(async ctx => 
    ///     {
    ///         var prefs = await FetchUserPreferences();
    ///         return prefs.Creativity;
    ///     }),
    ///     
    ///     Input = new List&lt;ResponsesInputItem&gt; 
    ///     { 
    ///         new() { Type = "text", Text = "Hello" }
    ///     }
    /// }, tools);
    /// </code>
    /// </summary>
    /// <param name="dynamicRequest">Request with dynamic parameters</param>
    /// <param name="tools">Optional tools for automatic execution</param>
    /// <param name="maxTurns">Maximum number of turns for tool execution (default 5)</param>
    /// <param name="stateAccessor">Optional state accessor for conversation persistence</param>
    /// <param name="stopConditions">Optional stop conditions to check after each step</param>
    /// <returns>ModelResult wrapper with multiple consumption patterns</returns>
    public ModelResult CallModelDynamic(
        DynamicBetaResponsesRequest dynamicRequest,
        IEnumerable<ITool>? tools = null,
        int maxTurns = 5,
        IStateAccessor? stateAccessor = null,
        IReadOnlyList<StopCondition>? stopConditions = null)
    {
        return new ModelResult(Beta.Responses, dynamicRequest, tools, maxTurns, stateAccessor, stopConditions);
    }
}
