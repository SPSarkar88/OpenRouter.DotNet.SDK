using System.Text.Json.Serialization;

namespace OpenRouter.SDK.Models;

/// <summary>
/// Interval at which the limit resets
/// </summary>
public enum ResetInterval
{
    /// <summary>
    /// Daily reset interval
    /// </summary>
    Daily,
    
    /// <summary>
    /// Weekly reset interval
    /// </summary>
    Weekly,
    
    /// <summary>
    /// Monthly reset interval
    /// </summary>
    Monthly
}

/// <summary>
/// Represents a guardrail for content moderation and usage limits
/// </summary>
public class Guardrail
{
    /// <summary>
    /// Unique identifier for the guardrail
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    /// <summary>
    /// Name of the guardrail
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    /// <summary>
    /// Description of the guardrail
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Spending limit in USD
    /// </summary>
    [JsonPropertyName("limit_usd")]
    public double? LimitUsd { get; set; }
    
    /// <summary>
    /// Interval at which the limit resets (daily, weekly, monthly)
    /// </summary>
    [JsonPropertyName("reset_interval")]
    [JsonConverter(typeof(JsonStringEnumConverter<ResetInterval>))]
    public ResetInterval? ResetInterval { get; set; }
    
    /// <summary>
    /// List of allowed provider IDs
    /// </summary>
    [JsonPropertyName("allowed_providers")]
    public List<string>? AllowedProviders { get; set; }
    
    /// <summary>
    /// Array of model canonical_slugs (immutable identifiers)
    /// </summary>
    [JsonPropertyName("allowed_models")]
    public List<string>? AllowedModels { get; set; }
    
    /// <summary>
    /// Whether to enforce zero data retention
    /// </summary>
    [JsonPropertyName("enforce_zdr")]
    public bool? EnforceZdr { get; set; }
    
    /// <summary>
    /// ISO 8601 timestamp of when the guardrail was created
    /// </summary>
    [JsonPropertyName("created_at")]
    public required string CreatedAt { get; set; }
    
    /// <summary>
    /// ISO 8601 timestamp of when the guardrail was last updated
    /// </summary>
    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; set; }
}

/// <summary>
/// Request to create a new guardrail
/// </summary>
public class CreateGuardrailRequest
{
    /// <summary>
    /// Name for the new guardrail
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    /// <summary>
    /// Description of the guardrail
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Spending limit in USD
    /// </summary>
    [JsonPropertyName("limit_usd")]
    public double? LimitUsd { get; set; }
    
    /// <summary>
    /// Interval at which the limit resets (daily, weekly, monthly)
    /// </summary>
    [JsonPropertyName("reset_interval")]
    [JsonConverter(typeof(JsonStringEnumConverter<ResetInterval>))]
    public ResetInterval? ResetInterval { get; set; }
    
    /// <summary>
    /// List of allowed provider IDs
    /// </summary>
    [JsonPropertyName("allowed_providers")]
    public List<string>? AllowedProviders { get; set; }
    
    /// <summary>
    /// Array of model identifiers (slug or canonical_slug accepted)
    /// </summary>
    [JsonPropertyName("allowed_models")]
    public List<string>? AllowedModels { get; set; }
    
    /// <summary>
    /// Whether to enforce zero data retention
    /// </summary>
    [JsonPropertyName("enforce_zdr")]
    public bool? EnforceZdr { get; set; }
}

/// <summary>
/// Request to update an existing guardrail
/// </summary>
public class UpdateGuardrailRequest
{
    /// <summary>
    /// New name for the guardrail
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    /// <summary>
    /// New description for the guardrail
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    /// <summary>
    /// New spending limit in USD
    /// </summary>
    [JsonPropertyName("limit_usd")]
    public double? LimitUsd { get; set; }
    
    /// <summary>
    /// Interval at which the limit resets (daily, weekly, monthly)
    /// </summary>
    [JsonPropertyName("reset_interval")]
    [JsonConverter(typeof(JsonStringEnumConverter<ResetInterval>))]
    public ResetInterval? ResetInterval { get; set; }
    
    /// <summary>
    /// New list of allowed provider IDs
    /// </summary>
    [JsonPropertyName("allowed_providers")]
    public List<string>? AllowedProviders { get; set; }
    
    /// <summary>
    /// Array of model identifiers (slug or canonical_slug accepted)
    /// </summary>
    [JsonPropertyName("allowed_models")]
    public List<string>? AllowedModels { get; set; }
    
    /// <summary>
    /// Whether to enforce zero data retention
    /// </summary>
    [JsonPropertyName("enforce_zdr")]
    public bool? EnforceZdr { get; set; }
}

/// <summary>
/// Response containing a single guardrail
/// </summary>
public class GuardrailResponse
{
    /// <summary>
    /// The guardrail data
    /// </summary>
    [JsonPropertyName("data")]
    public required Guardrail Data { get; set; }
}

/// <summary>
/// Response containing a list of guardrails
/// </summary>
public class ListGuardrailsResponse
{
    /// <summary>
    /// List of guardrails
    /// </summary>
    [JsonPropertyName("data")]
    public required List<Guardrail> Data { get; set; }
    
    /// <summary>
    /// Total number of guardrails
    /// </summary>
    [JsonPropertyName("total_count")]
    public required int TotalCount { get; set; }
}

/// <summary>
/// Represents an API key assignment to a guardrail
/// </summary>
public class KeyAssignment
{
    /// <summary>
    /// Unique identifier for the assignment
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    /// <summary>
    /// Hash of the assigned API key
    /// </summary>
    [JsonPropertyName("key_hash")]
    public required string KeyHash { get; set; }
    
    /// <summary>
    /// ID of the guardrail
    /// </summary>
    [JsonPropertyName("guardrail_id")]
    public required string GuardrailId { get; set; }
    
    /// <summary>
    /// Name of the API key
    /// </summary>
    [JsonPropertyName("key_name")]
    public required string KeyName { get; set; }
    
    /// <summary>
    /// Label of the API key
    /// </summary>
    [JsonPropertyName("key_label")]
    public required string KeyLabel { get; set; }
    
    /// <summary>
    /// User ID of who made the assignment
    /// </summary>
    [JsonPropertyName("assigned_by")]
    public string? AssignedBy { get; set; }
    
    /// <summary>
    /// ISO 8601 timestamp of when the assignment was created
    /// </summary>
    [JsonPropertyName("created_at")]
    public required string CreatedAt { get; set; }
}

/// <summary>
/// Response containing a list of key assignments
/// </summary>
public class ListKeyAssignmentsResponse
{
    /// <summary>
    /// List of key assignments
    /// </summary>
    [JsonPropertyName("data")]
    public required List<KeyAssignment> Data { get; set; }
    
    /// <summary>
    /// Total number of key assignments
    /// </summary>
    [JsonPropertyName("total_count")]
    public required int TotalCount { get; set; }
}

/// <summary>
/// Represents an organization member assignment to a guardrail
/// </summary>
public class MemberAssignment
{
    /// <summary>
    /// Unique identifier for the assignment
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    /// <summary>
    /// ID of the organization member
    /// </summary>
    [JsonPropertyName("member_id")]
    public required string MemberId { get; set; }
    
    /// <summary>
    /// ID of the guardrail
    /// </summary>
    [JsonPropertyName("guardrail_id")]
    public required string GuardrailId { get; set; }
    
    /// <summary>
    /// Email of the member
    /// </summary>
    [JsonPropertyName("member_email")]
    public required string MemberEmail { get; set; }
    
    /// <summary>
    /// User ID of who made the assignment
    /// </summary>
    [JsonPropertyName("assigned_by")]
    public string? AssignedBy { get; set; }
    
    /// <summary>
    /// ISO 8601 timestamp of when the assignment was created
    /// </summary>
    [JsonPropertyName("created_at")]
    public required string CreatedAt { get; set; }
}

/// <summary>
/// Response containing a list of member assignments
/// </summary>
public class ListMemberAssignmentsResponse
{
    /// <summary>
    /// List of member assignments
    /// </summary>
    [JsonPropertyName("data")]
    public required List<MemberAssignment> Data { get; set; }
    
    /// <summary>
    /// Total number of member assignments
    /// </summary>
    [JsonPropertyName("total_count")]
    public required int TotalCount { get; set; }
}

/// <summary>
/// Request to bulk assign API keys to a guardrail
/// </summary>
public class BulkAssignKeysRequest
{
    /// <summary>
    /// Array of API key hashes to assign to the guardrail
    /// </summary>
    [JsonPropertyName("key_hashes")]
    public required List<string> KeyHashes { get; set; }
}

/// <summary>
/// Request to bulk assign members to a guardrail
/// </summary>
public class BulkAssignMembersRequest
{
    /// <summary>
    /// Array of member IDs to assign to the guardrail
    /// </summary>
    [JsonPropertyName("member_ids")]
    public required List<string> MemberIds { get; set; }
}

/// <summary>
/// Response for successful bulk operations
/// </summary>
public class BulkOperationResponse
{
    /// <summary>
    /// Success status message
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
