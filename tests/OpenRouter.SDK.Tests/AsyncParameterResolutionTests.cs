using FluentAssertions;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;
using System.Text.Json;

namespace OpenRouter.SDK.Tests;

/// <summary>
/// Tests for async parameter resolution feature
/// Verifies that parameters can be dynamic functions based on conversation context
/// </summary>
public class AsyncParameterResolutionTests
{
    [Fact]
    public async Task DynamicParameter_WithStaticValue_ShouldResolveToValue()
    {
        // Arrange
        var parameter = new DynamicParameter<string>("gpt-3.5-turbo");
        var context = new TurnContext { NumberOfTurns = 0 };

        // Act
        var result = await parameter.ResolveAsync(context);

        // Assert
        result.Should().Be("gpt-3.5-turbo");
    }

    [Fact]
    public async Task DynamicParameter_WithSyncResolver_ShouldResolveUsingContext()
    {
        // Arrange
        var parameter = new DynamicParameter<string>(ctx => 
            ctx.NumberOfTurns > 3 ? "gpt-4" : "gpt-3.5-turbo");
        
        var context1 = new TurnContext { NumberOfTurns = 1 };
        var context2 = new TurnContext { NumberOfTurns = 5 };

        // Act
        var result1 = await parameter.ResolveAsync(context1);
        var result2 = await parameter.ResolveAsync(context2);

        // Assert
        result1.Should().Be("gpt-3.5-turbo");
        result2.Should().Be("gpt-4");
    }

    [Fact]
    public async Task DynamicParameter_WithAsyncResolver_ShouldResolveAsynchronously()
    {
        // Arrange
        var parameter = new DynamicParameter<double>(async ctx =>
        {
            await Task.Delay(10); // Simulate async operation
            return ctx.NumberOfTurns * 0.1;
        });
        
        var context = new TurnContext { NumberOfTurns = 5 };

        // Act
        var result = await parameter.ResolveAsync(context);

        // Assert
        result.Should().Be(0.5);
    }

    [Fact]
    public void DynamicParameter_ImplicitConversionFromValue_ShouldWork()
    {
        // Arrange & Act
        DynamicParameter<string> parameter = "test-model";

        // Assert
        parameter.Should().NotBeNull();
    }

    [Fact]
    public void DynamicParameter_ImplicitConversionFromSyncFunc_ShouldWork()
    {
        // Arrange
        Func<TurnContext, string> resolver = ctx => "test-model";

        // Act
        DynamicParameter<string> parameter = resolver;

        // Assert
        parameter.Should().NotBeNull();
    }

    [Fact]
    public void DynamicParameter_ImplicitConversionFromAsyncFunc_ShouldWork()
    {
        // Arrange
        Func<TurnContext, Task<string>> resolver = ctx => Task.FromResult("test-model");

        // Act
        DynamicParameter<string> parameter = resolver;

        // Assert
        parameter.Should().NotBeNull();
    }

    [Fact]
    public async Task DynamicBetaResponsesRequest_WithStaticParameters_ShouldResolve()
    {
        // Arrange
        var dynamicRequest = new DynamicBetaResponsesRequest
        {
            Model = "gpt-3.5-turbo",
            Temperature = 0.7,
            MaxOutputTokens = 1000,
            Input = new List<ResponsesInputItem>
            {
                new() { Type = "text", Text = "Hello" }
            }
        };
        
        var context = new TurnContext { NumberOfTurns = 0 };

        // Act
        var resolved = await dynamicRequest.ResolveAsync(context);

        // Assert
        resolved.Model.Should().Be("gpt-3.5-turbo");
        resolved.Temperature.Should().Be(0.7);
        resolved.MaxOutputTokens.Should().Be(1000);
    }

    [Fact]
    public async Task DynamicBetaResponsesRequest_WithDynamicModel_ShouldResolveBasedOnTurns()
    {
        // Arrange
        Func<TurnContext, string> modelResolver = ctx => 
            ctx.NumberOfTurns > 3 ? "gpt-4" : "gpt-3.5-turbo";
        
        var dynamicRequest = new DynamicBetaResponsesRequest
        {
            Model = modelResolver,
            Input = new List<ResponsesInputItem>
            {
                new() { Type = "text", Text = "Test" }
            }
        };
        
        var context1 = new TurnContext { NumberOfTurns = 1 };
        var context2 = new TurnContext { NumberOfTurns = 5 };

        // Act
        var resolved1 = await dynamicRequest.ResolveAsync(context1);
        var resolved2 = await dynamicRequest.ResolveAsync(context2);

        // Assert
        resolved1.Model.Should().Be("gpt-3.5-turbo");
        resolved2.Model.Should().Be("gpt-4");
    }

    [Fact]
    public async Task DynamicBetaResponsesRequest_WithDynamicTemperature_ShouldResolveAsynchronously()
    {
        // Arrange
        Func<TurnContext, Task<double?>> tempResolver = async ctx =>
        {
            await Task.Delay(10); // Simulate fetching user preferences
            return ctx.HasError ? 0.0 : 0.8;
        };
        
        var dynamicRequest = new DynamicBetaResponsesRequest
        {
            Model = "gpt-3.5-turbo",
            Temperature = tempResolver,
            Input = new List<ResponsesInputItem>
            {
                new() { Type = "text", Text = "Test" }
            }
        };
        
        var context1 = new TurnContext { NumberOfTurns = 0, HasError = false };
        var context2 = new TurnContext { NumberOfTurns = 0, HasError = true };

        // Act
        var resolved1 = await dynamicRequest.ResolveAsync(context1);
        var resolved2 = await dynamicRequest.ResolveAsync(context2);

        // Assert
        resolved1.Temperature.Should().Be(0.8);
        resolved2.Temperature.Should().Be(0.0);
    }

    [Fact]
    public async Task DynamicBetaResponsesRequest_WithDynamicMaxTokens_ShouldResolveBasedOnTokenUsage()
    {
        // Arrange
        Func<TurnContext, int?> tokensResolver = ctx =>
        {
            var tokensUsed = ctx.TotalTokensUsed ?? 0;
            return tokensUsed > 5000 ? 500 : 2000; // Reduce tokens if budget is running low
        };
        
        var dynamicRequest = new DynamicBetaResponsesRequest
        {
            Model = "gpt-3.5-turbo",
            MaxOutputTokens = tokensResolver,
            Input = new List<ResponsesInputItem>
            {
                new() { Type = "text", Text = "Test" }
            }
        };
        
        var context1 = new TurnContext { NumberOfTurns = 1, TotalTokensUsed = 1000 };
        var context2 = new TurnContext { NumberOfTurns = 5, TotalTokensUsed = 8000 };

        // Act
        var resolved1 = await dynamicRequest.ResolveAsync(context1);
        var resolved2 = await dynamicRequest.ResolveAsync(context2);

        // Assert
        resolved1.MaxOutputTokens.Should().Be(2000);
        resolved2.MaxOutputTokens.Should().Be(500);
    }

    [Fact]
    public async Task DynamicBetaResponsesRequest_WithMultipleDynamicParameters_ShouldResolveAll()
    {
        // Arrange
        Func<TurnContext, string> modelResolver = ctx => 
            ctx.NumberOfTurns > 2 ? "gpt-4" : "gpt-3.5-turbo";
        Func<TurnContext, double?> tempResolver = ctx => 
            ctx.NumberOfTurns * 0.1;
        Func<TurnContext, Task<int?>> tokensResolver = async ctx =>
        {
            await Task.Delay(5);
            return 1000 + (ctx.NumberOfTurns * 100);
        };
        
        var dynamicRequest = new DynamicBetaResponsesRequest
        {
            Model = modelResolver,
            Temperature = tempResolver,
            MaxOutputTokens = tokensResolver,
            Input = new List<ResponsesInputItem>
            {
                new() { Type = "text", Text = "Test" }
            }
        };
        
        var context = new TurnContext { NumberOfTurns = 3 };

        // Act
        var resolved = await dynamicRequest.ResolveAsync(context);

        // Assert
        resolved.Model.Should().Be("gpt-4");
        resolved.Temperature.Should().BeApproximately(0.3, 0.0001);
        resolved.MaxOutputTokens.Should().Be(1300);
    }

    [Fact]
    public async Task DynamicBetaResponsesRequest_WithPreviousResponses_ShouldAccessConversationHistory()
    {
        // Arrange
        Func<TurnContext, string> modelResolver = ctx =>
        {
            // Switch to better model if conversation is getting complex
            var responseCount = ctx.PreviousResponses?.Count ?? 0;
            return responseCount > 2 ? "gpt-4" : "gpt-3.5-turbo";
        };
        
        var dynamicRequest = new DynamicBetaResponsesRequest
        {
            Model = modelResolver,
            Input = new List<ResponsesInputItem>
            {
                new() { Type = "text", Text = "Test" }
            }
        };
        
        var context1 = new TurnContext 
        { 
            NumberOfTurns = 1, 
            PreviousResponses = new List<BetaResponsesResponse>() 
        };
        var context2 = new TurnContext 
        { 
            NumberOfTurns = 4, 
            PreviousResponses = new List<BetaResponsesResponse>
            {
                new() { Id = "1", Object = "response", CreatedAt = 0, Model = "test", Status = "completed", Output = new List<ResponsesOutputItem>() },
                new() { Id = "2", Object = "response", CreatedAt = 0, Model = "test", Status = "completed", Output = new List<ResponsesOutputItem>() },
                new() { Id = "3", Object = "response", CreatedAt = 0, Model = "test", Status = "completed", Output = new List<ResponsesOutputItem>() }
            }
        };

        // Act
        var resolved1 = await dynamicRequest.ResolveAsync(context1);
        var resolved2 = await dynamicRequest.ResolveAsync(context2);

        // Assert
        resolved1.Model.Should().Be("gpt-3.5-turbo");
        resolved2.Model.Should().Be("gpt-4");
    }

    [Fact]
    public async Task DynamicBetaResponsesRequest_WithNullParameters_ShouldResolveToNull()
    {
        // Arrange
        var dynamicRequest = new DynamicBetaResponsesRequest
        {
            Model = "gpt-3.5-turbo",
            Input = new List<ResponsesInputItem>
            {
                new() { Type = "text", Text = "Test" }
            }
            // Temperature, MaxOutputTokens, TopP are null
        };
        
        var context = new TurnContext { NumberOfTurns = 0 };

        // Act
        var resolved = await dynamicRequest.ResolveAsync(context);

        // Assert
        resolved.Temperature.Should().BeNull();
        resolved.MaxOutputTokens.Should().BeNull();
        resolved.TopP.Should().BeNull();
    }

    [Fact]
    public async Task TurnContext_EnhancedProperties_ShouldBeAccessible()
    {
        // Arrange
        var context = new TurnContext
        {
            NumberOfTurns = 5,
            PreviousResponses = new List<BetaResponsesResponse>
            {
                new() { Id = "1", Object = "response", CreatedAt = 0, Model = "test", Status = "completed", Output = new List<ResponsesOutputItem>(), Usage = new() { TotalTokens = 100 } },
                new() { Id = "2", Object = "response", CreatedAt = 0, Model = "test", Status = "completed", Output = new List<ResponsesOutputItem>(), Usage = new() { TotalTokens = 200 } }
            },
            TotalTokensUsed = 300,
            HasError = false,
            Metadata = new Dictionary<string, object>
            {
                ["userId"] = "user123",
                ["sessionId"] = "session456"
            }
        };

        // Act & Assert
        context.NumberOfTurns.Should().Be(5);
        context.PreviousResponses.Should().HaveCount(2);
        context.TotalTokensUsed.Should().Be(300);
        context.HasError.Should().BeFalse();
        context.Metadata.Should().ContainKey("userId");
        context.Metadata!["userId"].Should().Be("user123");
    }

    [Fact]
    public async Task DynamicParameter_WithMetadata_ShouldAccessCustomData()
    {
        // Arrange
        Func<TurnContext, double?> resolver = ctx =>
        {
            if (ctx.Metadata?.ContainsKey("creativity") == true)
            {
                return Convert.ToDouble(ctx.Metadata["creativity"]);
            }
            return 0.7;
        };
        
        var parameter = new DynamicParameter<double?>(resolver);
        var context = new TurnContext 
        { 
            NumberOfTurns = 0,
            Metadata = new Dictionary<string, object> { ["creativity"] = 0.9 }
        };

        // Act
        var result = await parameter.ResolveAsync(context);

        // Assert
        result.Should().Be(0.9);
    }

    [Fact]
    public async Task DynamicBetaResponsesRequest_ComplexScenario_ShouldHandleAllFeatures()
    {
        // Arrange - Simulate a complex multi-turn conversation
        Func<TurnContext, string> modelResolver = ctx =>
            ctx.NumberOfTurns > 3 ? "openai/gpt-4" : "openai/gpt-3.5-turbo";
        Func<TurnContext, double?> tempResolver = ctx =>
            ctx.HasError ? 0.0 : 0.7;
        Func<TurnContext, Task<int?>> tokensResolver = async ctx =>
        {
            await Task.Delay(1); // Simulate async budget check
            var tokensUsed = ctx.TotalTokensUsed ?? 0;
            return tokensUsed > 10000 ? 500 : 2000;
        };
        
        var dynamicRequest = new DynamicBetaResponsesRequest
        {
            // Start with cheap model, upgrade if conversation continues
            Model = modelResolver,
            
            // Adaptive temperature based on error state
            Temperature = tempResolver,
            
            // Reduce max tokens if budget is running low
            MaxOutputTokens = tokensResolver,
            
            Input = new List<ResponsesInputItem>
            {
                new() { Type = "text", Text = "Test" }
            }
        };
        
        // Simulate different conversation states
        var earlyTurn = new TurnContext 
        { 
            NumberOfTurns = 1, 
            TotalTokensUsed = 500,
            HasError = false 
        };
        var lateTurn = new TurnContext 
        { 
            NumberOfTurns = 5, 
            TotalTokensUsed = 12000,
            HasError = false 
        };
        var errorTurn = new TurnContext 
        { 
            NumberOfTurns = 2, 
            TotalTokensUsed = 1000,
            HasError = true 
        };

        // Act
        var resolved1 = await dynamicRequest.ResolveAsync(earlyTurn);
        var resolved2 = await dynamicRequest.ResolveAsync(lateTurn);
        var resolved3 = await dynamicRequest.ResolveAsync(errorTurn);

        // Assert
        // Early turn: cheap model, normal temperature, high tokens
        resolved1.Model.Should().Be("openai/gpt-3.5-turbo");
        resolved1.Temperature.Should().Be(0.7);
        resolved1.MaxOutputTokens.Should().Be(2000);
        
        // Late turn with high usage: expensive model, normal temp, reduced tokens
        resolved2.Model.Should().Be("openai/gpt-4");
        resolved2.Temperature.Should().Be(0.7);
        resolved2.MaxOutputTokens.Should().Be(500);
        
        // Error turn: cheap model, zero temp for consistency, high tokens
        resolved3.Model.Should().Be("openai/gpt-3.5-turbo");
        resolved3.Temperature.Should().Be(0.0);
        resolved3.MaxOutputTokens.Should().Be(2000);
    }
}
