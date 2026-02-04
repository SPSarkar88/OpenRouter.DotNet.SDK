using Xunit;
using FluentAssertions;
using OpenRouter.SDK.Services;
using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Tests;

public class AnalyticsServiceTests
{
    [Fact]
    public void AnalyticsService_ShouldBeAccessible()
    {
        // Arrange
        var client = new OpenRouterClient("test-api-key");

        // Act
        var analyticsService = client.Analytics;

        // Assert
        analyticsService.Should().NotBeNull();
        analyticsService.Should().BeAssignableTo<IAnalyticsService>();
    }

    [Fact]
    public void GetUserActivityAsync_WithEmptyDate_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-api-key");

        // Act & Assert - empty string should throw
        var exception = Assert.ThrowsAsync<ArgumentException>(() => 
            client.Analytics.GetUserActivityAsync(""));
        
        exception.Should().NotBeNull();
    }

    [Fact]
    public void GetUserActivityAsync_WithInvalidDateFormat_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new OpenRouterClient("test-api-key");

        // Act & Assert - various invalid formats
        Assert.ThrowsAsync<ArgumentException>(() => 
            client.Analytics.GetUserActivityAsync("2024-13-01"));
        
        Assert.ThrowsAsync<ArgumentException>(() => 
            client.Analytics.GetUserActivityAsync("01-01-2024"));
        
        Assert.ThrowsAsync<ArgumentException>(() => 
            client.Analytics.GetUserActivityAsync("2024/01/01"));
        
        Assert.ThrowsAsync<ArgumentException>(() => 
            client.Analytics.GetUserActivityAsync("invalid-date"));
    }

    [Fact]
    public void ActivityItem_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var activityItem = new ActivityItem
        {
            Date = "2024-01-15",
            Model = "openai/gpt-4",
            ModelPermaslug = "openai/gpt-4-2024-01-15",
            EndpointId = "endpoint-123",
            ProviderName = "OpenAI",
            Usage = 1.50,
            ByokUsageInference = 0.25,
            Requests = 100,
            PromptTokens = 5000,
            CompletionTokens = 2000,
            ReasoningTokens = 500
        };

        // Assert
        activityItem.Date.Should().Be("2024-01-15");
        activityItem.Model.Should().Be("openai/gpt-4");
        activityItem.ModelPermaslug.Should().Be("openai/gpt-4-2024-01-15");
        activityItem.EndpointId.Should().Be("endpoint-123");
        activityItem.ProviderName.Should().Be("OpenAI");
        activityItem.Usage.Should().Be(1.50);
        activityItem.ByokUsageInference.Should().Be(0.25);
        activityItem.Requests.Should().Be(100);
        activityItem.PromptTokens.Should().Be(5000);
        activityItem.CompletionTokens.Should().Be(2000);
        activityItem.ReasoningTokens.Should().Be(500);
    }

    [Fact]
    public void GetUserActivityResponse_ShouldHaveDataProperty()
    {
        // Arrange & Act
        var response = new GetUserActivityResponse
        {
            Data = new List<ActivityItem>
            {
                new ActivityItem
                {
                    Date = "2024-01-15",
                    Model = "openai/gpt-4",
                    ModelPermaslug = "openai/gpt-4-2024-01-15",
                    EndpointId = "endpoint-123",
                    ProviderName = "OpenAI",
                    Usage = 1.50,
                    ByokUsageInference = 0.25,
                    Requests = 100,
                    PromptTokens = 5000,
                    CompletionTokens = 2000,
                    ReasoningTokens = 500
                }
            }
        };

        // Assert
        response.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(1);
        response.Data[0].Model.Should().Be("openai/gpt-4");
    }

    [Fact]
    public void ActivityItem_ShouldSerializeWithCorrectJsonPropertyNames()
    {
        // Arrange
        var activityItem = new ActivityItem
        {
            Date = "2024-01-15",
            Model = "openai/gpt-4",
            ModelPermaslug = "openai/gpt-4-2024-01-15",
            EndpointId = "endpoint-123",
            ProviderName = "OpenAI",
            Usage = 1.50,
            ByokUsageInference = 0.25,
            Requests = 100,
            PromptTokens = 5000,
            CompletionTokens = 2000,
            ReasoningTokens = 500
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(activityItem);

        // Assert
        json.Should().Contain("\"date\":");
        json.Should().Contain("\"model\":");
        json.Should().Contain("\"model_permaslug\":");
        json.Should().Contain("\"endpoint_id\":");
        json.Should().Contain("\"provider_name\":");
        json.Should().Contain("\"usage\":");
        json.Should().Contain("\"byok_usage_inference\":");
        json.Should().Contain("\"requests\":");
        json.Should().Contain("\"prompt_tokens\":");
        json.Should().Contain("\"completion_tokens\":");
        json.Should().Contain("\"reasoning_tokens\":");
    }

    [Fact]
    public void ActivityItem_ShouldDeserializeFromJson()
    {
        // Arrange
        var json = @"{
            ""date"": ""2024-01-15"",
            ""model"": ""openai/gpt-4"",
            ""model_permaslug"": ""openai/gpt-4-2024-01-15"",
            ""endpoint_id"": ""endpoint-123"",
            ""provider_name"": ""OpenAI"",
            ""usage"": 1.50,
            ""byok_usage_inference"": 0.25,
            ""requests"": 100,
            ""prompt_tokens"": 5000,
            ""completion_tokens"": 2000,
            ""reasoning_tokens"": 500
        }";

        // Act
        var activityItem = System.Text.Json.JsonSerializer.Deserialize<ActivityItem>(json);

        // Assert
        activityItem.Should().NotBeNull();
        activityItem!.Date.Should().Be("2024-01-15");
        activityItem.Model.Should().Be("openai/gpt-4");
        activityItem.ModelPermaslug.Should().Be("openai/gpt-4-2024-01-15");
        activityItem.EndpointId.Should().Be("endpoint-123");
        activityItem.ProviderName.Should().Be("OpenAI");
        activityItem.Usage.Should().Be(1.50);
        activityItem.ByokUsageInference.Should().Be(0.25);
        activityItem.Requests.Should().Be(100);
        activityItem.PromptTokens.Should().Be(5000);
        activityItem.CompletionTokens.Should().Be(2000);
        activityItem.ReasoningTokens.Should().Be(500);
    }

    [Fact]
    public void GetUserActivityResponse_ShouldDeserializeFromJson()
    {
        // Arrange
        var json = @"{
            ""data"": [
                {
                    ""date"": ""2024-01-15"",
                    ""model"": ""openai/gpt-4"",
                    ""model_permaslug"": ""openai/gpt-4-2024-01-15"",
                    ""endpoint_id"": ""endpoint-123"",
                    ""provider_name"": ""OpenAI"",
                    ""usage"": 1.50,
                    ""byok_usage_inference"": 0.25,
                    ""requests"": 100,
                    ""prompt_tokens"": 5000,
                    ""completion_tokens"": 2000,
                    ""reasoning_tokens"": 500
                }
            ]
        }";

        // Act
        var response = System.Text.Json.JsonSerializer.Deserialize<GetUserActivityResponse>(json);

        // Assert
        response.Should().NotBeNull();
        response!.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(1);
        response.Data[0].Model.Should().Be("openai/gpt-4");
        response.Data[0].Usage.Should().Be(1.50);
    }

    [Fact]
    public void ActivityItem_ShouldCalculateTotalTokens()
    {
        // Arrange
        var activityItem = new ActivityItem
        {
            Date = "2024-01-15",
            Model = "openai/gpt-4",
            ModelPermaslug = "openai/gpt-4-2024-01-15",
            EndpointId = "endpoint-123",
            ProviderName = "OpenAI",
            Usage = 1.50,
            ByokUsageInference = 0.25,
            Requests = 100,
            PromptTokens = 5000,
            CompletionTokens = 2000,
            ReasoningTokens = 500
        };

        // Act
        var totalTokens = activityItem.PromptTokens + activityItem.CompletionTokens + activityItem.ReasoningTokens;

        // Assert
        totalTokens.Should().Be(7500);
    }

    [Fact]
    public void ActivityItem_ShouldCalculateTotalCost()
    {
        // Arrange
        var activityItem = new ActivityItem
        {
            Date = "2024-01-15",
            Model = "openai/gpt-4",
            ModelPermaslug = "openai/gpt-4-2024-01-15",
            EndpointId = "endpoint-123",
            ProviderName = "OpenAI",
            Usage = 1.50,
            ByokUsageInference = 0.25,
            Requests = 100,
            PromptTokens = 5000,
            CompletionTokens = 2000,
            ReasoningTokens = 500
        };

        // Act
        var totalCost = activityItem.Usage + activityItem.ByokUsageInference;

        // Assert
        totalCost.Should().Be(1.75);
    }
}
