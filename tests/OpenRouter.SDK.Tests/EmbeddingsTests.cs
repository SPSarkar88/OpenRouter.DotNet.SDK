using Xunit;
using FluentAssertions;
using OpenRouter.SDK.Models;
using System.Text.Json;

namespace OpenRouter.SDK.Tests;

public class EmbeddingsTests
{
    private readonly JsonSerializerOptions _jsonOptions;

    public EmbeddingsTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    [Fact]
    public void EmbeddingRequest_Should_Serialize_Correctly()
    {
        // Arrange
        var request = new EmbeddingRequest
        {
            Input = "Hello, world!",
            Model = "text-embedding-ada-002",
            EncodingFormat = "float",
            Dimensions = 1536
        };

        // Act
        var json = JsonSerializer.Serialize(request, _jsonOptions);

        // Assert
        json.Should().Contain("\"model\":\"text-embedding-ada-002\"");
        json.Should().Contain("\"encoding_format\":\"float\"");
        json.Should().Contain("\"dimensions\":1536");
    }

    [Fact]
    public void EmbeddingRequest_WithArrayInput_Should_Serialize_Correctly()
    {
        // Arrange
        var request = new EmbeddingRequest
        {
            Input = new[] { "First text", "Second text", "Third text" },
            Model = "text-embedding-ada-002"
        };

        // Act
        var json = JsonSerializer.Serialize(request, _jsonOptions);

        // Assert
        json.Should().Contain("\"model\":\"text-embedding-ada-002\"");
        json.Should().Contain("First text");
        json.Should().Contain("Second text");
    }

    [Fact]
    public void EmbeddingResponse_Should_Deserialize_Correctly()
    {
        // Arrange
        var json = @"{
            ""id"": ""embd-123"",
            ""object"": ""list"",
            ""data"": [
                {
                    ""object"": ""embedding"",
                    ""embedding"": [0.1, 0.2, 0.3, 0.4, 0.5],
                    ""index"": 0
                }
            ],
            ""model"": ""text-embedding-ada-002"",
            ""usage"": {
                ""prompt_tokens"": 5,
                ""total_tokens"": 5,
                ""cost"": 0.00001
            }
        }";

        // Act
        var response = JsonSerializer.Deserialize<EmbeddingResponse>(json, _jsonOptions);

        // Assert
        response.Should().NotBeNull();
        response!.Id.Should().Be("embd-123");
        response.Object.Should().Be("list");
        response.Model.Should().Be("text-embedding-ada-002");
        response.Data.Should().HaveCount(1);
        response.Data[0].Object.Should().Be("embedding");
        response.Data[0].Index.Should().Be(0);
        response.Usage.Should().NotBeNull();
        response.Usage!.PromptTokens.Should().Be(5);
        response.Usage.TotalTokens.Should().Be(5);
        response.Usage.Cost.Should().Be(0.00001);
    }

    [Fact]
    public void EmbeddingResponse_WithMultipleEmbeddings_Should_Deserialize_Correctly()
    {
        // Arrange
        var json = @"{
            ""object"": ""list"",
            ""data"": [
                {
                    ""object"": ""embedding"",
                    ""embedding"": [0.1, 0.2, 0.3],
                    ""index"": 0
                },
                {
                    ""object"": ""embedding"",
                    ""embedding"": [0.4, 0.5, 0.6],
                    ""index"": 1
                }
            ],
            ""model"": ""text-embedding-ada-002"",
            ""usage"": {
                ""prompt_tokens"": 10,
                ""total_tokens"": 10
            }
        }";

        // Act
        var response = JsonSerializer.Deserialize<EmbeddingResponse>(json, _jsonOptions);

        // Assert
        response.Should().NotBeNull();
        response!.Data.Should().HaveCount(2);
        response.Data[0].Index.Should().Be(0);
        response.Data[1].Index.Should().Be(1);
    }
}
