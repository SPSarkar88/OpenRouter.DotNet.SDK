using System.Text.Json;
using OpenRouter.SDK.Core;
using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Interface for the Guardrails service
/// </summary>
public interface IGuardrailsService
{
    /// <summary>
    /// List all guardrails for the authenticated user
    /// </summary>
    /// <param name="offset">Number of records to skip for pagination</param>
    /// <param name="limit">Maximum number of records to return (max 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of guardrails</returns>
    Task<ListGuardrailsResponse> ListGuardrailsAsync(string? offset = null, string? limit = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create a new guardrail
    /// </summary>
    /// <param name="request">Guardrail creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created guardrail</returns>
    Task<GuardrailResponse> CreateGuardrailAsync(CreateGuardrailRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a single guardrail by ID
    /// </summary>
    /// <param name="id">Guardrail ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Guardrail details</returns>
    Task<GuardrailResponse> GetGuardrailAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update an existing guardrail
    /// </summary>
    /// <param name="id">Guardrail ID</param>
    /// <param name="request">Update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated guardrail</returns>
    Task<GuardrailResponse> UpdateGuardrailAsync(string id, UpdateGuardrailRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete an existing guardrail
    /// </summary>
    /// <param name="id">Guardrail ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task DeleteGuardrailAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// List all API key guardrail assignments
    /// </summary>
    /// <param name="offset">Number of records to skip for pagination</param>
    /// <param name="limit">Maximum number of records to return (max 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of key assignments</returns>
    Task<ListKeyAssignmentsResponse> ListKeyAssignmentsAsync(string? offset = null, string? limit = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// List all organization member guardrail assignments
    /// </summary>
    /// <param name="offset">Number of records to skip for pagination</param>
    /// <param name="limit">Maximum number of records to return (max 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of member assignments</returns>
    Task<ListMemberAssignmentsResponse> ListMemberAssignmentsAsync(string? offset = null, string? limit = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// List all API key assignments for a specific guardrail
    /// </summary>
    /// <param name="id">Guardrail ID</param>
    /// <param name="offset">Number of records to skip for pagination</param>
    /// <param name="limit">Maximum number of records to return (max 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of key assignments</returns>
    Task<ListKeyAssignmentsResponse> ListGuardrailKeyAssignmentsAsync(string id, string? offset = null, string? limit = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Assign multiple API keys to a specific guardrail
    /// </summary>
    /// <param name="id">Guardrail ID</param>
    /// <param name="request">Bulk assignment request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    Task<BulkOperationResponse> BulkAssignKeysAsync(string id, BulkAssignKeysRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// List all organization member assignments for a specific guardrail
    /// </summary>
    /// <param name="id">Guardrail ID</param>
    /// <param name="offset">Number of records to skip for pagination</param>
    /// <param name="limit">Maximum number of records to return (max 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of member assignments</returns>
    Task<ListMemberAssignmentsResponse> ListGuardrailMemberAssignmentsAsync(string id, string? offset = null, string? limit = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Assign multiple organization members to a specific guardrail
    /// </summary>
    /// <param name="id">Guardrail ID</param>
    /// <param name="request">Bulk assignment request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    Task<BulkOperationResponse> BulkAssignMembersAsync(string id, BulkAssignMembersRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unassign multiple API keys from a specific guardrail
    /// </summary>
    /// <param name="id">Guardrail ID</param>
    /// <param name="request">Bulk unassignment request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    Task<BulkOperationResponse> BulkUnassignKeysAsync(string id, BulkAssignKeysRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unassign multiple organization members from a specific guardrail
    /// </summary>
    /// <param name="id">Guardrail ID</param>
    /// <param name="request">Bulk unassignment request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    Task<BulkOperationResponse> BulkUnassignMembersAsync(string id, BulkAssignMembersRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for managing guardrails (content moderation rules)
/// Requires a provisioning key for authentication
/// </summary>
public class GuardrailsService : IGuardrailsService
{
    private readonly IHttpClientService _httpClient;

    public GuardrailsService(IHttpClientService httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<ListGuardrailsResponse> ListGuardrailsAsync(string? offset = null, string? limit = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(offset))
            queryParams.Add($"offset={Uri.EscapeDataString(offset)}");
        if (!string.IsNullOrEmpty(limit))
            queryParams.Add($"limit={Uri.EscapeDataString(limit)}");
        
        var endpoint = queryParams.Count > 0 
            ? $"/guardrails?{string.Join("&", queryParams)}"
            : "/guardrails";
        
        return await _httpClient.GetJsonAsync<ListGuardrailsResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GuardrailResponse> CreateGuardrailAsync(CreateGuardrailRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
        
        return await _httpClient.PostJsonAsync<GuardrailResponse>("/guardrails", request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GuardrailResponse> GetGuardrailAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        
        return await _httpClient.GetJsonAsync<GuardrailResponse>($"/guardrails/{Uri.EscapeDataString(id)}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GuardrailResponse> UpdateGuardrailAsync(string id, UpdateGuardrailRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(request);
        
        return await _httpClient.PatchJsonAsync<GuardrailResponse>($"/guardrails/{Uri.EscapeDataString(id)}", request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteGuardrailAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        
        await _httpClient.DeleteAsync($"/guardrails/{Uri.EscapeDataString(id)}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ListKeyAssignmentsResponse> ListKeyAssignmentsAsync(string? offset = null, string? limit = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(offset))
            queryParams.Add($"offset={Uri.EscapeDataString(offset)}");
        if (!string.IsNullOrEmpty(limit))
            queryParams.Add($"limit={Uri.EscapeDataString(limit)}");
        
        var endpoint = queryParams.Count > 0 
            ? $"/guardrails/keys/assignments?{string.Join("&", queryParams)}"
            : "/guardrails/keys/assignments";
        
        return await _httpClient.GetJsonAsync<ListKeyAssignmentsResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ListMemberAssignmentsResponse> ListMemberAssignmentsAsync(string? offset = null, string? limit = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(offset))
            queryParams.Add($"offset={Uri.EscapeDataString(offset)}");
        if (!string.IsNullOrEmpty(limit))
            queryParams.Add($"limit={Uri.EscapeDataString(limit)}");
        
        var endpoint = queryParams.Count > 0 
            ? $"/guardrails/members/assignments?{string.Join("&", queryParams)}"
            : "/guardrails/members/assignments";
        
        return await _httpClient.GetJsonAsync<ListMemberAssignmentsResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ListKeyAssignmentsResponse> ListGuardrailKeyAssignmentsAsync(string id, string? offset = null, string? limit = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(offset))
            queryParams.Add($"offset={Uri.EscapeDataString(offset)}");
        if (!string.IsNullOrEmpty(limit))
            queryParams.Add($"limit={Uri.EscapeDataString(limit)}");
        
        var endpoint = queryParams.Count > 0 
            ? $"/guardrails/{Uri.EscapeDataString(id)}/keys?{string.Join("&", queryParams)}"
            : $"/guardrails/{Uri.EscapeDataString(id)}/keys";
        
        return await _httpClient.GetJsonAsync<ListKeyAssignmentsResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BulkOperationResponse> BulkAssignKeysAsync(string id, BulkAssignKeysRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.KeyHashes);
        
        if (request.KeyHashes.Count == 0)
            throw new ArgumentException("KeyHashes cannot be empty", nameof(request));
        
        return await _httpClient.PostJsonAsync<BulkOperationResponse>($"/guardrails/{Uri.EscapeDataString(id)}/keys", request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ListMemberAssignmentsResponse> ListGuardrailMemberAssignmentsAsync(string id, string? offset = null, string? limit = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(offset))
            queryParams.Add($"offset={Uri.EscapeDataString(offset)}");
        if (!string.IsNullOrEmpty(limit))
            queryParams.Add($"limit={Uri.EscapeDataString(limit)}");
        
        var endpoint = queryParams.Count > 0 
            ? $"/guardrails/{Uri.EscapeDataString(id)}/members?{string.Join("&", queryParams)}"
            : $"/guardrails/{Uri.EscapeDataString(id)}/members";
        
        return await _httpClient.GetJsonAsync<ListMemberAssignmentsResponse>(endpoint, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BulkOperationResponse> BulkAssignMembersAsync(string id, BulkAssignMembersRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.MemberIds);
        
        if (request.MemberIds.Count == 0)
            throw new ArgumentException("MemberIds cannot be empty", nameof(request));
        
        return await _httpClient.PostJsonAsync<BulkOperationResponse>($"/guardrails/{Uri.EscapeDataString(id)}/members", request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BulkOperationResponse> BulkUnassignKeysAsync(string id, BulkAssignKeysRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.KeyHashes);
        
        if (request.KeyHashes.Count == 0)
            throw new ArgumentException("KeyHashes cannot be empty", nameof(request));
        
        return await _httpClient.DeleteAsync<BulkOperationResponse>($"/guardrails/{Uri.EscapeDataString(id)}/keys", request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BulkOperationResponse> BulkUnassignMembersAsync(string id, BulkAssignMembersRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.MemberIds);
        
        if (request.MemberIds.Count == 0)
            throw new ArgumentException("MemberIds cannot be empty", nameof(request));
        
        return await _httpClient.DeleteAsync<BulkOperationResponse>($"/guardrails/{Uri.EscapeDataString(id)}/members", request, cancellationToken);
    }
}
