using FluentAssertions;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;

namespace OpenRouter.SDK.Tests;

public class OAuthServiceTests
{
    [Fact]
    public void CreateSHA256CodeChallenge_ShouldGenerateValidChallenge()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);

        // Act
        var result = client.OAuth.CreateSHA256CodeChallenge();

        // Assert
        result.Should().NotBeNull();
        result.CodeChallenge.Should().NotBeNullOrEmpty();
        result.CodeVerifier.Should().NotBeNullOrEmpty();
        result.CodeVerifier.Length.Should().BeInRange(43, 128);
        
        // Verify base64url encoding (no +, /, or = characters)
        result.CodeChallenge.Should().NotContain("+");
        result.CodeChallenge.Should().NotContain("/");
        result.CodeChallenge.Should().NotContain("=");
    }

    [Fact]
    public void CreateSHA256CodeChallenge_WithCustomVerifier_ShouldUseProvidedVerifier()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);
        var customVerifier = "my-custom-verifier-that-is-at-least-43-characters-long-123456789";

        // Act
        var result = client.OAuth.CreateSHA256CodeChallenge(customVerifier);

        // Assert
        result.CodeVerifier.Should().Be(customVerifier);
        result.CodeChallenge.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateSHA256CodeChallenge_WithShortVerifier_ShouldThrow()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);
        var shortVerifier = "too-short";

        // Act
        Action act = () => client.OAuth.CreateSHA256CodeChallenge(shortVerifier);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Code verifier must be 43-128 characters*");
    }

    [Fact]
    public void CreateSHA256CodeChallenge_WithInvalidCharacters_ShouldThrow()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);
        var invalidVerifier = "invalid@verifier#with$special%characters&that-is-long-enough!";

        // Act
        Action act = () => client.OAuth.CreateSHA256CodeChallenge(invalidVerifier);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Code verifier must only contain unreserved characters*");
    }

    [Fact]
    public void CreateAuthorizationUrl_ShouldGenerateValidUrl()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);
        var request = new CreateAuthorizationUrlRequest
        {
            CallbackUrl = "https://myapp.com/callback"
        };

        // Act
        var url = client.OAuth.CreateAuthorizationUrl(request);

        // Assert
        url.Should().NotBeNullOrEmpty();
        url.Should().Contain("/auth");
        url.Should().Contain("callback_url=");
        url.Should().Contain("myapp.com");
    }

    [Fact]
    public void CreateAuthorizationUrl_WithPKCE_ShouldIncludeChallenge()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);
        var challenge = client.OAuth.CreateSHA256CodeChallenge();
        var request = new CreateAuthorizationUrlRequest
        {
            CallbackUrl = "https://myapp.com/callback",
            CodeChallenge = challenge.CodeChallenge,
            CodeChallengeMethod = CodeChallengeMethod.S256
        };

        // Act
        var url = client.OAuth.CreateAuthorizationUrl(request);

        // Assert
        url.Should().Contain("code_challenge=");
        url.Should().Contain("code_challenge_method=S256");
    }

    [Fact]
    public void CreateAuthorizationUrl_WithLimit_ShouldIncludeLimit()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);
        var request = new CreateAuthorizationUrlRequest
        {
            CallbackUrl = "https://myapp.com/callback",
            Limit = 10.0
        };

        // Act
        var url = client.OAuth.CreateAuthorizationUrl(request);

        // Assert
        url.Should().Contain("limit=10");
    }

    [Fact]
    public void CreateAuthorizationUrl_WithNullCallbackUrl_ShouldThrow()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);
        var request = new CreateAuthorizationUrlRequest
        {
            CallbackUrl = null!
        };

        // Act
        Action act = () => client.OAuth.CreateAuthorizationUrl(request);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Callback URL cannot be null or empty*");
    }

    [Fact]
    public void CreateAuthorizationUrl_WithInvalidUrl_ShouldThrow()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);
        var request = new CreateAuthorizationUrlRequest
        {
            CallbackUrl = "not-a-valid-url"
        };

        // Act
        Action act = () => client.OAuth.CreateAuthorizationUrl(request);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Callback URL must be a valid absolute URL*");
    }

    [Fact]
    public void ExchangeAuthCodeForAPIKeyAsync_WithNullCode_ShouldThrow()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);
        var request = new ExchangeAuthCodeRequest
        {
            Code = null!
        };

        // Act
        Func<Task> act = async () => await client.OAuth.ExchangeAuthCodeForAPIKeyAsync(request);

        // Assert
        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Authorization code cannot be null or empty*");
    }

    [Fact]
    public void OAuthService_ShouldBeAccessibleFromClient()
    {
        // Arrange
        var apiKey = "test-key";
        var client = new OpenRouterClient(apiKey);

        // Act
        var service = client.OAuth;

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IOAuthService>();
    }

    [Fact]
    public void CodeChallengeResult_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var result = new CodeChallengeResult
        {
            CodeChallenge = "test-challenge",
            CodeVerifier = "test-verifier"
        };

        // Assert
        result.CodeChallenge.Should().Be("test-challenge");
        result.CodeVerifier.Should().Be("test-verifier");
    }

    [Fact]
    public void CreateAuthCodeRequest_ShouldSupportAllProperties()
    {
        // Arrange & Act
        var request = new CreateAuthCodeRequest
        {
            CallbackUrl = "https://example.com/callback",
            CodeChallenge = "challenge123",
            CodeChallengeMethod = CodeChallengeMethod.S256,
            Limit = 100.0,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        // Assert
        request.CallbackUrl.Should().Be("https://example.com/callback");
        request.CodeChallenge.Should().Be("challenge123");
        request.CodeChallengeMethod.Should().Be(CodeChallengeMethod.S256);
        request.Limit.Should().Be(100.0);
        request.ExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public void ExchangeAuthCodeRequest_ShouldSupportAllProperties()
    {
        // Arrange & Act
        var request = new ExchangeAuthCodeRequest
        {
            Code = "auth-code-123",
            CodeVerifier = "verifier-abc",
            CodeChallengeMethod = CodeChallengeMethod.S256
        };

        // Assert
        request.Code.Should().Be("auth-code-123");
        request.CodeVerifier.Should().Be("verifier-abc");
        request.CodeChallengeMethod.Should().Be(CodeChallengeMethod.S256);
    }
}
