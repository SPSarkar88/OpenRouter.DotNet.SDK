using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

/// <summary>
/// The method used to generate the code challenge
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<CodeChallengeMethod>))]
public enum CodeChallengeMethod
{
    /// <summary>
    /// SHA-256 hashing (recommended for security)
    /// </summary>
    [JsonPropertyName("S256")]
    S256,
    
    /// <summary>
    /// Plain text (not recommended)
    /// </summary>
    [JsonPropertyName("plain")]
    Plain
}

/// <summary>
/// Request to create an authorization code for PKCE flow
/// </summary>
public class CreateAuthCodeRequest
{
    /// <summary>
    /// The callback URL to redirect to after authorization. Only HTTPS URLs on ports 443 and 3000 are allowed.
    /// </summary>
    [JsonPropertyName("callback_url")]
    public required string CallbackUrl { get; init; }

    /// <summary>
    /// PKCE code challenge for enhanced security
    /// </summary>
    [JsonPropertyName("code_challenge")]
    public string? CodeChallenge { get; init; }

    /// <summary>
    /// The method used to generate the code challenge
    /// </summary>
    [JsonPropertyName("code_challenge_method")]
    public CodeChallengeMethod? CodeChallengeMethod { get; init; }

    /// <summary>
    /// Credit limit for the API key to be created
    /// </summary>
    [JsonPropertyName("limit")]
    public double? Limit { get; init; }

    /// <summary>
    /// Optional expiration time for the API key to be created
    /// </summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>
/// Auth code data returned from creating an authorization code
/// </summary>
public class AuthCodeData
{
    /// <summary>
    /// The authorization code ID to use in the exchange request
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// The application ID associated with this auth code
    /// </summary>
    [JsonPropertyName("app_id")]
    public required int AppId { get; init; }

    /// <summary>
    /// ISO 8601 timestamp of when the auth code was created
    /// </summary>
    [JsonPropertyName("created_at")]
    public required string CreatedAt { get; init; }
}

/// <summary>
/// Response containing the created authorization code
/// </summary>
public class CreateAuthCodeResponse
{
    /// <summary>
    /// Auth code data
    /// </summary>
    [JsonPropertyName("data")]
    public required AuthCodeData Data { get; init; }
}

/// <summary>
/// Request to exchange an authorization code for an API key
/// </summary>
public class ExchangeAuthCodeRequest
{
    /// <summary>
    /// The authorization code received from the OAuth redirect
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    /// <summary>
    /// The code verifier if code_challenge was used in the authorization request
    /// </summary>
    [JsonPropertyName("code_verifier")]
    public string? CodeVerifier { get; init; }

    /// <summary>
    /// The method used to generate the code challenge
    /// </summary>
    [JsonPropertyName("code_challenge_method")]
    public CodeChallengeMethod? CodeChallengeMethod { get; init; }
}

/// <summary>
/// Response containing the exchanged API key
/// </summary>
public class ExchangeAuthCodeResponse
{
    /// <summary>
    /// The API key to use for OpenRouter requests
    /// </summary>
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    /// <summary>
    /// User ID associated with the API key
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; init; }
}

/// <summary>
/// Result of creating a SHA-256 code challenge
/// </summary>
public class CodeChallengeResult
{
    /// <summary>
    /// The code challenge to send in the authorization request
    /// </summary>
    public required string CodeChallenge { get; init; }

    /// <summary>
    /// The code verifier to use when exchanging the authorization code
    /// </summary>
    public required string CodeVerifier { get; init; }
}

/// <summary>
/// Request parameters for creating an authorization URL
/// </summary>
public class CreateAuthorizationUrlRequest
{
    /// <summary>
    /// The callback URL to redirect to after authorization
    /// </summary>
    public required string CallbackUrl { get; init; }

    /// <summary>
    /// Optional code challenge for PKCE flow
    /// </summary>
    public string? CodeChallenge { get; init; }

    /// <summary>
    /// The method used to generate the code challenge
    /// </summary>
    public CodeChallengeMethod? CodeChallengeMethod { get; init; }

    /// <summary>
    /// Optional credit limit for the API key
    /// </summary>
    public double? Limit { get; init; }
}
