namespace OpenRouter.SDK.Exceptions;

/// <summary>
/// Base exception for all OpenRouter SDK errors.
/// </summary>
public class OpenRouterException : Exception
{
    /// <summary>
    /// Gets the HTTP status code if applicable.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Gets the response body if available.
    /// </summary>
    public string? ResponseBody { get; }

    /// <summary>
    /// Gets the request ID for tracking purposes.
    /// </summary>
    public string? RequestId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenRouterException"/> class.
    /// </summary>
    public OpenRouterException(
        string message, 
        int? statusCode = null,
        string? responseBody = null,
        string? requestId = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        RequestId = requestId;
    }
}

/// <summary>
/// Exception thrown when a request is malformed or contains invalid parameters.
/// HTTP 400 Bad Request.
/// </summary>
public class BadRequestException : OpenRouterException
{
    public BadRequestException(string message, string? responseBody = null, string? requestId = null)
        : base(message, 400, responseBody, requestId)
    {
    }
}

/// <summary>
/// Exception thrown when authentication fails or API key is invalid.
/// HTTP 401 Unauthorized.
/// </summary>
public class UnauthorizedException : OpenRouterException
{
    public UnauthorizedException(string message = "Unauthorized - Invalid API key")
        : base(message, 401)
    {
    }
}

/// <summary>
/// Exception thrown when the user doesn't have permission to access a resource.
/// HTTP 403 Forbidden.
/// </summary>
public class ForbiddenException : OpenRouterException
{
    public ForbiddenException(string message = "Forbidden - Insufficient permissions")
        : base(message, 403)
    {
    }
}

/// <summary>
/// Exception thrown when a requested resource is not found.
/// HTTP 404 Not Found.
/// </summary>
public class NotFoundException : OpenRouterException
{
    public NotFoundException(string message = "Resource not found")
        : base(message, 404)
    {
    }
}

/// <summary>
/// Exception thrown when the request payload is too large.
/// HTTP 413 Payload Too Large.
/// </summary>
public class PayloadTooLargeException : OpenRouterException
{
    public PayloadTooLargeException(string message = "Request payload too large")
        : base(message, 413)
    {
    }
}

/// <summary>
/// Exception thrown when rate limits are exceeded.
/// HTTP 429 Too Many Requests.
/// </summary>
public class RateLimitException : OpenRouterException
{
    /// <summary>
    /// Gets the time to wait before retrying.
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    public RateLimitException(string message = "Rate limit exceeded", TimeSpan? retryAfter = null)
        : base(message, 429)
    {
        RetryAfter = retryAfter;
    }
}

/// <summary>
/// Exception thrown when the server encounters an internal error.
/// HTTP 500 Internal Server Error.
/// </summary>
public class InternalServerErrorException : OpenRouterException
{
    public InternalServerErrorException(string message = "Internal server error")
        : base(message, 500)
    {
    }
}

/// <summary>
/// Exception thrown when the service is temporarily unavailable.
/// HTTP 503 Service Unavailable.
/// </summary>
public class ServiceUnavailableException : OpenRouterException
{
    public ServiceUnavailableException(string message = "Service temporarily unavailable")
        : base(message, 503)
    {
    }
}

/// <summary>
/// Exception thrown when a provider is overloaded.
/// HTTP 529 Provider Overloaded (custom status code).
/// </summary>
public class ProviderOverloadedException : OpenRouterException
{
    public ProviderOverloadedException(string message = "Provider is currently overloaded")
        : base(message, 529)
    {
    }
}

/// <summary>
/// Exception thrown when a connection error occurs.
/// </summary>
public class ConnectionException : OpenRouterException
{
    public ConnectionException(string message, Exception? innerException = null)
        : base(message, null, null, null, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a request times out.
/// </summary>
public class TimeoutException : OpenRouterException
{
    public TimeoutException(string message = "Request timed out")
        : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when a validation error occurs.
/// </summary>
public class ValidationException : OpenRouterException
{
    public ValidationException(string message)
        : base(message)
    {
    }
}
