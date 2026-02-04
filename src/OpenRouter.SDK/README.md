# OpenRouter .NET SDK

A modern, type-safe .NET SDK for the [OpenRouter](https://openrouter.ai/) API, providing access to 300+ AI models through a unified interface.

## Features

- ✅ **Type-safe**: Full C# type safety with nullable reference types
- ✅ **Async/Await**: Modern async patterns throughout
- ✅ **Streaming**: Native support for streaming responses with `IAsyncEnumerable<T>`
- ✅ **Concurrent Streaming**: Multiple independent consumers can read the same stream simultaneously
- ✅ **7 Stream Patterns**: Text, Reasoning, Tool Calls, Tool Results, Messages, Responses, and Full Events
- ✅ **Resilience**: Built-in retry policies with Polly
- ✅ **Extensible**: HTTP request/response hooks for logging and customization
- ✅ **Easy to use**: Simple high-level API with advanced options available
- ✅ **.NET 8.0**: Built on the latest .NET with C# 12 features

## Installation

```bash
dotnet add package OpenRouter.SDK
```

Or add to your `.csproj` file:

```xml
<PackageReference Include="OpenRouter.SDK" Version="1.0.0" />
```

## Quick Start

### Simple Text Completion

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var response = await client.CallModelAsync(
    model: "openai/gpt-3.5-turbo",
    userMessage: "What is the capital of France?",
    systemMessage: "You are a helpful assistant."
);

Console.WriteLine(response);
```

### Streaming Response

```csharp
await foreach (var chunk in client.CallModelStreamAsync(
    model: "openai/gpt-3.5-turbo",
    userMessage: "Write a short poem about coding."
))
{
    Console.Write(chunk);
}
```

### Advanced Chat Completion

```csharp
using OpenRouter.SDK.Models;

var request = new ChatCompletionRequest
{
    Model = "anthropic/claude-3-sonnet",
    Messages = new List<Message>
    {
        new SystemMessage { Content = "You are a helpful coding assistant." },
        new UserMessage { Content = "Explain async/await in C#." }
    },
    Temperature = 0.7,
    MaxTokens = 500
};

var response = await client.Chat.CreateAsync(request);
Console.WriteLine(response.Choices[0].Message.Content);
Console.WriteLine($"Tokens used: {response.Usage?.TotalTokens}");
```

## Configuration

### Basic Configuration

```csharp
var client = new OpenRouterClient("YOUR_API_KEY", options =>
{
    options.BaseUrl = "https://openrouter.ai/api/v1";
    options.DefaultHeaders["HTTP-Referer"] = "https://yourapp.com";
    options.DefaultHeaders["X-Title"] = "Your App Name";
});
```


### HTTP Hooks

The SDK provides events for logging and monitoring HTTP requests and responses.

```csharp
var client = new OpenRouterClient("YOUR_API_KEY");

// Log all requests
client.BeforeRequest += async (request) =>
{
    Console.WriteLine($"Sending request to: {request.RequestUri}");
    await Task.CompletedTask;
};

// Log all responses
client.AfterResponse += async (response) =>
{
    Console.WriteLine($"Received response: {response.StatusCode}");
    await Task.CompletedTask;
};

// Handle errors
client.OnError += async (exception, request) =>
{
    Console.WriteLine($"Error: {exception.Message}");
    await Task.CompletedTask;
};
```

For advanced scenarios, access the HTTP client directly:

```csharp
var client = new OpenRouterClient("YOUR_API_KEY");

if (client.HttpClient != null)
{
    // Modify requests before sending
    client.HttpClient.AddBeforeRequestHook(async (request) =>
    {
        request.Headers.Add("X-Tracking-Id", Guid.NewGuid().ToString());
        return request;
    });
}
```

## API Reference

For comprehensive API documentation, see the [SDK Documentation](docs/sdks/).

### Available Services

The SDK provides the following services through the `OpenRouterClient`:

- **[Analytics](docs/sdks/analytics/)** - User activity and analytics tracking
- **[API Keys](docs/sdks/apikeys/)** - API key management (create, list, update, delete)
- **[Chat](docs/sdks/chat/)** - Chat completions with streaming support
- **[Credits](docs/sdks/credits/)** - Credit balance and payment management
- **[Embeddings](docs/sdks/embeddings/)** - Text embedding generation
- **[Endpoints](docs/sdks/endpoints/)** - Model endpoint information
- **[Generations](docs/sdks/generations/)** - Generation metadata and usage tracking
- **[Guardrails](docs/sdks/guardrails/)** - Content moderation and safety controls
- **[Models](docs/sdks/models/)** - Model listing and information
- **[OAuth](docs/sdks/oauth/)** - OAuth2 PKCE authentication flow
- **[Providers](docs/sdks/providers/)** - Provider information
- **[Responses](docs/sdks/responses/)** - Beta Responses API (modern interface)

### OpenRouterClient

Main client for interacting with the OpenRouter API.

#### Constructor

```csharp
public OpenRouterClient(string apiKey, Action<OpenRouterOptions>? configure = null)
```

#### Service Properties

```csharp
// Analytics and monitoring
IAnalyticsService Analytics { get; }

// API key management
IApiKeysService ApiKeys { get; }

// Chat completions
IChatService Chat { get; }

// Credit management
ICreditsService Credits { get; }

// Embeddings
IEmbeddingsService Embeddings { get; }

// Endpoints
IEndpointsService Endpoints { get; }

// Generation metadata
IGenerationsService Generations { get; }

// Content moderation
IGuardrailsService Guardrails { get; }

// Model information
IModelsService Models { get; }

// OAuth authentication
IOAuthService OAuth { get; }

// Provider information
IProvidersService Providers { get; }

// Beta Responses API
IBetaResponsesService BetaResponses { get; }
```

#### Convenience Methods

- `Task<string> CallModelAsync(...)` - Simple text completion
- `IAsyncEnumerable<string> CallModelStreamAsync(...)` - Streaming text completion

## Error Handling

The SDK provides specific exception types for different error scenarios:

```csharp
try
{
    var response = await client.Chat.CreateAsync(request);
}
catch (UnauthorizedException ex)
{
    Console.WriteLine("Invalid API key");
}
catch (RateLimitException ex)
{
    Console.WriteLine($"Rate limited. Retry after: {ex.ErrorData?.RetryAfter}");
}
catch (BadRequestException ex)
{
    Console.WriteLine($"Bad request: {ex.ErrorData?.Message}");
}
catch (OpenRouterException ex)
{
    Console.WriteLine($"API error: {ex.Message}");
}
```

## Stop Conditions

Control when multi-turn tool orchestration loops should terminate using comprehensive stop conditions:

```csharp
var result = client.Beta.Responses.CallModel(
    request,
    tools: tools,
    stopConditions: new[]
    {
        StopConditions.Any(
            StopConditions.StepCountIs(10),           // Max 10 turns
            StopConditions.MaxTokensUsed(50000),       // Token budget
            StopConditions.MaxCost(1.00),              // Cost limit ($1)
            StopConditions.HasToolCall("finalize")     // Semantic completion
        )
    }
);
```


## Concurrent Stream Consumption

Consume the same streaming response from multiple independent consumers concurrently using all available stream patterns:

```csharp
var request = new BetaResponsesRequest
{
    Model = "openai/gpt-4",
    Input = "Write a story about AI",
    Stream = true  // Enable streaming
};

var result = new ModelResult(client.Beta.Responses, request);

// Multiple consumers reading from same stream concurrently
var textTask = Task.Run(async () =>
{
    var text = new StringBuilder();
    await foreach (var chunk in result.GetTextStreamAsync())
    {
        text.Append(chunk);
    }
    return text.ToString();
});

var messagesTask = Task.Run(async () =>
{
    await foreach (var message in result.GetNewMessagesStreamAsync())
    {
        Console.WriteLine($"Message: {message.Role}");
    }
});

var fullStreamTask = Task.Run(async () =>
{
    int eventCount = 0;
    await foreach (var chunk in result.GetFullStreamAsync())
    {
        eventCount++;
    }
    return eventCount;
});

await Task.WhenAll(textTask, messagesTask, fullStreamTask);
Console.WriteLine($"Text: {textTask.Result}");
Console.WriteLine($"Events: {fullStreamTask.Result}");
```


## Examples

### Quick Examples

See the [SDK Documentation](docs/sdks/) for detailed examples of each service.

The `Examples/` folder contains comprehensive example projects:

- **Example01.SimpleTextCompletion** - Basic text completion
- **Example02.StreamingCompletion** - Streaming responses
- **Example03.ChatServiceDirect** - Direct chat service usage
- **Example04.ListModels** - Retrieving available models
- **Example05.AdvancedChatParameters** - Advanced chat configurations
- **Example06.GenerateEmbeddings** - Text embeddings
- **Example07.ListProviders** - Provider information
- **Example08.ListEndpoints** - Model endpoints
- **Example09.ZDREndpoints** - Zero Data Retention endpoints
- **Example10.BetaResponsesAPI** - Modern responses interface
- **Example11.BetaResponsesWithTools** - Tool/function calling
- **Example12.AutomaticWeatherTool** - Automatic tool execution
- **Example13.MultiToolCalculator** - Multiple tools
- **Example14.ModelResultPatterns** - Result handling patterns
- **Example15.GenerationMetadata** - Usage tracking
- **Example16.OAuthPKCE** - OAuth authentication flow
- **Example17.AsyncParameterResolution** - Advanced async patterns
- **Example18.APIKeysManagement** - API key CRUD operations
- **Example19.StopConditions** - Complete stop conditions (limits, budget, completion)
- **Example20.MultiTurnOrchestration** - Multi-turn conversations
- **Example21.OrchestratedWorkflow** - Complex workflows
- **Example22.ChainedLLMWorkflow** - Chained LLM calls
- **Example23.ChainedLLMWorkflowAdvanced** - Advanced chaining
- **Example24.PracticalUseCases** - Real-world scenarios
- **Example25.HttpHooks** - HTTP request/response hooks and logging
- **Example26.StopConditions** - All stop condition helpers with modern Tool API
- **Example27.ConcurrentStreams** - Multiple concurrent stream consumers
- **Example28.AllStreamingPatterns** - Complete demonstration of all 7 streaming patterns

All examples use centralized configuration via `OpenRouter.Examples.EnvConfig` for API keys and model names.

## Testing

Run the test suite:

```bash
dotnet test
```

The SDK includes comprehensive unit tests using xUnit, FluentAssertions, and Moq.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Acknowledgments

- Converted from the official [OpenRouter TypeScript SDK](https://github.com/openrouter/typescript-sdk)
- Built with inspiration from the conversion guides in this repository

## Documentation

- **[SDK Documentation](docs/sdks/)** - Complete API reference for all services
- **[Stop Conditions](docs/STOP_CONDITIONS.md)** - Comprehensive guide to controlling tool orchestration
- **[Reusable Stream](docs/REUSABLE_STREAM.md)** - Multiple concurrent consumers from single stream
- **[Streaming Patterns Implementation](STREAMING_PATTERNS_IMPLEMENTATION.md)** - All 7 streaming patterns with TypeScript parity
- **[HTTP Hooks](HTTP_HOOKS.md)** - Request/response monitoring and customization
- **[OpenRouter Docs](https://openrouter.ai/docs)** - Official OpenRouter documentation
- **[Examples Guide](ALL_EXAMPLES_GUIDE.md)** - Comprehensive examples guide
- **[Quick Start](QUICK_START.md)** - Get started quickly
- **[Configuration Guide](CONFIGURATION.md)** - Configuration options

## Support

- SDK Documentation: [docs/sdks/](docs/sdks/)
- OpenRouter Docs: [https://openrouter.ai/docs](https://openrouter.ai/docs)
- API Key: Get yours at [https://openrouter.ai/keys](https://openrouter.ai/keys)

**Have a feature request?** Open an issue on GitHub to discuss it!
