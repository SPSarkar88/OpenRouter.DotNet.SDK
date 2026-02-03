using System.Net.Http.Json;

namespace OpenRouter.SDK.Core;

/// <summary>
/// Extension methods for IHttpClientService
/// </summary>
public static class HttpClientServiceExtensions
{
    /// <summary>
    /// Sends a POST request with JSON body and returns the deserialized response.
    /// </summary>
    public static Task<TResponse> PostJsonAsync<TResponse>(
        this IHttpClientService httpClient,
        string path,
        object body,
        CancellationToken cancellationToken = default)
    {
        return httpClient.PostJsonAsync<object, TResponse>(path, body, null, cancellationToken);
    }

    /// <summary>
    /// Sends a GET request and returns the deserialized response.
    /// </summary>
    public static Task<TResponse> GetJsonAsync<TResponse>(
        this IHttpClientService httpClient,
        string path,
        CancellationToken cancellationToken = default)
    {
        return httpClient.GetAsync<TResponse>(path, null, cancellationToken);
    }

    /// <summary>
    /// Sends a PATCH request with JSON body and returns the deserialized response.
    /// </summary>
    public static Task<TResponse> PatchJsonAsync<TResponse>(
        this IHttpClientService httpClient,
        string path,
        object body,
        CancellationToken cancellationToken = default)
    {
        return httpClient.PatchJsonAsync<object, TResponse>(path, body, null, cancellationToken);
    }

    /// <summary>
    /// Sends a DELETE request without a response body.
    /// </summary>
    public static async Task DeleteAsync(
        this IHttpClientService httpClient,
        string path,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, path);
        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Sends a DELETE request with JSON body and returns the deserialized response.
    /// </summary>
    public static async Task<TResponse> DeleteAsync<TResponse>(
        this IHttpClientService httpClient,
        string path,
        object body,
        CancellationToken cancellationToken = default)
    {
        var request = await httpClient.CreateRequestMessage(HttpMethod.Delete, path, body, null);
        var response = await httpClient.SendAsync(request, cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
            return content ?? throw new InvalidOperationException("Response content was null");
        }

        response.EnsureSuccessStatusCode();
        throw new InvalidOperationException("This code should not be reached");
    }
}
