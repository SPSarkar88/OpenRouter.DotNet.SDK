using Microsoft.Extensions.Logging;
using OpenRouter.SDK.Exceptions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using RestSharp;

namespace OpenRouter.SDK.Core;

/// <summary>
/// HTTP client service for making requests to the OpenRouter API.
/// </summary>
public interface IHttpClientService
{
    /// <summary>
    /// Sends an HTTP request.
    /// </summary>
    Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a JSON POST request and returns the deserialized response.
    /// </summary>
    Task<TResponse> PostJsonAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a GET request and returns the deserialized response.
    /// </summary>
    Task<TResponse> GetAsync<TResponse>(
        string path,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PATCH request with JSON body and returns the deserialized response.
    /// </summary>
    Task<TResponse> PatchJsonAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request and returns the deserialized response.
    /// </summary>
    Task<TResponse> DeleteAsync<TResponse>(
        string path,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an HTTP request message with the specified method and body.
    /// </summary>
    Task<HttpRequestMessage> CreateRequestMessage<TRequest>(
        HttpMethod method,
        string path,
        TRequest body,
        RequestOptions? options = null);

    /// <summary>
    /// Adds a hook that is called before a request is sent.
    /// </summary>
    void AddBeforeRequestHook(Func<HttpRequestMessage, Task<HttpRequestMessage?>> hook);

    /// <summary>
    /// Adds a hook that is called after a response is received.
    /// </summary>
    void AddResponseHook(Func<HttpResponseMessage, HttpRequestMessage, Task> hook);

    /// <summary>
    /// Adds a hook that is called when a request error occurs.
    /// </summary>
    void AddErrorHook(Func<Exception, HttpRequestMessage, Task> hook);
}

/// <summary>
/// Default implementation of the HTTP client service using RestSharp.
/// </summary>
public class HttpClientService : IHttpClientService
{
    private readonly RestClient _restClient;
    private readonly OpenRouterOptions _options;
    private readonly ILogger<HttpClientService>? _logger;
    private readonly List<Func<HttpRequestMessage, Task<HttpRequestMessage?>>> _beforeRequestHooks = new();
    private readonly List<Func<HttpResponseMessage, HttpRequestMessage, Task>> _responseHooks = new();
    private readonly List<Func<Exception, HttpRequestMessage, Task>> _errorHooks = new();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters = 
        { 
            new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) 
        }
    };

    /// <summary>
    /// Gets the JSON serialization options used for API requests and responses.
    /// </summary>
    public JsonSerializerOptions JsonOptions => _jsonOptions;
    
    /// <summary>
    /// Constructor for HttpClientService
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public HttpClientService(
        OpenRouterOptions options,
        ILogger<HttpClientService>? logger = null)
    {
        _options = options;
        _logger = logger;

        var restOptions = new RestClientOptions(_options.BaseUrl);
        
        _restClient = new RestClient(restOptions);
        ConfigureRestClient();
    }
    
    /// <summary>
    /// Constructor for HttpClientService (legacy compatibility - wraps HttpClient)
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    [Obsolete("This constructor is deprecated. Use the constructor without HttpClient parameter.")]
    public HttpClientService(
        HttpClient httpClient,
        OpenRouterOptions options,
        ILogger<HttpClientService>? logger = null)
        : this(options, logger)
    {
        // Legacy constructor - just use the new implementation
    }

    private void ConfigureRestClient()
    {
        // RestSharp handles authentication differently - we'll add it per request
        // No default headers configuration needed as RestClient handles this better
    }
    /// <summary>
    /// Sends an HTTP request using RestSharp but returning HttpResponseMessage for compatibility.
    /// </summary>
    /// <param name="request">The HTTP request message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        // Apply before request hooks
        foreach (var hook in _beforeRequestHooks)
        {
            var modifiedRequest = await hook(request);
            if (modifiedRequest != null)
            {
                request = modifiedRequest;
            }
        }

        try
        {
            _logger?.LogDebug("Sending {Method} request to {Uri}", request.Method, request.RequestUri);
            
            // Convert HttpRequestMessage to RestRequest
            var restRequest = await ConvertToRestRequest(request, cancellationToken);
            
            // Execute with RestSharp
            var restResponse = await _restClient.ExecuteAsync(restRequest, cancellationToken);
            
            // Convert RestResponse back to HttpResponseMessage
            var response = ConvertToHttpResponseMessage(restResponse, request.RequestUri!);

            // Apply response hooks
            foreach (var hook in _responseHooks)
            {
                await hook(response, request);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Request failed to {Uri}", request.RequestUri);

            // Apply error hooks
            foreach (var hook in _errorHooks)
            {
                await hook(ex, request);
            }

            throw;
        }
    }

    private async Task<RestRequest> ConvertToRestRequest(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var method = request.Method.Method.ToUpper() switch
        {
            "GET" => Method.Get,
            "POST" => Method.Post,
            "PUT" => Method.Put,
            "DELETE" => Method.Delete,
            "PATCH" => Method.Patch,
            "HEAD" => Method.Head,
            "OPTIONS" => Method.Options,
            _ => Method.Get
        };

        // Handle both absolute and relative URIs
        var path = request.RequestUri?.IsAbsoluteUri == true 
            ? request.RequestUri.PathAndQuery 
            : request.RequestUri?.ToString() ?? "/";
            
        var restRequest = new RestRequest(path, method);
        
        // Add authorization header
        var apiKey = _options.ApiKey;
        if (!apiKey.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            apiKey = $"Bearer {apiKey}";
        }
        restRequest.AddHeader("Authorization", apiKey);
        
        // Add default headers
        if (_options.DefaultHeaders != null)
        {
            foreach (var header in _options.DefaultHeaders)
            {
                restRequest.AddHeader(header.Key, header.Value);
            }
        }

        // Add headers from request
        foreach (var header in request.Headers)
        {
            if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                continue; // Already added
                
            foreach (var value in header.Value)
            {
                restRequest.AddHeader(header.Key, value);
            }
        }

        // Add content if present
        if (request.Content != null)
        {
            var content = await request.Content.ReadAsStringAsync(cancellationToken);
            var contentType = request.Content.Headers.ContentType?.MediaType ?? "application/json";
            
            restRequest.AddStringBody(content, contentType);
            
            _logger?.LogDebug("Body: {Body}", content.Length > 500 ? content.Substring(0, 500) : content);
        }

        return restRequest;
    }

    private HttpResponseMessage ConvertToHttpResponseMessage(RestResponse restResponse, Uri requestUri)
    {
        var response = new HttpResponseMessage((HttpStatusCode)(restResponse.StatusCode))
        {
            Content = new StringContent(restResponse.Content ?? string.Empty),
            RequestMessage = new HttpRequestMessage { RequestUri = requestUri }
        };

        // Copy headers
        if (restResponse.Headers != null)
        {
            foreach (var header in restResponse.Headers)
            {
                if (header.Name != null && header.Value != null)
                {
                    response.Headers.TryAddWithoutValidation(header.Name, header.Value.ToString());
                }
            }
        }

        // Set content type
        if (restResponse.ContentType != null)
        {
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(restResponse.ContentType);
        }

        return response;
    }
    /// <summary>
    /// Sends a JSON POST request and returns the deserialized response.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="path"></param>
    /// <param name="body"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The deserialized response</returns>
    public async Task<TResponse> PostJsonAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };

        ApplyRequestOptions(request, options);

        var response = await SendAsync(request, cancellationToken);
        return await DeserializeResponseAsync<TResponse>(response, cancellationToken);
    }
    /// <summary>
    ///  Sends a GET request and returns the deserialized response.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="path"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The deserialized response</returns>
    public async Task<TResponse> GetAsync<TResponse>(
        string path,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        ApplyRequestOptions(request, options);

        var response = await SendAsync(request, cancellationToken);
        return await DeserializeResponseAsync<TResponse>(response, cancellationToken);
    }
    /// <summary>
    /// Sends a PATCH request with JSON body and returns the deserialized response.
    /// </summary>
    /// <typeparam name="TRequest">Type of the request body</typeparam>
    /// <typeparam name="TResponse">Type of the response</typeparam>
    /// <param name="path">API endpoint path</param>
    /// <param name="body">Request body</param>
    /// <param name="options">Optional request options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<TResponse> PatchJsonAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, path)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };

        ApplyRequestOptions(request, options);

        var response = await SendAsync(request, cancellationToken);
        return await DeserializeResponseAsync<TResponse>(response, cancellationToken);
    }
    /// <summary>
    /// Sends a DELETE request and returns the deserialized response.
    /// </summary>
    /// <typeparam name="TResponse">Type of the response</typeparam>
    /// <param name="path">API endpoint path</param>
    /// <param name="options">Optional request options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deserialized response</returns>
    public async Task<TResponse> DeleteAsync<TResponse>(
        string path,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, path);
        ApplyRequestOptions(request, options);

        var response = await SendAsync(request, cancellationToken);
        return await DeserializeResponseAsync<TResponse>(response, cancellationToken);
    }

    private void ApplyRequestOptions(HttpRequestMessage request, RequestOptions? options)
    {
        if (options?.Headers != null)
        {
            foreach (var header in options.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        if (!string.IsNullOrEmpty(options?.ServerUrl))
        {
            request.RequestUri = new Uri(options.ServerUrl + request.RequestUri?.PathAndQuery);
        }
    }

    private async Task<T> DeserializeResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        var contentType = response.Content.Headers.ContentType?.MediaType;
        var statusCode = (int)response.StatusCode;
        
        _logger?.LogDebug("Response Status: {StatusCode}, Content-Type: {ContentType}", statusCode, contentType);
        
        if (response.IsSuccessStatusCode)
        {
            // Check if the response is actually JSON
            if (contentType != null && !contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
            {
                var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger?.LogError("Expected JSON but received {ContentType}. Content: {Content}", 
                    contentType, rawContent.Length > 500 ? rawContent.Substring(0, 500) + "..." : rawContent);
                throw new InvalidOperationException(
                    $"API returned {contentType} instead of JSON (Status: {statusCode}). This usually indicates an API configuration issue or invalid endpoint. First 200 chars: {(rawContent.Length > 200 ? rawContent.Substring(0, 200) : rawContent)}");
            }
            
            try
            {
                var content = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
                return content ?? throw new InvalidOperationException("Response content was null");
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger?.LogError("JSON deserialization failed. Content: {Content}", 
                    rawContent.Length > 500 ? rawContent.Substring(0, 500) + "..." : rawContent);
                throw new InvalidOperationException(
                    $"Failed to parse JSON response. First 200 chars: {(rawContent.Length > 200 ? rawContent.Substring(0, 200) : rawContent)}", jsonEx);
            }
        }

        await ThrowForErrorResponseAsync(response, cancellationToken);
        throw new InvalidOperationException("This code should not be reached");
    }

    private async Task ThrowForErrorResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var requestId = response.Headers.TryGetValues("X-Request-ID", out var values)
            ? values.FirstOrDefault()
            : null;

        _logger?.LogError("HTTP {StatusCode} error: {ErrorBody}", (int)response.StatusCode, errorBody);

        switch (response.StatusCode)
        {
            case HttpStatusCode.BadRequest:
                throw new BadRequestException("Bad request - Invalid parameters", errorBody, requestId);

            case HttpStatusCode.Unauthorized:
                throw new UnauthorizedException("Unauthorized - Invalid API key");

            case HttpStatusCode.Forbidden:
                throw new ForbiddenException("Forbidden - Insufficient permissions");

            case HttpStatusCode.NotFound:
                throw new NotFoundException("Resource not found");

            case HttpStatusCode.RequestEntityTooLarge:
                throw new PayloadTooLargeException("Request payload too large");

            case (HttpStatusCode)429:
                var retryAfter = GetRetryAfter(response);
                throw new RateLimitException("Rate limit exceeded", retryAfter);

            case HttpStatusCode.InternalServerError:
                throw new InternalServerErrorException("Internal server error");

            case HttpStatusCode.ServiceUnavailable:
                throw new ServiceUnavailableException("Service temporarily unavailable");

            case (HttpStatusCode)529:
                throw new ProviderOverloadedException("Provider is currently overloaded");

            default:
                throw new OpenRouterException(
                    $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}",
                    (int)response.StatusCode,
                    errorBody,
                    requestId);
        }
    }

    private TimeSpan? GetRetryAfter(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("Retry-After", out var values))
        {
            var retryAfterValue = values.FirstOrDefault();
            if (int.TryParse(retryAfterValue, out var seconds))
            {
                return TimeSpan.FromSeconds(seconds);
            }
        }
        return null;
    }
    /// <summary>
    /// Adds a hook that is called before a request is sent.
    /// </summary>
    /// <param name="hook">The hook function</param>
    public void AddBeforeRequestHook(Func<HttpRequestMessage, Task<HttpRequestMessage?>> hook)
    {
        _beforeRequestHooks.Add(hook);
    }
    /// <summary>
    /// Adds a hook that is called after a response is received.
    /// </summary>
    /// <param name="hook">The hook function</param>
    public void AddResponseHook(Func<HttpResponseMessage, HttpRequestMessage, Task> hook)
    {
        _responseHooks.Add(hook);
    }
    /// <summary>
    /// Adds a hook that is called when a request error occurs.
    /// </summary>
    /// <param name="hook">The hook function</param>
    public void AddErrorHook(Func<Exception, HttpRequestMessage, Task> hook)
    {
        _errorHooks.Add(hook);
    }

    /// <summary>
    /// Creates an HTTP request message with JSON content
    /// </summary>
    public Task<HttpRequestMessage> CreateRequestMessage<TRequest>(
        HttpMethod method,
        string path,
        TRequest body,
        RequestOptions? options = null)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };

        ApplyRequestOptions(request, options);
        return Task.FromResult(request);
    }
}

