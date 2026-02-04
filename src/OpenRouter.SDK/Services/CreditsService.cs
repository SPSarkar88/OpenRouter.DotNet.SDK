using System.Text.Json.Serialization;
using OpenRouter.SDK.Core;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Interface for Credits service operations
/// Provides credit balance and payment functionality
/// </summary>
public interface ICreditsService
{
    /// <summary>
    /// Get total credits purchased and used for the authenticated user.
    /// Provisioning key required.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Credits information including balance</returns>
    Task<GetCreditsResponse> GetCreditsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a Coinbase charge for crypto payment
    /// </summary>
    /// <param name="request">Charge creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Coinbase charge response with payment URL</returns>
    Task<CoinbaseChargeResponse> CreateCoinbaseChargeAsync(CreateCoinbaseChargeRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of Credits service
/// Requires a provisioning key for authentication
/// </summary>
public class CreditsService : ICreditsService
{
    private readonly IHttpClientService _httpClient;

    /// <summary>
    /// Constructor for CreditsService
    /// </summary>
    /// <param name="httpClient">HTTP client service for making API requests</param>
    public CreditsService(IHttpClientService httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<GetCreditsResponse> GetCreditsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync<GetCreditsResponse>(
            "/credits",
            cancellationToken: cancellationToken);

        return response;
    }

    /// <inheritdoc />
    public async Task<CoinbaseChargeResponse> CreateCoinbaseChargeAsync(
        CreateCoinbaseChargeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero", nameof(request));
        }

        var response = await _httpClient.PostJsonAsync<CreateCoinbaseChargeRequest, CoinbaseChargeResponse>(
            "/credits/coinbase",
            request,
            null,
            cancellationToken);

        return response;
    }
}

#region Request/Response Models

/// <summary>
/// Response from the get credits endpoint
/// </summary>
public class GetCreditsResponse
{
    /// <summary>
    /// Total credits purchased (in USD)
    /// </summary>
    [JsonPropertyName("total_credits")]
    public decimal TotalCredits { get; init; }

    /// <summary>
    /// Total credits used (in USD)
    /// </summary>
    [JsonPropertyName("total_usage")]
    public decimal TotalUsage { get; init; }

    /// <summary>
    /// Remaining credit balance (in USD)
    /// </summary>
    [JsonPropertyName("balance")]
    public decimal Balance { get; init; }

    /// <summary>
    /// Currency code (e.g., "USD")
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; init; }
}

/// <summary>
/// Request to create a Coinbase charge for crypto payment
/// </summary>
public class CreateCoinbaseChargeRequest
{
    /// <summary>
    /// Amount to charge in USD
    /// </summary>
    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }

    /// <summary>
    /// Optional sender name for the charge
    /// </summary>
    [JsonPropertyName("sender")]
    public string? Sender { get; init; }
}

/// <summary>
/// Response from creating a Coinbase charge
/// </summary>
public class CoinbaseChargeResponse
{
    /// <summary>
    /// Unique identifier for the charge
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Coinbase hosted checkout URL
    /// </summary>
    [JsonPropertyName("hosted_url")]
    public string? HostedUrl { get; init; }

    /// <summary>
    /// Charge code
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    /// <summary>
    /// Charge status
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>
    /// Expiration timestamp
    /// </summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; init; }
}

#endregion
