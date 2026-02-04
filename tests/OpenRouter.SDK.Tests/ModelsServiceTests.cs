using Xunit;
using FluentAssertions;
using OpenRouter.SDK.Services;
using OpenRouter.SDK.Models;
using System.Text.Json;

namespace OpenRouter.SDK.Tests;

public class ModelsServiceTests
{
    [Fact]
    public void ModelsService_ShouldBeAccessible()
    {
        // Arrange
        var client = new OpenRouterClient("test-api-key");

        // Act
        var modelsService = client.Models;

        // Assert
        modelsService.Should().NotBeNull();
        modelsService.Should().BeAssignableTo<IModelsService>();
    }

    [Fact]
    public void Model_ShouldHaveAllRequiredProperties()
    {
        // Arrange & Act
        var model = new Model
        {
            Id = "openai/gpt-4-turbo",
            Name = "GPT-4 Turbo",
            Description = "Most capable GPT-4 model with vision capabilities",
            ContextLength = 128000,
            Pricing = new ModelPricing
            {
                Prompt = "0.00001",
                Completion = "0.00003",
                Image = "0.01445"
            },
            Architecture = new ModelArchitecture
            {
                Modality = "text+image->text",
                Tokenizer = "GPT",
                InstructType = "vicuna"
            },
            TopProvider = new ModelProvider
            {
                Name = "OpenAI",
                MaxCompletionTokens = 4096,
                IsModerated = false
            }
        };

        // Assert
        model.Id.Should().Be("openai/gpt-4-turbo");
        model.Name.Should().Be("GPT-4 Turbo");
        model.Description.Should().Be("Most capable GPT-4 model with vision capabilities");
        model.ContextLength.Should().Be(128000);
        model.Pricing.Should().NotBeNull();
        model.Pricing!.Prompt.Should().Be("0.00001");
        model.Pricing.Completion.Should().Be("0.00003");
        model.Pricing.Image.Should().Be("0.01445");
        model.Architecture.Should().NotBeNull();
        model.TopProvider.Should().NotBeNull();
        model.TopProvider!.Name.Should().Be("OpenAI");
        model.TopProvider.MaxCompletionTokens.Should().Be(4096);
    }

    [Fact]
    public void ModelPricing_ShouldSerializeCorrectly()
    {
        // Arrange
        var pricing = new ModelPricing
        {
            Prompt = "0.000015",
            Completion = "0.000075",
            Image = "0.01445"
        };

        // Act
        var json = JsonSerializer.Serialize(pricing);
        var result = JsonSerializer.Deserialize<ModelPricing>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Prompt.Should().Be("0.000015");
        result.Completion.Should().Be("0.000075");
        result.Image.Should().Be("0.01445");
    }

    [Fact]
    public void ModelArchitecture_ShouldSerializeCorrectly()
    {
        // Arrange
        var architecture = new ModelArchitecture
        {
            Modality = "text->text",
            Tokenizer = "GPT",
            InstructType = "alpaca"
        };

        // Act
        var json = JsonSerializer.Serialize(architecture);
        var result = JsonSerializer.Deserialize<ModelArchitecture>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Modality.Should().Be("text->text");
        result.Tokenizer.Should().Be("GPT");
        result.InstructType.Should().Be("alpaca");
    }

    [Fact]
    public void ModelsResponse_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "data": [
                {
                    "id": "openai/gpt-4",
                    "name": "GPT-4",
                    "description": "GPT-4 by OpenAI",
                    "context_length": 8192,
                    "pricing": {
                        "prompt": "0.00003",
                        "completion": "0.00006"
                    },
                    "architecture": {
                        "modality": "text->text",
                        "tokenizer": "GPT",
                        "instruct_type": null
                    },
                    "top_provider": {
                        "name": "OpenAI",
                        "max_completion_tokens": 4096,
                        "is_moderated": false
                    }
                },
                {
                    "id": "anthropic/claude-3-opus",
                    "name": "Claude 3 Opus",
                    "context_length": 200000,
                    "pricing": {
                        "prompt": "0.000015",
                        "completion": "0.000075"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<ModelsResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(2);
        
        result.Data[0].Id.Should().Be("openai/gpt-4");
        result.Data[0].Name.Should().Be("GPT-4");
        result.Data[0].ContextLength.Should().Be(8192);
        result.Data[0].Pricing.Should().NotBeNull();
        result.Data[0].Pricing!.Prompt.Should().Be("0.00003");
        
        result.Data[1].Id.Should().Be("anthropic/claude-3-opus");
        result.Data[1].ContextLength.Should().Be(200000);
    }

    [Fact]
    public void ModelsResponse_WithEmptyData_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "data": []
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<ModelsResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().BeEmpty();
    }

    [Fact]
    public void Model_WithMultimodalCapabilities_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "id": "google/gemini-pro-vision",
            "name": "Gemini Pro Vision",
            "context_length": 32768,
            "architecture": {
                "modality": "text+image->text",
                "tokenizer": "Gemini",
                "instruct_type": null
            },
            "pricing": {
                "prompt": "0.000125",
                "completion": "0.000375",
                "image": "0.0025"
            }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<Model>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("google/gemini-pro-vision");
        result.Architecture.Should().NotBeNull();
        result.Architecture!.Modality.Should().Be("text+image->text");
        result.Pricing.Should().NotBeNull();
        result.Pricing!.Image.Should().Be("0.0025");
    }

    [Fact]
    public void ModelsCountResponse_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "count": 150
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<ModelsCountResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(150);
    }

    [Fact]
    public void ModelsCountResponse_WithZeroCount_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "count": 0
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<ModelsCountResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(0);
    }

    [Fact]
    public void ModelProvider_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var provider = new ModelProvider
        {
            Name = "OpenAI",
            MaxCompletionTokens = 4096,
            IsModerated = true
        };

        // Assert
        provider.Name.Should().Be("OpenAI");
        provider.MaxCompletionTokens.Should().Be(4096);
        provider.IsModerated.Should().BeTrue();
    }

    [Fact]
    public void Model_WithNullableFields_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "id": "test/model",
            "name": null,
            "description": null,
            "context_length": 4096,
            "pricing": null,
            "architecture": null,
            "top_provider": null
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<Model>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("test/model");
        result.Name.Should().BeNull();
        result.Description.Should().BeNull();
        result.ContextLength.Should().Be(4096);
        result.Pricing.Should().BeNull();
        result.Architecture.Should().BeNull();
        result.TopProvider.Should().BeNull();
    }

    [Fact]
    public void PerRequestLimits_ShouldSerializeCorrectly()
    {
        // Arrange
        var limits = new PerRequestLimits
        {
            PromptTokens = 100000,
            CompletionTokens = 50000
        };

        // Act
        var json = JsonSerializer.Serialize(limits);
        var result = JsonSerializer.Deserialize<PerRequestLimits>(json);

        // Assert
        result.Should().NotBeNull();
        result!.PromptTokens.Should().Be(100000);
        result.CompletionTokens.Should().Be(50000);
    }

    [Fact]
    public void Model_WithComplexArchitecture_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "id": "meta-llama/llama-3-70b-instruct",
            "name": "Llama 3 70B Instruct",
            "description": "Meta's Llama 3 70B parameter instruction-tuned model",
            "context_length": 8192,
            "architecture": {
                "modality": "text->text",
                "tokenizer": "Llama2",
                "instruct_type": "llama2"
            },
            "pricing": {
                "prompt": "0.00059",
                "completion": "0.00079"
            },
            "per_request_limits": {
                "prompt_tokens": 100000,
                "completion_tokens": 50000
            }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<Model>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("meta-llama/llama-3-70b-instruct");
        result.Name.Should().Be("Llama 3 70B Instruct");
        result.Architecture.Should().NotBeNull();
        result.Architecture!.Tokenizer.Should().Be("Llama2");
        result.Architecture.InstructType.Should().Be("llama2");
        result.PerRequestLimits.Should().NotBeNull();
        result.PerRequestLimits!.PromptTokens.Should().Be(100000);
        result.PerRequestLimits.CompletionTokens.Should().Be(50000);
    }

    [Fact]
    public void Model_SerializeAndDeserialize_ShouldMaintainData()
    {
        // Arrange
        var originalModel = new Model
        {
            Id = "test/model-123",
            Name = "Test Model",
            Description = "A test model for serialization",
            ContextLength = 16384,
            Pricing = new ModelPricing
            {
                Prompt = "0.0001",
                Completion = "0.0002"
            },
            Architecture = new ModelArchitecture
            {
                Modality = "text->text",
                Tokenizer = "Custom"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(originalModel);
        var deserializedModel = JsonSerializer.Deserialize<Model>(json);

        // Assert
        deserializedModel.Should().NotBeNull();
        deserializedModel!.Id.Should().Be(originalModel.Id);
        deserializedModel.Name.Should().Be(originalModel.Name);
        deserializedModel.Description.Should().Be(originalModel.Description);
        deserializedModel.ContextLength.Should().Be(originalModel.ContextLength);
        deserializedModel.Pricing.Should().NotBeNull();
        deserializedModel.Pricing!.Prompt.Should().Be(originalModel.Pricing!.Prompt);
    }
}
