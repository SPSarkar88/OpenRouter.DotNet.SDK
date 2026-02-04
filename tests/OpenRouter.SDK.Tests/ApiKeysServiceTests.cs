using FluentAssertions;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;

namespace OpenRouter.SDK.Tests;

/// <summary>
/// Tests for API Keys Service
/// </summary>
public class ApiKeysServiceTests
{
    [Fact]
    public void ApiKeysService_ShouldBeAccessible()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");

        // Act
        var service = client.ApiKeys;

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IApiKeysService>();
    }

    [Fact]
    public async Task CreateAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");

        // Act
        Func<Task> act = async () => await client.ApiKeys.CreateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");
        var request = new CreateApiKeyRequest
        {
            Name = ""
        };

        // Act
        Func<Task> act = async () => await client.ApiKeys.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("request")
            .WithMessage("*Name is required*");
    }

    [Fact]
    public async Task CreateAsync_WithNullName_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");
        var request = new CreateApiKeyRequest
        {
            Name = null!
        };

        // Act
        Func<Task> act = async () => await client.ApiKeys.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("request")
            .WithMessage("*Name is required*");
    }

    [Fact]
    public async Task CreateAsync_WithWhitespaceName_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");
        var request = new CreateApiKeyRequest
        {
            Name = "   "
        };

        // Act
        Func<Task> act = async () => await client.ApiKeys.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("request")
            .WithMessage("*Name is required*");
    }

    [Fact]
    public async Task UpdateAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");

        // Act
        Func<Task> act = async () => await client.ApiKeys.UpdateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyHash_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");
        var request = new UpdateApiKeyRequest
        {
            Hash = ""
        };

        // Act
        Func<Task> act = async () => await client.ApiKeys.UpdateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("request")
            .WithMessage("*Hash is required*");
    }

    [Fact]
    public async Task DeleteAsync_WithNullHash_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");

        // Act
        Func<Task> act = async () => await client.ApiKeys.DeleteAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("hash")
            .WithMessage("*Hash cannot be null or empty*");
    }

    [Fact]
    public async Task DeleteAsync_WithEmptyHash_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");

        // Act
        Func<Task> act = async () => await client.ApiKeys.DeleteAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("hash")
            .WithMessage("*Hash cannot be null or empty*");
    }

    [Fact]
    public async Task DeleteAsync_WithWhitespaceHash_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");

        // Act
        Func<Task> act = async () => await client.ApiKeys.DeleteAsync("   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("hash")
            .WithMessage("*Hash cannot be null or empty*");
    }

    [Fact]
    public async Task GetAsync_WithNullHash_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");

        // Act
        Func<Task> act = async () => await client.ApiKeys.GetAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("hash")
            .WithMessage("*Hash cannot be null or empty*");
    }

    [Fact]
    public async Task GetAsync_WithEmptyHash_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-key");

        // Act
        Func<Task> act = async () => await client.ApiKeys.GetAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("hash")
            .WithMessage("*Hash cannot be null or empty*");
    }

    [Fact]
    public void CreateApiKeyRequest_WithValidData_ShouldSetProperties()
    {
        // Arrange & Act
        var request = new CreateApiKeyRequest
        {
            Name = "Test Key",
            Limit = 10.5,
            LimitReset = LimitReset.Daily,
            IncludeByokInLimit = true,
            ExpiresAt = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc)
        };

        // Assert
        request.Name.Should().Be("Test Key");
        request.Limit.Should().Be(10.5);
        request.LimitReset.Should().Be(LimitReset.Daily);
        request.IncludeByokInLimit.Should().BeTrue();
        request.ExpiresAt.Should().Be(new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void UpdateApiKeyRequest_WithValidData_ShouldSetProperties()
    {
        // Arrange & Act
        var request = new UpdateApiKeyRequest
        {
            Hash = "test-hash-123",
            Name = "Updated Key",
            Disabled = true,
            Limit = 25.0,
            LimitReset = LimitReset.Weekly,
            IncludeByokInLimit = false
        };

        // Assert
        request.Hash.Should().Be("test-hash-123");
        request.Name.Should().Be("Updated Key");
        request.Disabled.Should().BeTrue();
        request.Limit.Should().Be(25.0);
        request.LimitReset.Should().Be(LimitReset.Weekly);
        request.IncludeByokInLimit.Should().BeFalse();
    }

    [Fact]
    public void ApiKeyData_ShouldHaveAllProperties()
    {
        // Arrange & Act
        var data = new ApiKeyData
        {
            Hash = "abc123",
            Name = "Test API Key",
            Label = "Test Label",
            Disabled = false,
            Limit = 100.0,
            LimitRemaining = 75.5,
            LimitResetValue = "daily",
            IncludeByokInLimit = true,
            Usage = 24.5,
            UsageDaily = 5.0,
            UsageWeekly = 20.0,
            UsageMonthly = 24.5,
            ByokUsage = 10.0,
            ByokUsageDaily = 2.0,
            ByokUsageWeekly = 8.0,
            ByokUsageMonthly = 10.0,
            CreatedAt = "2026-01-01T00:00:00Z",
            UpdatedAt = "2026-01-15T12:00:00Z",
            ExpiresAt = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Assert
        data.Hash.Should().Be("abc123");
        data.Name.Should().Be("Test API Key");
        data.Label.Should().Be("Test Label");
        data.Disabled.Should().BeFalse();
        data.Limit.Should().Be(100.0);
        data.LimitRemaining.Should().Be(75.5);
        data.LimitResetValue.Should().Be("daily");
        data.IncludeByokInLimit.Should().BeTrue();
        data.Usage.Should().Be(24.5);
        data.UsageDaily.Should().Be(5.0);
        data.UsageWeekly.Should().Be(20.0);
        data.UsageMonthly.Should().Be(24.5);
        data.ByokUsage.Should().Be(10.0);
        data.ByokUsageDaily.Should().Be(2.0);
        data.ByokUsageWeekly.Should().Be(8.0);
        data.ByokUsageMonthly.Should().Be(10.0);
        data.CreatedAt.Should().Be("2026-01-01T00:00:00Z");
        data.UpdatedAt.Should().Be("2026-01-15T12:00:00Z");
        data.ExpiresAt.Should().Be(new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void LimitReset_ShouldHaveCorrectValues()
    {
        // Assert
        LimitReset.Daily.Should().Be(LimitReset.Daily);
        LimitReset.Weekly.Should().Be(LimitReset.Weekly);
        LimitReset.Monthly.Should().Be(LimitReset.Monthly);
    }

    [Fact]
    public void CreateApiKeyResponse_ShouldHaveDataAndKey()
    {
        // Arrange & Act
        var response = new CreateApiKeyResponse
        {
            Data = new ApiKeyData
            {
                Hash = "abc123",
                Name = "Test",
                Label = "Test Label",
                CreatedAt = "2026-01-01T00:00:00Z"
            },
            Key = "sk-or-v1-abc123def456"
        };

        // Assert
        response.Data.Should().NotBeNull();
        response.Data.Hash.Should().Be("abc123");
        response.Key.Should().Be("sk-or-v1-abc123def456");
    }

    [Fact]
    public void DeleteApiKeyResponse_ShouldHaveDeletedFlag()
    {
        // Arrange & Act
        var response = new DeleteApiKeyResponse
        {
            Deleted = true
        };

        // Assert
        response.Deleted.Should().BeTrue();
    }
}
