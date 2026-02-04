using Xunit;
using FluentAssertions;
using OpenRouter.SDK.Services;
using System.Text.Json;

namespace OpenRouter.SDK.Tests;

public class CreditsServiceTests
{
    [Fact]
    public void CreditsService_ShouldBeAccessible()
    {
        // Arrange
        var client = new OpenRouterClient("test-api-key");

        // Act
        var creditsService = client.Credits;

        // Assert
        creditsService.Should().NotBeNull();
        creditsService.Should().BeAssignableTo<ICreditsService>();
    }

    [Fact]
    public void GetCreditsResponse_ShouldHaveAllProperties()
    {
        // Arrange & Act
        var response = new GetCreditsResponse
        {
            TotalCredits = 100.50m,
            TotalUsage = 25.75m,
            Balance = 74.75m,
            Currency = "USD"
        };

        // Assert
        response.TotalCredits.Should().Be(100.50m);
        response.TotalUsage.Should().Be(25.75m);
        response.Balance.Should().Be(74.75m);
        response.Currency.Should().Be("USD");
    }

    [Fact]
    public void GetCreditsResponse_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "total_credits": 150.00,
            "total_usage": 50.25,
            "balance": 99.75,
            "currency": "USD"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<GetCreditsResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.TotalCredits.Should().Be(150.00m);
        result.TotalUsage.Should().Be(50.25m);
        result.Balance.Should().Be(99.75m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void GetCreditsResponse_WithZeroCredits_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "total_credits": 0.00,
            "total_usage": 0.00,
            "balance": 0.00,
            "currency": "USD"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<GetCreditsResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.TotalCredits.Should().Be(0.00m);
        result.TotalUsage.Should().Be(0.00m);
        result.Balance.Should().Be(0.00m);
    }

    [Fact]
    public void GetCreditsResponse_WithNegativeBalance_ShouldDeserializeCorrectly()
    {
        // Arrange - Some accounts might have negative balance if over-limit
        var json = """
        {
            "total_credits": 100.00,
            "total_usage": 150.00,
            "balance": -50.00,
            "currency": "USD"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<GetCreditsResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.TotalCredits.Should().Be(100.00m);
        result.TotalUsage.Should().Be(150.00m);
        result.Balance.Should().Be(-50.00m);
    }

    [Fact]
    public void GetCreditsResponse_WithLargeNumbers_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "total_credits": 9999999.99,
            "total_usage": 1234567.89,
            "balance": 8765432.10,
            "currency": "USD"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<GetCreditsResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.TotalCredits.Should().Be(9999999.99m);
        result.TotalUsage.Should().Be(1234567.89m);
        result.Balance.Should().Be(8765432.10m);
    }

    [Fact]
    public void GetCreditsResponse_WithNullCurrency_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "total_credits": 100.00,
            "total_usage": 25.00,
            "balance": 75.00,
            "currency": null
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<GetCreditsResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.TotalCredits.Should().Be(100.00m);
        result.Currency.Should().BeNull();
    }

    [Fact]
    public void GetCreditsResponse_SerializeAndDeserialize_ShouldMaintainData()
    {
        // Arrange
        var originalResponse = new GetCreditsResponse
        {
            TotalCredits = 500.50m,
            TotalUsage = 123.45m,
            Balance = 377.05m,
            Currency = "USD"
        };

        // Act
        var json = JsonSerializer.Serialize(originalResponse);
        var deserializedResponse = JsonSerializer.Deserialize<GetCreditsResponse>(json);

        // Assert
        deserializedResponse.Should().NotBeNull();
        deserializedResponse!.TotalCredits.Should().Be(originalResponse.TotalCredits);
        deserializedResponse.TotalUsage.Should().Be(originalResponse.TotalUsage);
        deserializedResponse.Balance.Should().Be(originalResponse.Balance);
        deserializedResponse.Currency.Should().Be(originalResponse.Currency);
    }

    [Fact]
    public void CreateCoinbaseChargeRequest_ShouldHaveRequiredAmount()
    {
        // Arrange & Act
        var request = new CreateCoinbaseChargeRequest
        {
            Amount = 50.00m,
            Sender = "user@example.com"
        };

        // Assert
        request.Amount.Should().Be(50.00m);
        request.Sender.Should().Be("user@example.com");
    }

    [Fact]
    public void CreateCoinbaseChargeRequest_WithMinimalData_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new CreateCoinbaseChargeRequest
        {
            Amount = 25.00m
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var result = JsonSerializer.Deserialize<CreateCoinbaseChargeRequest>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Amount.Should().Be(25.00m);
        result.Sender.Should().BeNull();
    }

    [Fact]
    public void CreateCoinbaseChargeRequest_WithSender_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new CreateCoinbaseChargeRequest
        {
            Amount = 100.00m,
            Sender = "john.doe@example.com"
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var result = JsonSerializer.Deserialize<CreateCoinbaseChargeRequest>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Amount.Should().Be(100.00m);
        result.Sender.Should().Be("john.doe@example.com");
    }

    [Fact]
    public void CreateCoinbaseChargeRequest_ShouldValidatePositiveAmount()
    {
        // Arrange
        var client = new OpenRouterClient("test-api-key");
        var request = new CreateCoinbaseChargeRequest
        {
            Amount = 0m // Invalid: zero amount
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() =>
            client.Credits.CreateCoinbaseChargeAsync(request));
    }

    [Fact]
    public void CreateCoinbaseChargeRequest_WithNegativeAmount_ShouldThrowException()
    {
        // Arrange
        var client = new OpenRouterClient("test-api-key");
        var request = new CreateCoinbaseChargeRequest
        {
            Amount = -10.00m // Invalid: negative amount
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() =>
            client.Credits.CreateCoinbaseChargeAsync(request));
    }

    [Fact]
    public void CoinbaseChargeResponse_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "id": "charge-abc123",
            "hosted_url": "https://commerce.coinbase.com/charges/charge-abc123",
            "code": "ABC123",
            "status": "NEW",
            "expires_at": "2024-12-31T23:59:59Z"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<CoinbaseChargeResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("charge-abc123");
        result.HostedUrl.Should().Be("https://commerce.coinbase.com/charges/charge-abc123");
        result.Code.Should().Be("ABC123");
        result.Status.Should().Be("NEW");
        result.ExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public void CoinbaseChargeResponse_WithNullableFields_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "id": "charge-xyz789",
            "hosted_url": "https://commerce.coinbase.com/charges/charge-xyz789",
            "code": null,
            "status": "PENDING",
            "expires_at": null
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<CoinbaseChargeResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("charge-xyz789");
        result.HostedUrl.Should().Be("https://commerce.coinbase.com/charges/charge-xyz789");
        result.Code.Should().BeNull();
        result.Status.Should().Be("PENDING");
        result.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void CoinbaseChargeResponse_WithAllStatuses_ShouldDeserialize()
    {
        // Arrange
        var statuses = new[] { "NEW", "PENDING", "COMPLETED", "EXPIRED", "UNRESOLVED", "RESOLVED", "CANCELED" };

        foreach (var status in statuses)
        {
            var json = $@"{{
                ""id"": ""charge-test"",
                ""hosted_url"": ""https://example.com"",
                ""status"": ""{status}""
            }}";

            // Act
            var result = JsonSerializer.Deserialize<CoinbaseChargeResponse>(json);

            // Assert
            result.Should().NotBeNull();
            result!.Status.Should().Be(status);
        }
    }

    [Fact]
    public void CoinbaseChargeResponse_SerializeAndDeserialize_ShouldMaintainData()
    {
        // Arrange
        var originalResponse = new CoinbaseChargeResponse
        {
            Id = "charge-test-123",
            HostedUrl = "https://commerce.coinbase.com/charges/test",
            Code = "TEST123",
            Status = "NEW",
            ExpiresAt = DateTime.Parse("2024-12-31T23:59:59Z").ToUniversalTime()
        };

        // Act
        var json = JsonSerializer.Serialize(originalResponse);
        var deserializedResponse = JsonSerializer.Deserialize<CoinbaseChargeResponse>(json);

        // Assert
        deserializedResponse.Should().NotBeNull();
        deserializedResponse!.Id.Should().Be(originalResponse.Id);
        deserializedResponse.HostedUrl.Should().Be(originalResponse.HostedUrl);
        deserializedResponse.Code.Should().Be(originalResponse.Code);
        deserializedResponse.Status.Should().Be(originalResponse.Status);
    }

    [Fact]
    public void CreateCoinbaseChargeAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var client = new OpenRouterClient("test-api-key");

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.Credits.CreateCoinbaseChargeAsync(null!));
    }

    [Fact]
    public void CreateCoinbaseChargeRequest_WithLargeAmount_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new CreateCoinbaseChargeRequest
        {
            Amount = 99999.99m,
            Sender = "enterprise@example.com"
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var result = JsonSerializer.Deserialize<CreateCoinbaseChargeRequest>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Amount.Should().Be(99999.99m);
        result.Sender.Should().Be("enterprise@example.com");
    }

    [Fact]
    public void CreateCoinbaseChargeRequest_WithDecimalPrecision_ShouldMaintainAccuracy()
    {
        // Arrange
        var request = new CreateCoinbaseChargeRequest
        {
            Amount = 12.345678m // High precision
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var result = JsonSerializer.Deserialize<CreateCoinbaseChargeRequest>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Amount.Should().Be(12.345678m);
    }
}
