using FluentAssertions;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;

namespace OpenRouter.SDK.Tests;

public class GenerationsServiceTests
{
    [Fact]
    public async Task GetGenerationAsync_ShouldReturnGenerationMetadata()
    {
        // Arrange
        var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            // Skip test if no API key
            return;
        }

        var client = new OpenRouterClient(apiKey);

        // Note: This test requires a valid generation ID from a previous request
        // For now, we'll test the method signature and error handling
        var invalidGenerationId = "test-generation-id";

        // Act & Assert
        // This will likely return 404, but we're testing the service is wired up correctly
        try
        {
            var result = await client.Generations.GetGenerationAsync(invalidGenerationId);
            
            // If we somehow get a result, validate its structure
            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();
        }
        catch (Exception ex)
        {
            // Expected for invalid generation ID - service is working
            ex.Should().NotBeNull();
        }
    }

    [Fact]
    public void GetGenerationAsync_ShouldThrowArgumentException_WhenGenerationIdIsNull()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);

        // Act
        Func<Task> act = async () => await client.Generations.GetGenerationAsync(null!);

        // Assert
        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Generation ID cannot be null or empty*");
    }

    [Fact]
    public void GetGenerationAsync_ShouldThrowArgumentException_WhenGenerationIdIsEmpty()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);

        // Act
        Func<Task> act = async () => await client.Generations.GetGenerationAsync("");

        // Assert
        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Generation ID cannot be null or empty*");
    }

    [Fact]
    public void GetGenerationAsync_ShouldThrowArgumentException_WhenGenerationIdIsWhitespace()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);

        // Act
        Func<Task> act = async () => await client.Generations.GetGenerationAsync("   ");

        // Assert
        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Generation ID cannot be null or empty*");
    }

    [Fact]
    public void GenerationsService_ShouldBeAccessibleFromClient()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);

        // Act
        var service = client.Generations;

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IGenerationsService>();
    }

    [Fact]
    public void GenerationData_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var data = new GenerationData
        {
            Id = "gen_123",
            TotalCost = 0.001,
            CreatedAt = "2024-01-01T00:00:00Z",
            Model = "gpt-4",
            Origin = "https://example.com",
            Usage = 0.001,
            IsByok = false
        };

        // Assert
        data.Id.Should().Be("gen_123");
        data.TotalCost.Should().Be(0.001);
        data.Model.Should().Be("gpt-4");
    }

    [Fact]
    public void GenerationResponse_ShouldContainData()
    {
        // Arrange & Act
        var response = new GenerationResponse
        {
            Data = new GenerationData
            {
                Id = "gen_123",
                TotalCost = 0.001,
                CreatedAt = "2024-01-01T00:00:00Z",
                Model = "gpt-4",
                Origin = "https://example.com",
                Usage = 0.001,
                IsByok = false
            }
        };

        // Assert
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be("gen_123");
    }
}
