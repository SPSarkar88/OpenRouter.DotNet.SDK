using OpenRouter.SDK.Core;
using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Interface for API Keys service operations
/// Requires a provisioning key for authentication
/// </summary>
public interface IApiKeysService
{
    /// <summary>
    /// List all API keys for the authenticated user
    /// </summary>
    /// <param name="includeDisabled">Whether to include disabled API keys in the list</param>
    /// <param name="offset">Pagination offset for the list</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of API keys</returns>
    Task<ListApiKeysResponse> ListAsync(bool? includeDisabled = null, int? offset = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create a new API key for the authenticated user
    /// </summary>
    /// <param name="request">Request with API key configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created API key with the actual key string (only shown once)</returns>
    Task<CreateApiKeyResponse> CreateAsync(CreateApiKeyRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update an existing API key
    /// </summary>
    /// <param name="request">Request with hash and updated fields</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated API key information</returns>
    Task<UpdateApiKeyResponse> UpdateAsync(UpdateApiKeyRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete an existing API key
    /// </summary>
    /// <param name="hash">The hash identifier of the API key to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confirmation of deletion</returns>
    Task<DeleteApiKeyResponse> DeleteAsync(string hash, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a single API key by hash
    /// </summary>
    /// <param name="hash">The hash identifier of the API key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API key information</returns>
    Task<GetApiKeyResponse> GetAsync(string hash, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get information on the API key associated with the current authentication session
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current API key metadata</returns>
    Task<GetCurrentApiKeyResponse> GetCurrentKeyMetadataAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for managing API keys
/// All operations require a provisioning key for authentication
/// </summary>
public class ApiKeysService : IApiKeysService
{
    private readonly IHttpClientService _httpClientService;
    /// <summary>
    /// Constructor for ApiKeysService
    /// </summary>
    /// <param name="httpClientService">HTTP client service for making API requests</param>
    public ApiKeysService(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
    }

    /// <summary>
    /// List all API keys for the authenticated user
    /// Provisioning key required
    /// </summary>
    public async Task<ListApiKeysResponse> ListAsync(
        bool? includeDisabled = null, 
        int? offset = null, 
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>();
        
        if (includeDisabled.HasValue)
        {
            queryParams["include_disabled"] = includeDisabled.Value.ToString().ToLowerInvariant();
        }
        
        if (offset.HasValue)
        {
            queryParams["offset"] = offset.Value.ToString();
        }
        
        var queryString = queryParams.Count > 0 
            ? "?" + string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))
            : "";
        
        return await _httpClientService.GetAsync<ListApiKeysResponse>(
            $"/keys{queryString}",
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Create a new API key for the authenticated user
    /// Provisioning key required
    /// </summary>
    public async Task<CreateApiKeyResponse> CreateAsync(
        CreateApiKeyRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Name is required", nameof(request));
        }
        
        return await _httpClientService.PostJsonAsync<CreateApiKeyRequest, CreateApiKeyResponse>(
            "/keys",
            request,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Update an existing API key
    /// Provisioning key required
    /// </summary>
    public async Task<UpdateApiKeyResponse> UpdateAsync(
        UpdateApiKeyRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        
        if (string.IsNullOrWhiteSpace(request.Hash))
        {
            throw new ArgumentException("Hash is required", nameof(request));
        }
        
        // Extract hash from request
        var hash = request.Hash;
        
        // Create update body without the hash field
        var updateBody = new
        {
            name = request.Name,
            disabled = request.Disabled,
            limit = request.Limit,
            limit_reset = request.LimitReset,
            include_byok_in_limit = request.IncludeByokInLimit
        };
        
        return await _httpClientService.PatchJsonAsync<object, UpdateApiKeyResponse>(
            $"/keys/{hash}",
            updateBody,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Delete an existing API key
    /// Provisioning key required
    /// </summary>
    public async Task<DeleteApiKeyResponse> DeleteAsync(
        string hash, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException("Hash cannot be null or empty", nameof(hash));
        }
        
        return await _httpClientService.DeleteAsync<DeleteApiKeyResponse>(
            $"/keys/{hash}",
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Get a single API key by hash
    /// Provisioning key required
    /// </summary>
    public async Task<GetApiKeyResponse> GetAsync(
        string hash, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException("Hash cannot be null or empty", nameof(hash));
        }
        
        return await _httpClientService.GetAsync<GetApiKeyResponse>(
            $"/keys/{hash}",
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Get information on the API key associated with the current authentication session
    /// </summary>
    public async Task<GetCurrentApiKeyResponse> GetCurrentKeyMetadataAsync(
        CancellationToken cancellationToken = default)
    {
        return await _httpClientService.GetAsync<GetCurrentApiKeyResponse>(
            "/key",
            cancellationToken: cancellationToken
        );
    }
}
