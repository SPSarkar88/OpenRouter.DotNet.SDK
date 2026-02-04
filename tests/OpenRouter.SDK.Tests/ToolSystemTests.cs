using FluentAssertions;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;
using System.Text.Json;
using Xunit;

namespace OpenRouter.SDK.Tests;

public class ToolSystemTests
{
    [Fact]
    public void Tool_ShouldCreateWithRequiredProperties()
    {
        // Arrange
        var schema = JsonSchemaBuilder.CreateObjectSchema(
            new Dictionary<string, object>
            {
                ["location"] = JsonSchemaBuilder.String("The location to get weather for")
            },
            new List<string> { "location" });

        // Act
        var tool = new Tool<WeatherInput, WeatherOutput>(
            name: "get_weather",
            description: "Get the current weather for a location",
            inputSchema: schema,
            executeFunc: async (input, context) =>
            {
                return new WeatherOutput { Temperature = 72, Condition = "Sunny" };
            });

        // Assert
        tool.Name.Should().Be("get_weather");
        tool.Description.Should().Be("Get the current weather for a location");
        tool.Type.Should().Be(ToolType.Function);
        tool.HasExecuteFunction.Should().BeTrue();
        tool.RequiresApproval.Should().BeFalse();
    }

    [Fact]
    public void ManualTool_ShouldCreateWithoutExecuteFunction()
    {
        // Arrange
        var schema = JsonSchemaBuilder.CreateObjectSchema(
            new Dictionary<string, object>
            {
                ["action"] = JsonSchemaBuilder.String("The action to perform")
            },
            new List<string> { "action" });

        // Act
        var tool = new ManualTool(
            name: "external_action",
            description: "Perform an external action",
            inputSchema: schema);

        // Assert
        tool.Name.Should().Be("external_action");
        tool.HasExecuteFunction.Should().BeFalse();
    }

    [Fact]
    public void JsonSchemaBuilder_ShouldCreateValidObjectSchema()
    {
        // Act
        var schema = JsonSchemaBuilder.CreateObjectSchema(
            new Dictionary<string, object>
            {
                ["name"] = JsonSchemaBuilder.String("Person's name"),
                ["age"] = JsonSchemaBuilder.Integer("Person's age", 0, 120),
                ["active"] = JsonSchemaBuilder.Boolean("Is active"),
                ["score"] = JsonSchemaBuilder.Number("Score", 0.0, 100.0)
            },
            new List<string> { "name", "age" });

        // Assert
        schema.ValueKind.Should().Be(JsonValueKind.Object);
        schema.GetProperty("type").GetString().Should().Be("object");
        schema.GetProperty("properties").ValueKind.Should().Be(JsonValueKind.Object);
        schema.GetProperty("required").EnumerateArray().Should().HaveCount(2);
    }

    [Fact]
    public async Task Tool_ShouldExecuteSuccessfully()
    {
        // Arrange
        var schema = JsonSchemaBuilder.CreateObjectSchema(
            new Dictionary<string, object>
            {
                ["x"] = JsonSchemaBuilder.Number("First number"),
                ["y"] = JsonSchemaBuilder.Number("Second number")
            },
            new List<string> { "x", "y" });

        var tool = new Tool<CalculatorInput, CalculatorOutput>(
            name: "add",
            description: "Add two numbers",
            inputSchema: schema,
            executeFunc: async (input, context) =>
            {
                return new CalculatorOutput { Result = input.X + input.Y };
            });

        var input = new CalculatorInput { X = 5, Y = 3 };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        result.Result.Should().Be(8);
    }

    [Fact]
    public async Task ToolExecutor_ShouldExecuteToolWithJsonArguments()
    {
        // Arrange
        var schema = JsonSchemaBuilder.CreateObjectSchema(
            new Dictionary<string, object>
            {
                ["location"] = JsonSchemaBuilder.String("The location")
            },
            new List<string> { "location" });

        var tool = new Tool<WeatherInput, WeatherOutput>(
            name: "get_weather",
            description: "Get weather",
            inputSchema: schema,
            executeFunc: async (input, context) =>
            {
                return new WeatherOutput 
                { 
                    Temperature = 75, 
                    Condition = $"Sunny in {input.Location}" 
                };
            });

        var toolCall = new FunctionToolCall
        {
            Id = "call_123",
            Name = "get_weather",
            Arguments = "{\"location\":\"San Francisco\"}"
        };

        var context = new TurnContext
        {
            NumberOfTurns = 1
        };

        // Act
        var result = await ToolExecutor.ExecuteToolAsync(tool, toolCall, context);

        // Assert
        if (!result.IsSuccess)
        {
            var errorMessage = result.Error?.Message ?? "Unknown error";
            Console.WriteLine($"Tool execution failed: {errorMessage}");
            if (result.Error?.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {result.Error.InnerException.Message}");
            }
        }
        
        result.IsSuccess.Should().BeTrue(result.Error?.ToString() ?? "");
        result.ToolCallId.Should().Be("call_123");
        result.ToolName.Should().Be("get_weather");
        result.Result.Should().NotBeNull();
        
        var output = result.Result as WeatherOutput;
        output.Should().NotBeNull();
        output!.Temperature.Should().Be(75);
        output.Condition.Should().Contain("San Francisco");
    }

    [Fact]
    public void ToolExecutor_ShouldConvertToolsToApiFormat()
    {
        // Arrange
        var schema = JsonSchemaBuilder.CreateObjectSchema(
            new Dictionary<string, object>
            {
                ["location"] = JsonSchemaBuilder.String("The location")
            },
            new List<string> { "location" });

        var tool = new Tool<WeatherInput, WeatherOutput>(
            name: "get_weather",
            description: "Get the current weather",
            inputSchema: schema,
            executeFunc: async (input, context) => new WeatherOutput());

        var tools = new List<ITool> { tool };

        // Act
        var apiTools = ToolExecutor.ConvertToolsToApiFormat(tools);

        // Assert
        apiTools.Should().HaveCount(1);
        var firstTool = apiTools[0];
        firstTool.Should().NotBeNull();
        
        // Serialize and check properties
        var json = JsonSerializer.Serialize(firstTool);
        json.Should().Contain("\"type\":\"function\"");
        json.Should().Contain("\"name\":\"get_weather\"");
        json.Should().Contain("\"description\":\"Get the current weather\"");
    }

    [Fact]
    public void TurnContext_ShouldContainConversationState()
    {
        // Arrange & Act
        var context = new TurnContext
        {
            NumberOfTurns = 3,
            ToolCall = new FunctionToolCall
            {
                Id = "call_456",
                Name = "test_tool",
                Arguments = "{}"
            },
            TurnRequest = new BetaResponsesRequest
            {
                Model = "openai/gpt-4"
            }
        };

        // Assert
        context.NumberOfTurns.Should().Be(3);
        context.ToolCall.Should().NotBeNull();
        context.ToolCall!.Id.Should().Be("call_456");
        context.TurnRequest.Should().NotBeNull();
        context.TurnRequest!.Model.Should().Be("openai/gpt-4");
    }

    [Fact]
    public async Task ToolExecutor_ShouldHandleExecutionError()
    {
        // Arrange
        var schema = JsonSchemaBuilder.CreateObjectSchema(
            new Dictionary<string, object>
            {
                ["value"] = JsonSchemaBuilder.Number("A value")
            },
            new List<string> { "value" });

        var tool = new Tool<ErrorInput, ErrorOutput>(
            name: "error_tool",
            description: "A tool that throws an error",
            inputSchema: schema,
            executeFunc: async (input, context) =>
            {
                throw new InvalidOperationException("Test error");
            });

        var toolCall = new FunctionToolCall
        {
            Id = "call_error",
            Name = "error_tool",
            Arguments = "{\"value\":42}"
        };

        var context = new TurnContext { NumberOfTurns = 1 };

        // Act
        var result = await ToolExecutor.ExecuteToolAsync(tool, toolCall, context);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("Test error");
    }
}

// Test data classes
public class WeatherInput
{
    public required string Location { get; set; }
}

public class WeatherOutput
{
    public int Temperature { get; set; }
    public string Condition { get; set; } = string.Empty;
}

public class CalculatorInput
{
    public double X { get; set; }
    public double Y { get; set; }
}

public class CalculatorOutput
{
    public double Result { get; set; }
}

public class ErrorInput
{
    public int Value { get; set; }
}

public class ErrorOutput
{
    public string Message { get; set; } = string.Empty;
}
