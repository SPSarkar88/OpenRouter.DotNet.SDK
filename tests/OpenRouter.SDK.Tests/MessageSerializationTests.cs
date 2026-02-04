using Xunit;
using FluentAssertions;
using OpenRouter.SDK.Models;
using System.Text.Json;

namespace OpenRouter.SDK.Tests;

public class MessageSerializationTests
{
    private readonly JsonSerializerOptions _jsonOptions;

    public MessageSerializationTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }

    [Fact]
    public void SystemMessage_Should_Serialize_Correctly()
    {
        // Arrange
        var message = new SystemMessage { Content = "You are a helpful assistant." };

        // Act
        var json = JsonSerializer.Serialize<Message>(message, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SystemMessage>();
        ((SystemMessage)deserialized!).Content.Should().Be("You are a helpful assistant.");
    }

    [Fact]
    public void UserMessage_Should_Serialize_Correctly()
    {
        // Arrange
        var message = new UserMessage { Content = "Hello, how are you?" };

        // Act
        var json = JsonSerializer.Serialize<Message>(message, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<UserMessage>();
        var userMessage = (UserMessage)deserialized!;
        // Content is deserialized as JsonElement, get its string value
        var contentElement = (System.Text.Json.JsonElement)userMessage.Content;
        contentElement.GetString().Should().Be("Hello, how are you?");
    }

    [Fact]
    public void AssistantMessage_Should_Serialize_Correctly()
    {
        // Arrange
        var message = new AssistantMessage { Content = "I'm doing well, thank you!" };

        // Act
        var json = JsonSerializer.Serialize<Message>(message, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<AssistantMessage>();
        ((AssistantMessage)deserialized!).Content.Should().Be("I'm doing well, thank you!");
    }

    [Fact]
    public void MessageList_Should_Serialize_With_Different_Types()
    {
        // Arrange
        var messages = new List<Message>
        {
            new SystemMessage { Content = "System" },
            new UserMessage { Content = "User" },
            new AssistantMessage { Content = "Assistant" }
        };

        // Act
        var json = JsonSerializer.Serialize(messages, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<List<Message>>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().HaveCount(3);
        deserialized![0].Should().BeOfType<SystemMessage>();
        deserialized[1].Should().BeOfType<UserMessage>();
        deserialized[2].Should().BeOfType<AssistantMessage>();
    }
}
