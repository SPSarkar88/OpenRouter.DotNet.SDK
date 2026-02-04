using FluentAssertions;
using OpenRouter.SDK.Models;
using System.Text.Json;
using Xunit;

namespace OpenRouter.SDK.Tests;

public class EndpointsTests
{
    [Fact]
    public void PublicEndpoint_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "name": "OpenAI: GPT-4",
            "model_id": "openai/gpt-4",
            "model_name": "GPT-4",
            "context_length": 8192,
            "pricing": {
                "prompt": "0.00003",
                "completion": "0.00006"
            },
            "provider_name": "OpenAI",
            "tag": "featured",
            "quantization": "fp16",
            "max_completion_tokens": 4096,
            "max_prompt_tokens": 4096,
            "supported_parameters": ["temperature", "top_p", "max_tokens"],
            "status": "active",
            "uptime_last_30m": 0.99,
            "supports_implicit_caching": true,
            "latency_last_30m": {
                "p50": 150.5,
                "p95": 300.2,
                "p99": 450.8
            },
            "throughput_last_30m": {
                "p50": 100.0,
                "p95": 80.0,
                "p99": 60.0
            }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<PublicEndpoint>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("OpenAI: GPT-4");
        result.ModelId.Should().Be("openai/gpt-4");
        result.ModelName.Should().Be("GPT-4");
        result.ContextLength.Should().Be(8192);
        result.Pricing.Prompt.Should().Be("0.00003");
        result.Pricing.Completion.Should().Be("0.00006");
        result.ProviderName.Should().Be("OpenAI");
        result.Tag.Should().Be("featured");
        result.MaxCompletionTokens.Should().Be(4096);
        result.MaxPromptTokens.Should().Be(4096);
        result.SupportedParameters.Should().Contain(new[] { "temperature", "top_p", "max_tokens" });
        result.Status.Should().Be("active");
        result.UptimeLast30m.Should().Be(0.99);
        result.SupportsImplicitCaching.Should().BeTrue();
        result.LatencyLast30m.Should().NotBeNull();
        result.LatencyLast30m!.P50.Should().Be(150.5);
        result.LatencyLast30m.P95.Should().Be(300.2);
        result.LatencyLast30m.P99.Should().Be(450.8);
    }

    [Fact]
    public void ModelEndpointsResponse_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "id": "openai/gpt-4",
            "name": "GPT-4",
            "created": 1687882410,
            "description": "GPT-4 is a large language model by OpenAI",
            "architecture": {
                "tokenizer": "GPT",
                "instruct_type": "chat",
                "modality": "text",
                "input_modalities": ["text"],
                "output_modalities": ["text"]
            },
            "endpoints": [
                {
                    "name": "OpenAI: GPT-4",
                    "model_id": "openai/gpt-4",
                    "model_name": "GPT-4",
                    "context_length": 8192,
                    "pricing": {
                        "prompt": "0.00003",
                        "completion": "0.00006"
                    },
                    "provider_name": "OpenAI",
                    "tag": "featured",
                    "quantization": null,
                    "max_completion_tokens": null,
                    "max_prompt_tokens": null,
                    "supported_parameters": [],
                    "uptime_last_30m": null,
                    "supports_implicit_caching": false,
                    "latency_last_30m": null,
                    "throughput_last_30m": null
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<ModelEndpointsResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("openai/gpt-4");
        result.Name.Should().Be("GPT-4");
        result.Created.Should().Be(1687882410);
        result.Description.Should().Be("GPT-4 is a large language model by OpenAI");
        result.Architecture.Should().NotBeNull();
        result.Architecture.Tokenizer.Should().Be("GPT");
        result.Architecture.InstructType.Should().Be("chat");
        result.Architecture.Modality.Should().Be("text");
        result.Architecture.InputModalities.Should().Contain("text");
        result.Architecture.OutputModalities.Should().Contain("text");
        result.Endpoints.Should().HaveCount(1);
        result.Endpoints[0].Name.Should().Be("OpenAI: GPT-4");
    }

    [Fact]
    public void ZdrEndpointsResponse_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "data": [
                {
                    "name": "OpenAI: GPT-4",
                    "model_id": "openai/gpt-4",
                    "model_name": "GPT-4",
                    "context_length": 8192,
                    "pricing": {
                        "prompt": "0.00003",
                        "completion": "0.00006"
                    },
                    "provider_name": "OpenAI",
                    "tag": "featured",
                    "quantization": null,
                    "max_completion_tokens": null,
                    "max_prompt_tokens": null,
                    "uptime_last_30m": null,
                    "supports_implicit_caching": false,
                    "latency_last_30m": null,
                    "throughput_last_30m": null
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<ZdrEndpointsResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(1);
        result.Data[0].Name.Should().Be("OpenAI: GPT-4");
        result.Data[0].ModelId.Should().Be("openai/gpt-4");
    }

    [Fact]
    public void Pricing_WithOptionalFields_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "prompt": "0.00001",
            "completion": "0.00002",
            "request": "0.001",
            "image": "0.01",
            "image_token": "0.00005",
            "image_output": "0.02",
            "audio": "0.001",
            "audio_output": "0.002",
            "input_audio_cache": "0.0001",
            "web_search": "0.005",
            "internal_reasoning": "0.00003",
            "input_cache_read": "0.0001",
            "input_cache_write": "0.0002",
            "discount": 0.15
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<Pricing>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Prompt.Should().Be("0.00001");
        result.Completion.Should().Be("0.00002");
        result.Request.Should().Be("0.001");
        result.Image.Should().Be("0.01");
        result.ImageToken.Should().Be("0.00005");
        result.ImageOutput.Should().Be("0.02");
        result.Audio.Should().Be("0.001");
        result.AudioOutput.Should().Be("0.002");
        result.InputAudioCache.Should().Be("0.0001");
        result.WebSearch.Should().Be("0.005");
        result.InternalReasoning.Should().Be("0.00003");
        result.InputCacheRead.Should().Be("0.0001");
        result.InputCacheWrite.Should().Be("0.0002");
        result.Discount.Should().Be(0.15);
    }
}
