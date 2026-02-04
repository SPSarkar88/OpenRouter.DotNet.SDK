using Xunit;
using FluentAssertions;
using OpenRouter.SDK.Models;
using System.Text.Json;

namespace OpenRouter.SDK.Tests;

public class ChatCompletionTests
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ChatCompletionTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    [Fact]
    public void ChatCompletionRequest_Should_Serialize_Correctly()
    {
        // Arrange
        var request = new ChatCompletionRequest
        {
            Model = "openai/gpt-3.5-turbo",
            Messages = new List<Message>
            {
                new SystemMessage { Content = "System" },
                new UserMessage { Content = "Hello" }
            },
            Temperature = 0.7,
            MaxTokens = 100
        };

        // Act
        var json = JsonSerializer.Serialize(request, _jsonOptions);

        // Assert
        json.Should().Contain("\"model\":\"openai/gpt-3.5-turbo\"");
        json.Should().Contain("\"temperature\":0.7");
        json.Should().Contain("\"max_tokens\":100");
    }

    [Fact]
    public void ChatCompletionResponse_Should_Deserialize_Correctly()
    {
        // Arrange
        var json = @"{
            ""id"": ""chatcmpl-123"",
            ""object"": ""chat.completion"",
            ""created"": 1677652288,
            ""model"": ""openai/gpt-3.5-turbo"",
            ""choices"": [
                {
                    ""index"": 0,
                    ""message"": {
                        ""role"": ""assistant"",
                        ""content"": ""Hello! How can I help you?""
                    },
                    ""finish_reason"": ""stop""
                }
            ],
            ""usage"": {
                ""prompt_tokens"": 10,
                ""completion_tokens"": 20,
                ""total_tokens"": 30
            }
        }";

        // Act
        var response = JsonSerializer.Deserialize<ChatCompletionResponse>(json, _jsonOptions);

        // Assert
        response.Should().NotBeNull();
        response!.Id.Should().Be("chatcmpl-123");
        response.Model.Should().Be("openai/gpt-3.5-turbo");
        response.Choices.Should().HaveCount(1);
        response.Choices[0].Message.Content.Should().Be("Hello! How can I help you?");
        response.Usage.Should().NotBeNull();
        response.Usage!.TotalTokens.Should().Be(30);
    }

    [Fact]
    public void ChatCompletionChunk_Should_Deserialize_Correctly()
    {
        // Arrange
        var json = @"{
            ""id"": ""chatcmpl-123"",
            ""object"": ""chat.completion.chunk"",
            ""created"": 1677652288,
            ""model"": ""openai/gpt-3.5-turbo"",
            ""choices"": [
                {
                    ""index"": 0,
                    ""delta"": {
                        ""content"": ""Hello""
                    },
                    ""finish_reason"": null
                }
            ]
        }";

        // Act
        var chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(json, _jsonOptions);

        // Assert
        chunk.Should().NotBeNull();
        chunk!.Id.Should().Be("chatcmpl-123");
        chunk.Choices[0].Delta.Content.Should().Be("Hello");
    }
}
