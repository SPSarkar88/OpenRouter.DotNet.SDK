using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

/// <summary>
/// Information about an AI provider available on OpenRouter
/// </summary>
public class ProviderData
{
    /// <summary>
    /// Display name of the provider
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// URL-friendly identifier for the provider
    /// </summary>
    [JsonPropertyName("slug")]
    public required string Slug { get; init; }

    /// <summary>
    /// URL to the provider's privacy policy
    /// </summary>
    [JsonPropertyName("privacy_policy_url")]
    public string? PrivacyPolicyUrl { get; init; }

    /// <summary>
    /// URL to the provider's terms of service
    /// </summary>
    [JsonPropertyName("terms_of_service_url")]
    public string? TermsOfServiceUrl { get; init; }

    /// <summary>
    /// URL to the provider's status page
    /// </summary>
    [JsonPropertyName("status_page_url")]
    public string? StatusPageUrl { get; init; }
}

/// <summary>
/// Response containing a list of providers
/// </summary>
public class ProvidersResponse
{
    /// <summary>
    /// List of available providers
    /// </summary>
    [JsonPropertyName("data")]
    public required List<ProviderData> Data { get; init; }
}
