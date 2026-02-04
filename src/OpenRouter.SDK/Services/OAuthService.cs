using System.Security.Cryptography;
using System.Text;
using OpenRouter.SDK.Core;
using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Service for OAuth2 PKCE (Proof Key for Code Exchange) authentication flow
/// </summary>
public interface IOAuthService
{
    /// <summary>
    /// Generate a SHA-256 code challenge for PKCE flow
    /// </summary>
    /// <param name="codeVerifier">Optional code verifier (43-128 characters, unreserved chars only). If not provided, a random one will be generated.</param>
    /// <returns>Code challenge and verifier pair</returns>
    CodeChallengeResult CreateSHA256CodeChallenge(string? codeVerifier = null);

    /// <summary>
    /// Generate an OAuth2 authorization URL for user authentication
    /// </summary>
    /// <param name="request">Authorization URL parameters</param>
    /// <param name="baseUrl">Optional base URL (defaults to https://openrouter.ai)</param>
    /// <returns>The complete authorization URL to redirect users to</returns>
    string CreateAuthorizationUrl(CreateAuthorizationUrlRequest request, string? baseUrl = null);

    /// <summary>
    /// Create an authorization code for the PKCE flow
    /// </summary>
    /// <param name="request">Authorization code request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authorization code data</returns>
    Task<CreateAuthCodeResponse> CreateAuthCodeAsync(CreateAuthCodeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchange an authorization code for an API key
    /// </summary>
    /// <param name="request">Exchange request with code and verifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API key and user information</returns>
    Task<ExchangeAuthCodeResponse> ExchangeAuthCodeForAPIKeyAsync(ExchangeAuthCodeRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of OAuth service for PKCE authentication
/// </summary>
public class OAuthService : IOAuthService
{
    private readonly IHttpClientService _httpClient;
    /// <summary>
    /// Constructor for OAuthService
    /// </summary>
    /// <param name="httpClient">HTTP client service for making requests</param>
    /// <exception cref="ArgumentNullException">Thrown if httpClient is null</exception>
    public OAuthService(IHttpClientService httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public CodeChallengeResult CreateSHA256CodeChallenge(string? codeVerifier = null)
    {
        // Validate or generate code verifier
        if (codeVerifier != null)
        {
            if (codeVerifier.Length < 43 || codeVerifier.Length > 128)
            {
                throw new ArgumentException("Code verifier must be 43-128 characters", nameof(codeVerifier));
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(codeVerifier, @"^[A-Za-z0-9\-._~]+$"))
            {
                throw new ArgumentException("Code verifier must only contain unreserved characters: [A-Za-z0-9-._~]", nameof(codeVerifier));
            }
        }
        else
        {
            // Generate random code verifier (32 bytes = 43 chars base64url encoded)
            var randomBytes = new byte[32];
            RandomNumberGenerator.Fill(randomBytes);
            codeVerifier = Base64UrlEncode(randomBytes);
        }

        // Generate SHA-256 hash of code verifier
        var verifierBytes = Encoding.UTF8.GetBytes(codeVerifier);
        var hashBytes = SHA256.HashData(verifierBytes);
        var codeChallenge = Base64UrlEncode(hashBytes);

        return new CodeChallengeResult
        {
            CodeChallenge = codeChallenge,
            CodeVerifier = codeVerifier
        };
    }

    /// <inheritdoc />
    public string CreateAuthorizationUrl(CreateAuthorizationUrlRequest request, string? baseUrl = null)
    {
        if (string.IsNullOrWhiteSpace(request.CallbackUrl))
        {
            throw new ArgumentException("Callback URL cannot be null or empty", nameof(request.CallbackUrl));
        }

        // Validate callback URL
        if (!Uri.TryCreate(request.CallbackUrl, UriKind.Absolute, out var callbackUri))
        {
            throw new ArgumentException("Callback URL must be a valid absolute URL", nameof(request.CallbackUrl));
        }

        // Build authorization URL
        baseUrl ??= "https://openrouter.ai";
        var authUrl = new UriBuilder($"{baseUrl}/auth");
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);

        query["callback_url"] = request.CallbackUrl;

        if (!string.IsNullOrWhiteSpace(request.CodeChallenge))
        {
            query["code_challenge"] = request.CodeChallenge;
            query["code_challenge_method"] = request.CodeChallengeMethod?.ToString() ?? "S256";
        }

        if (request.Limit.HasValue)
        {
            query["limit"] = request.Limit.Value.ToString();
        }

        authUrl.Query = query.ToString();
        return authUrl.ToString();
    }

    /// <inheritdoc />
    public async Task<CreateAuthCodeResponse> CreateAuthCodeAsync(CreateAuthCodeRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CallbackUrl))
        {
            throw new ArgumentException("Callback URL cannot be null or empty", nameof(request.CallbackUrl));
        }

        var response = await _httpClient.PostJsonAsync<CreateAuthCodeRequest, CreateAuthCodeResponse>(
            "/auth/keys/code",
            request,
            cancellationToken: cancellationToken
        );

        return response;
    }

    /// <inheritdoc />
    public async Task<ExchangeAuthCodeResponse> ExchangeAuthCodeForAPIKeyAsync(ExchangeAuthCodeRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            throw new ArgumentException("Authorization code cannot be null or empty", nameof(request.Code));
        }

        var response = await _httpClient.PostJsonAsync<ExchangeAuthCodeRequest, ExchangeAuthCodeResponse>(
            "/auth/keys",
            request,
            cancellationToken: cancellationToken
        );

        return response;
    }

    /// <summary>
    /// Convert bytes to base64url encoding (RFC 4648)
    /// </summary>
    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
