using FluentAssertions;
using OpenRouter.SDK.Models;
using System.Text.Json;
using Xunit;

namespace OpenRouter.SDK.Tests;

public class BetaResponsesTests
{
    [Fact]
    public void BetaResponsesRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new BetaResponsesRequest
        {
            Input = "What is the capital of France?",
            Instructions = "You are a helpful assistant",
            Model = "openai/gpt-4",
            MaxOutputTokens = 100,
            Temperature = 0.7,
            Stream = false
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var result = JsonSerializer.Deserialize<BetaResponsesRequest>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Input.Should().NotBeNull();
        result.Input.ToString().Should().Be("What is the capital of France?");
        result.Instructions.Should().Be("You are a helpful assistant");
        result.Model.Should().Be("openai/gpt-4");
        result.MaxOutputTokens.Should().Be(100);
        result.Temperature.Should().Be(0.7);
        result.Stream.Should().BeFalse();
    }

    [Fact]
    public void BetaResponsesRequest_WithTools_ShouldSerializeCorrectly()
    {
        // Arrange
        var tool = new ResponsesFunctionTool
        {
            Name = "get_weather",
            Description = "Get the weather for a location",
            Parameters = new Dictionary<string, object?>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["location"] = new { type = "string", description = "City name" },
                    ["units"] = new { type = "string", @enum = new[] { "celsius", "fahrenheit" } }
                },
                ["required"] = new[] { "location" }
            }
        };

        var request = new BetaResponsesRequest
        {
            Input = "What's the weather in Paris?",
            Model = "openai/gpt-4",
            Tools = new List<ResponsesFunctionTool> { tool }
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var result = JsonSerializer.Deserialize<BetaResponsesRequest>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Tools.Should().HaveCount(1);
        result.Tools![0].Name.Should().Be("get_weather");
        result.Tools[0].Description.Should().Be("Get the weather for a location");
    }

    [Fact]
    public void BetaResponsesResponse_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "id": "gen-123456",
            "object": "response",
            "created_at": 1687882410,
            "model": "openai/gpt-4",
            "status": "completed",
            "completed_at": 1687882415,
            "output": [
                {
                    "type": "text",
                    "text": "The capital of France is Paris."
                }
            ],
            "output_text": "The capital of France is Paris.",
            "error": null,
            "usage": {
                "prompt_tokens": 15,
                "completion_tokens": 8,
                "total_tokens": 23,
                "cost": 0.00069
            },
            "temperature": 0.7,
            "top_p": 1.0,
            "max_output_tokens": 100
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<BetaResponsesResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("gen-123456");
        result.Object.Should().Be("response");
        result.Model.Should().Be("openai/gpt-4");
        result.Status.Should().Be("completed");
        result.Output.Should().HaveCount(1);
        result.Output[0].Type.Should().Be("text");
        result.Output[0].Text.Should().Be("The capital of France is Paris.");
        result.OutputText.Should().Be("The capital of France is Paris.");
        result.Usage.Should().NotBeNull();
        result.Usage!.PromptTokens.Should().Be(15);
        result.Usage.CompletionTokens.Should().Be(8);
        result.Usage.TotalTokens.Should().Be(23);
        result.Usage.Cost.Should().Be(0.00069);
    }

    [Fact]
    public void BetaResponsesStreamChunk_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "type": "response.delta",
            "delta": {
                "type": "text",
                "text": "The "
            },
            "index": 0
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<BetaResponsesStreamChunk>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be("response.delta");
        result.Delta.Should().NotBeNull();
        result.Delta!.Type.Should().Be("text");
        result.Delta.Text.Should().Be("The ");
        result.Index.Should().Be(0);
    }

    [Fact]
    public void BetaResponsesRequest_WithStructuredInput_ShouldSerializeCorrectly()
    {
        // Arrange
        var inputItems = new List<ResponsesInputItem>
        {
            new ResponsesInputItem
            {
                Type = "text",
                Text = "Describe this image:"
            },
            new ResponsesInputItem
            {
                Type = "image_url",
                ImageUrl = new { url = "https://example.com/image.jpg" }
            }
        };

        var request = new BetaResponsesRequest
        {
            Input = inputItems,
            Model = "openai/gpt-4-vision",
            MaxOutputTokens = 300
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        json.Should().Contain("\"type\":\"text\"");
        json.Should().Contain("\"type\":\"image_url\"");
    }

    [Fact]
    public void ResponseProviderPreferences_ShouldSerializeCorrectly()
    {
        // Arrange
        var provider = new ResponseProviderPreferences
        {
            AllowFallbacks = false,
            Zdr = true,
            Order = new List<string> { "openai", "anthropic" },
            DataCollection = "deny"
        };

        var request = new BetaResponsesRequest
        {
            Input = "Hello",
            Model = "gpt-4",
            Provider = provider
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var result = JsonSerializer.Deserialize<BetaResponsesRequest>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Provider.Should().NotBeNull();
        result.Provider!.AllowFallbacks.Should().BeFalse();
        result.Provider.Zdr.Should().BeTrue();
        result.Provider.Order.Should().Contain(new[] { "openai", "anthropic" });
        result.Provider.DataCollection.Should().Be("deny");
    }
}
