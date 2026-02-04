using FluentAssertions;
using OpenRouter.SDK.Models;
using System.Text.Json;
using Xunit;

namespace OpenRouter.SDK.Tests;

public class ProvidersTests
{
    [Fact]
    public void ProviderData_ShouldSerializeCorrectly()
    {
        // Arrange
        var provider = new ProviderData
        {
            Name = "OpenAI",
            Slug = "openai",
            PrivacyPolicyUrl = "https://openai.com/privacy",
            TermsOfServiceUrl = "https://openai.com/terms",
            StatusPageUrl = "https://status.openai.com"
        };

        // Act
        var json = JsonSerializer.Serialize(provider);
        var result = JsonSerializer.Deserialize<ProviderData>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("OpenAI");
        result.Slug.Should().Be("openai");
        result.PrivacyPolicyUrl.Should().Be("https://openai.com/privacy");
        result.TermsOfServiceUrl.Should().Be("https://openai.com/terms");
        result.StatusPageUrl.Should().Be("https://status.openai.com");
    }

    [Fact]
    public void ProvidersResponse_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "data": [
                {
                    "name": "OpenAI",
                    "slug": "openai",
                    "privacy_policy_url": "https://openai.com/privacy",
                    "terms_of_service_url": "https://openai.com/terms",
                    "status_page_url": "https://status.openai.com"
                },
                {
                    "name": "Anthropic",
                    "slug": "anthropic",
                    "privacy_policy_url": "https://anthropic.com/privacy",
                    "terms_of_service_url": null,
                    "status_page_url": null
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<ProvidersResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(2);
        
        result.Data[0].Name.Should().Be("OpenAI");
        result.Data[0].Slug.Should().Be("openai");
        result.Data[0].PrivacyPolicyUrl.Should().Be("https://openai.com/privacy");
        
        result.Data[1].Name.Should().Be("Anthropic");
        result.Data[1].Slug.Should().Be("anthropic");
        result.Data[1].TermsOfServiceUrl.Should().BeNull();
        result.Data[1].StatusPageUrl.Should().BeNull();
    }

    [Fact]
    public void ProvidersResponse_WithNullUrls_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "data": [
                {
                    "name": "Test Provider",
                    "slug": "test",
                    "privacy_policy_url": null
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<ProvidersResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(1);
        result.Data[0].Name.Should().Be("Test Provider");
        result.Data[0].Slug.Should().Be("test");
        result.Data[0].PrivacyPolicyUrl.Should().BeNull();
        result.Data[0].TermsOfServiceUrl.Should().BeNull();
        result.Data[0].StatusPageUrl.Should().BeNull();
    }
}
