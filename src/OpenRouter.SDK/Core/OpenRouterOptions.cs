namespace OpenRouter.SDK;

/// <summary>
/// Configuration options for the OpenRouter client.
/// </summary>
public class OpenRouterOptions
{
    /// <summary>
    /// Gets or sets the API key for authentication.
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the OpenRouter API.
    /// Default is "https://openrouter.ai/api/v1".
    /// </summary>
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";

    /// <summary>
    /// Gets or sets the default timeout for HTTP requests.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets or sets the retry configuration.
    /// </summary>
    public RetryConfiguration? RetryConfig { get; set; }

    /// <summary>
    /// Gets or sets the default headers to include with all requests.
    /// </summary>
    public Dictionary<string, string>? DefaultHeaders { get; set; }

    /// <summary>
    /// Gets or sets the HTTP referer header.
    /// </summary>
    public string? HttpReferer { get; set; }

    /// <summary>
    /// Gets or sets the site URL for attribution.
    /// </summary>
    public string? SiteUrl { get; set; }

    /// <summary>
    /// Gets or sets the app name for attribution.
    /// </summary>
    public string? AppName { get; set; }
}

/// <summary>
/// Configuration for HTTP retry policies.
/// </summary>
public class RetryConfiguration
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// Default is 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial delay between retries.
    /// Default is 1 second.
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the backoff multiplier for exponential backoff.
    /// Default is 2.0.
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Gets or sets the maximum delay between retries.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the HTTP status codes that should trigger a retry.
    /// </summary>
    public int[]? RetryCodes { get; set; }
}

/// <summary>
/// Options for individual API requests.
/// </summary>
public class RequestOptions
{
    /// <summary>
    /// Gets or sets the timeout for this specific request.
    /// Overrides the default timeout from OpenRouterOptions.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets or sets the retry configuration for this specific request.
    /// Overrides the default retry configuration from OpenRouterOptions.
    /// </summary>
    public RetryConfiguration? Retries { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status codes that should be retried for this request.
    /// </summary>
    public int[]? RetryCodes { get; set; }

    /// <summary>
    /// Gets or sets the server URL override for this specific request.
    /// </summary>
    public string? ServerUrl { get; set; }

    /// <summary>
    /// Gets or sets additional headers for this specific request.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }
}
