using OpenRouter.SDK;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;
using System.Text.Json;

namespace OpenRouter.SDK.Tests;

public class GuardrailsServiceTests
{
    [Fact]
    public void GuardrailsService_ShouldBeAccessible()
    {
        // Arrange & Act
        var client = new OpenRouterClient("test-key");

        // Assert
        Assert.NotNull(client.Guardrails);
    }

    [Fact]
    public void CreateGuardrailAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => 
            client.Guardrails.CreateGuardrailAsync(null!, CancellationToken.None));
    }

    [Fact]
    public void CreateGuardrailAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");
        var request = new CreateGuardrailRequest { Name = "" };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => 
            client.Guardrails.CreateGuardrailAsync(request, CancellationToken.None));
    }

    [Fact]
    public void GetGuardrailAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => 
            client.Guardrails.GetGuardrailAsync("", CancellationToken.None));
    }

    [Fact]
    public void UpdateGuardrailAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => 
            client.Guardrails.UpdateGuardrailAsync("test-id", null!, CancellationToken.None));
    }

    [Fact]
    public void DeleteGuardrailAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => 
            client.Guardrails.DeleteGuardrailAsync("", CancellationToken.None));
    }

    [Fact]
    public void BulkAssignKeysAsync_WithEmptyKeyHashes_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");
        var request = new BulkAssignKeysRequest { KeyHashes = new List<string>() };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => 
            client.Guardrails.BulkAssignKeysAsync("test-id", request, CancellationToken.None));
    }

    [Fact]
    public void BulkAssignMembersAsync_WithEmptyMemberIds_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");
        var request = new BulkAssignMembersRequest { MemberIds = new List<string>() };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => 
            client.Guardrails.BulkAssignMembersAsync("test-id", request, CancellationToken.None));
    }

    [Fact]
    public void Guardrail_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var guardrail = new Guardrail
        {
            Id = "gr_123",
            Name = "Test Guardrail",
            Description = "Test description",
            LimitUsd = 100.50,
            ResetInterval = ResetInterval.Daily,
            AllowedProviders = new List<string> { "openai", "anthropic" },
            AllowedModels = new List<string> { "gpt-4", "claude-3" },
            EnforceZdr = true,
            CreatedAt = "2024-01-01T00:00:00Z",
            UpdatedAt = "2024-01-02T00:00:00Z"
        };

        // Assert
        Assert.Equal("gr_123", guardrail.Id);
        Assert.Equal("Test Guardrail", guardrail.Name);
        Assert.Equal("Test description", guardrail.Description);
        Assert.Equal(100.50, guardrail.LimitUsd);
        Assert.Equal(ResetInterval.Daily, guardrail.ResetInterval);
        Assert.Contains("openai", guardrail.AllowedProviders);
        Assert.Contains("gpt-4", guardrail.AllowedModels);
        Assert.True(guardrail.EnforceZdr);
        Assert.Equal("2024-01-01T00:00:00Z", guardrail.CreatedAt);
        Assert.Equal("2024-01-02T00:00:00Z", guardrail.UpdatedAt);
    }

    [Fact]
    public void KeyAssignment_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var assignment = new KeyAssignment
        {
            Id = "ka_123",
            KeyHash = "hash123",
            GuardrailId = "gr_123",
            KeyName = "Test Key",
            KeyLabel = "Production",
            AssignedBy = "user_123",
            CreatedAt = "2024-01-01T00:00:00Z"
        };

        // Assert
        Assert.Equal("ka_123", assignment.Id);
        Assert.Equal("hash123", assignment.KeyHash);
        Assert.Equal("gr_123", assignment.GuardrailId);
        Assert.Equal("Test Key", assignment.KeyName);
        Assert.Equal("Production", assignment.KeyLabel);
        Assert.Equal("user_123", assignment.AssignedBy);
        Assert.Equal("2024-01-01T00:00:00Z", assignment.CreatedAt);
    }

    [Fact]
    public void MemberAssignment_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var assignment = new MemberAssignment
        {
            Id = "ma_123",
            MemberId = "member_123",
            GuardrailId = "gr_123",
            MemberEmail = "test@example.com",
            AssignedBy = "admin_123",
            CreatedAt = "2024-01-01T00:00:00Z"
        };

        // Assert
        Assert.Equal("ma_123", assignment.Id);
        Assert.Equal("member_123", assignment.MemberId);
        Assert.Equal("gr_123", assignment.GuardrailId);
        Assert.Equal("test@example.com", assignment.MemberEmail);
        Assert.Equal("admin_123", assignment.AssignedBy);
        Assert.Equal("2024-01-01T00:00:00Z", assignment.CreatedAt);
    }

    [Fact]
    public void Guardrail_ShouldSerializeWithCorrectJsonPropertyNames()
    {
        // Arrange
        var guardrail = new Guardrail
        {
            Id = "gr_123",
            Name = "Test",
            LimitUsd = 100.50,
            ResetInterval = ResetInterval.Weekly,
            AllowedProviders = new List<string> { "openai" },
            AllowedModels = new List<string> { "gpt-4" },
            EnforceZdr = true,
            CreatedAt = "2024-01-01T00:00:00Z"
        };

        // Act
        var json = JsonSerializer.Serialize(guardrail);

        // Assert
        Assert.Contains("\"id\":", json);
        Assert.Contains("\"name\":", json);
        Assert.Contains("\"limit_usd\":", json);
        Assert.Contains("\"reset_interval\":", json);
        Assert.Contains("\"allowed_providers\":", json);
        Assert.Contains("\"allowed_models\":", json);
        Assert.Contains("\"enforce_zdr\":", json);
        Assert.Contains("\"created_at\":", json);
    }

    [Fact]
    public void CreateGuardrailRequest_ShouldSerializeWithCorrectJsonPropertyNames()
    {
        // Arrange
        var request = new CreateGuardrailRequest
        {
            Name = "Test",
            LimitUsd = 50.0,
            ResetInterval = ResetInterval.Monthly,
            AllowedProviders = new List<string> { "anthropic" },
            AllowedModels = new List<string> { "claude-3" },
            EnforceZdr = false
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        Assert.Contains("\"name\":", json);
        Assert.Contains("\"limit_usd\":", json);
        Assert.Contains("\"reset_interval\":", json);
        Assert.Contains("\"allowed_providers\":", json);
        Assert.Contains("\"allowed_models\":", json);
        Assert.Contains("\"enforce_zdr\":", json);
    }

    [Fact]
    public void GuardrailResponse_ShouldDeserializeFromJson()
    {
        // Arrange
        var json = @"{
            ""data"": {
                ""id"": ""gr_123"",
                ""name"": ""Test Guardrail"",
                ""description"": ""Test description"",
                ""limit_usd"": 100.5,
                ""reset_interval"": ""daily"",
                ""allowed_providers"": [""openai""],
                ""allowed_models"": [""gpt-4""],
                ""enforce_zdr"": true,
                ""created_at"": ""2024-01-01T00:00:00Z"",
                ""updated_at"": ""2024-01-02T00:00:00Z""
            }
        }";

        // Act
        var response = JsonSerializer.Deserialize<GuardrailResponse>(json);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Equal("gr_123", response.Data.Id);
        Assert.Equal("Test Guardrail", response.Data.Name);
        Assert.Equal("Test description", response.Data.Description);
        Assert.Equal(100.5, response.Data.LimitUsd);
        Assert.Equal(ResetInterval.Daily, response.Data.ResetInterval);
        Assert.Single(response.Data.AllowedProviders);
        Assert.Equal("openai", response.Data.AllowedProviders[0]);
        Assert.True(response.Data.EnforceZdr);
    }

    [Fact]
    public void ListGuardrailsResponse_ShouldDeserializeFromJson()
    {
        // Arrange
        var json = @"{
            ""data"": [
                {
                    ""id"": ""gr_123"",
                    ""name"": ""Guardrail 1"",
                    ""created_at"": ""2024-01-01T00:00:00Z""
                },
                {
                    ""id"": ""gr_456"",
                    ""name"": ""Guardrail 2"",
                    ""created_at"": ""2024-01-02T00:00:00Z""
                }
            ],
            ""total_count"": 2
        }";

        // Act
        var response = JsonSerializer.Deserialize<ListGuardrailsResponse>(json);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Data.Count);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal("gr_123", response.Data[0].Id);
        Assert.Equal("Guardrail 1", response.Data[0].Name);
    }

    [Fact]
    public void ListKeyAssignmentsResponse_ShouldDeserializeFromJson()
    {
        // Arrange
        var json = @"{
            ""data"": [
                {
                    ""id"": ""ka_123"",
                    ""key_hash"": ""hash123"",
                    ""guardrail_id"": ""gr_123"",
                    ""key_name"": ""Test Key"",
                    ""key_label"": ""Production"",
                    ""assigned_by"": ""user_123"",
                    ""created_at"": ""2024-01-01T00:00:00Z""
                }
            ],
            ""total_count"": 1
        }";

        // Act
        var response = JsonSerializer.Deserialize<ListKeyAssignmentsResponse>(json);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Data);
        Assert.Equal(1, response.TotalCount);
        Assert.Equal("ka_123", response.Data[0].Id);
        Assert.Equal("hash123", response.Data[0].KeyHash);
    }

    [Fact]
    public void ResetInterval_ShouldHaveCorrectEnumValues()
    {
        // Assert
        Assert.Equal(ResetInterval.Daily, ResetInterval.Daily);
        Assert.Equal(ResetInterval.Weekly, ResetInterval.Weekly);
        Assert.Equal(ResetInterval.Monthly, ResetInterval.Monthly);
        Assert.NotEqual(ResetInterval.Daily, ResetInterval.Weekly);
    }

    [Fact]
    public void BulkAssignKeysRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new BulkAssignKeysRequest
        {
            KeyHashes = new List<string> { "hash1", "hash2", "hash3" }
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        Assert.Contains("\"key_hashes\":", json);
        Assert.Contains("\"hash1\"", json);
        Assert.Contains("\"hash2\"", json);
        Assert.Contains("\"hash3\"", json);
    }

    [Fact]
    public void BulkAssignMembersRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new BulkAssignMembersRequest
        {
            MemberIds = new List<string> { "member1", "member2" }
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        Assert.Contains("\"member_ids\":", json);
        Assert.Contains("\"member1\"", json);
        Assert.Contains("\"member2\"", json);
    }
}
