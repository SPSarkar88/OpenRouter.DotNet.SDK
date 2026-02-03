using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

/// <summary>
/// Type of limit reset for the API key
/// Resets happen automatically at midnight UTC, and weeks are Monday through Sunday
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LimitReset
{
    /// <summary>
    /// Reset daily at midnight UTC
    /// </summary>
    [JsonPropertyName("daily")]
    Daily,
    
    /// <summary>
    /// Reset weekly (Monday through Sunday) at midnight UTC
    /// </summary>
    [JsonPropertyName("weekly")]
    Weekly,
    
    /// <summary>
    /// Reset monthly at midnight UTC
    /// </summary>
    [JsonPropertyName("monthly")]
    Monthly
}

/// <summary>
/// Request to create a new API key
/// </summary>
public class CreateApiKeyRequest
{
    /// <summary>
    /// Name for the new API key
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    /// <summary>
    /// Optional spending limit for the API key in USD
    /// </summary>
    [JsonPropertyName("limit")]
    public double? Limit { get; set; }
    
    /// <summary>
    /// Type of limit reset for the API key (daily, weekly, monthly, or null for no reset)
    /// </summary>
    [JsonPropertyName("limit_reset")]
    public LimitReset? LimitReset { get; set; }
    
    /// <summary>
    /// Whether to include BYOK usage in the limit
    /// </summary>
    [JsonPropertyName("include_byok_in_limit")]
    public bool? IncludeByokInLimit { get; set; }
    
    /// <summary>
    /// Optional ISO 8601 UTC timestamp when the API key should expire
    /// Must be UTC, other timezones will be rejected
    /// </summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// API key data returned from API operations
/// </summary>
public class ApiKeyData
{
    /// <summary>
    /// Unique hash identifier for the API key
    /// </summary>
    [JsonPropertyName("hash")]
    public required string Hash { get; set; }
    
    /// <summary>
    /// Name of the API key
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    /// <summary>
    /// Human-readable label for the API key
    /// </summary>
    [JsonPropertyName("label")]
    public required string Label { get; set; }
    
    /// <summary>
    /// Whether the API key is disabled
    /// </summary>
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }
    
    /// <summary>
    /// Spending limit for the API key in USD
    /// </summary>
    [JsonPropertyName("limit")]
    public double? Limit { get; set; }
    
    /// <summary>
    /// Remaining spending limit in USD
    /// </summary>
    [JsonPropertyName("limit_remaining")]
    public double? LimitRemaining { get; set; }
    
    /// <summary>
    /// Type of limit reset for the API key
    /// </summary>
    [JsonPropertyName("limit_reset")]
    public string? LimitResetValue { get; set; }
    
    /// <summary>
    /// Whether to include external BYOK usage in the credit limit
    /// </summary>
    [JsonPropertyName("include_byok_in_limit")]
    public bool IncludeByokInLimit { get; set; }
    
    /// <summary>
    /// Total OpenRouter credit usage (in USD) for the API key
    /// </summary>
    [JsonPropertyName("usage")]
    public double Usage { get; set; }
    
    /// <summary>
    /// OpenRouter credit usage (in USD) for the current UTC day
    /// </summary>
    [JsonPropertyName("usage_daily")]
    public double UsageDaily { get; set; }
    
    /// <summary>
    /// OpenRouter credit usage (in USD) for the current UTC week (Monday-Sunday)
    /// </summary>
    [JsonPropertyName("usage_weekly")]
    public double UsageWeekly { get; set; }
    
    /// <summary>
    /// OpenRouter credit usage (in USD) for the current UTC month
    /// </summary>
    [JsonPropertyName("usage_monthly")]
    public double UsageMonthly { get; set; }
    
    /// <summary>
    /// Total external BYOK usage (in USD) for the API key
    /// </summary>
    [JsonPropertyName("byok_usage")]
    public double ByokUsage { get; set; }
    
    /// <summary>
    /// External BYOK usage (in USD) for the current UTC day
    /// </summary>
    [JsonPropertyName("byok_usage_daily")]
    public double ByokUsageDaily { get; set; }
    
    /// <summary>
    /// External BYOK usage (in USD) for the current UTC week (Monday-Sunday)
    /// </summary>
    [JsonPropertyName("byok_usage_weekly")]
    public double ByokUsageWeekly { get; set; }
    
    /// <summary>
    /// External BYOK usage (in USD) for current UTC month
    /// </summary>
    [JsonPropertyName("byok_usage_monthly")]
    public double ByokUsageMonthly { get; set; }
    
    /// <summary>
    /// ISO 8601 timestamp of when the API key was created
    /// </summary>
    [JsonPropertyName("created_at")]
    public required string CreatedAt { get; set; }
    
    /// <summary>
    /// ISO 8601 timestamp of when the API key was last updated
    /// </summary>
    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; set; }
    
    /// <summary>
    /// ISO 8601 UTC timestamp when the API key expires, or null if no expiration
    /// </summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Response from creating a new API key
/// </summary>
public class CreateApiKeyResponse
{
    /// <summary>
    /// The created API key information
    /// </summary>
    [JsonPropertyName("data")]
    public required ApiKeyData Data { get; set; }
    
    /// <summary>
    /// The actual API key string (only shown once during creation)
    /// </summary>
    [JsonPropertyName("key")]
    public required string Key { get; set; }
}

/// <summary>
/// Request to update an existing API key
/// </summary>
public class UpdateApiKeyRequest
{
    /// <summary>
    /// The hash identifier of the API key to update
    /// </summary>
    public required string Hash { get; set; }
    
    /// <summary>
    /// New name for the API key
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    /// <summary>
    /// Whether to disable the API key
    /// </summary>
    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; }
    
    /// <summary>
    /// New spending limit for the API key in USD
    /// </summary>
    [JsonPropertyName("limit")]
    public double? Limit { get; set; }
    
    /// <summary>
    /// New limit reset type for the API key
    /// </summary>
    [JsonPropertyName("limit_reset")]
    public LimitReset? LimitReset { get; set; }
    
    /// <summary>
    /// Whether to include BYOK usage in the limit
    /// </summary>
    [JsonPropertyName("include_byok_in_limit")]
    public bool? IncludeByokInLimit { get; set; }
}

/// <summary>
/// Response from updating an API key
/// </summary>
public class UpdateApiKeyResponse
{
    /// <summary>
    /// The updated API key information
    /// </summary>
    [JsonPropertyName("data")]
    public required ApiKeyData Data { get; set; }
}

/// <summary>
/// Response from listing API keys
/// </summary>
public class ListApiKeysResponse
{
    /// <summary>
    /// List of API keys
    /// </summary>
    [JsonPropertyName("data")]
    public required List<ApiKeyData> Data { get; set; }
}

/// <summary>
/// Response from getting a single API key
/// </summary>
public class GetApiKeyResponse
{
    /// <summary>
    /// The API key data
    /// </summary>
    [JsonPropertyName("data")]
    public required ApiKeyData Data { get; set; }
}

/// <summary>
/// Response from deleting an API key
/// </summary>
public class DeleteApiKeyResponse
{
    /// <summary>
    /// Confirmation that the API key was deleted
    /// </summary>
    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }
}

/// <summary>
/// Response from getting current API key metadata
/// </summary>
public class GetCurrentApiKeyResponse
{
    /// <summary>
    /// The current API key data
    /// </summary>
    [JsonPropertyName("data")]
    public required ApiKeyData Data { get; set; }
}
