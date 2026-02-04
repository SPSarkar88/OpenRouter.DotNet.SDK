using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenRouter.SDK.Core;
using OpenRouter.SDK.Services;

namespace OpenRouter.SDK;

/// <summary>
/// Extension methods for registering OpenRouter SDK services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenRouter SDK services to the service collection.
    /// API key can be configured from appsettings.json or environment variables.
    /// 
    /// Example appsettings.json:
    /// <code>
    /// {
    ///   "OpenRouter": {
    ///     "ApiKey": "your-api-key-here",
    ///     "BaseUrl": "https://openrouter.ai/api/v1",
    ///     "SiteName": "MyApp",
    ///     "SiteUrl": "https://myapp.com"
    ///   }
    /// }
    /// </code>
    /// 
    /// Example Program.cs (ASP.NET Core):
    /// <code>
    /// builder.Services.AddOpenRouter(builder.Configuration);
    /// </code>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration object.</param>
    /// <param name="sectionName">The configuration section name (default: "OpenRouter").</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenRouter(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "OpenRouter")
    {
        // Bind configuration options
        services.Configure<OpenRouterOptions>(configuration.GetSection(sectionName));

        // Register core services
        RegisterCoreServices(services);

        return services;
    }

    /// <summary>
    /// Adds OpenRouter SDK services to the service collection with manual configuration.
    /// 
    /// Example:
    /// <code>
    /// builder.Services.AddOpenRouter(options =>
    /// {
    ///     options.ApiKey = "your-api-key-here";
    ///     options.SiteName = "MyApp";
    ///     options.SiteUrl = "https://myapp.com";
    /// });
    /// </code>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for OpenRouter options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenRouter(
        this IServiceCollection services,
        Action<OpenRouterOptions> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        // Configure options manually
        services.Configure(configure);

        // Register core services
        RegisterCoreServices(services);

        return services;
    }

    /// <summary>
    /// Adds OpenRouter SDK services to the service collection with API key.
    /// 
    /// Example:
    /// <code>
    /// builder.Services.AddOpenRouter("your-api-key-here");
    /// </code>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">The OpenRouter API key.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenRouter(
        this IServiceCollection services,
        string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or whitespace.", nameof(apiKey));

        services.Configure<OpenRouterOptions>(options => options.ApiKey = apiKey);

        // Register core services
        RegisterCoreServices(services);

        return services;
    }

    private static void RegisterCoreServices(IServiceCollection services)
    {
        // Register IHttpClientService using RestSharp (no HttpClient needed)
        services.TryAddScoped<IHttpClientService>(serviceProvider =>
        {
            // Get configured options
            var optionsMonitor = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<OpenRouterOptions>>();
            var options = optionsMonitor?.Value ?? new OpenRouterOptions { ApiKey = string.Empty };

            // Get logger if available
            var logger = serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<HttpClientService>>();

            return new HttpClientService(options, logger);
        });

        // Register all service interfaces
        services.TryAddScoped<IChatService, ChatService>();
        services.TryAddScoped<IModelsService, ModelsService>();
        services.TryAddScoped<IEmbeddingsService, EmbeddingsService>();
        services.TryAddScoped<IProvidersService, ProvidersService>();
        services.TryAddScoped<IEndpointsService, EndpointsService>();
        services.TryAddScoped<IGenerationsService, GenerationsService>();
        services.TryAddScoped<IOAuthService, OAuthService>();
        services.TryAddScoped<IApiKeysService, ApiKeysService>();
        services.TryAddScoped<IAnalyticsService, AnalyticsService>();
        services.TryAddScoped<IGuardrailsService, GuardrailsService>();
        services.TryAddScoped<BetaServices>();

        // Register the main client
        services.TryAddScoped<IOpenRouterClient, OpenRouterClient>();
        services.TryAddScoped<OpenRouterClient>();
    }
}
