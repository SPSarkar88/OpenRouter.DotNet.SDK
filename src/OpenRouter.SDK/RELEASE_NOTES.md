# OpenRouter.SDK v1.0.0 - Release Notes

**Release Date:** February 6, 2026

## 🎉 Initial Release

We're excited to announce the initial release of **OpenRouter.SDK** - a modern, type-safe .NET SDK for the [OpenRouter](https://openrouter.ai/) API, providing access to 300+ AI models through a unified interface.

## 📦 Package Information

- **Package ID:** OpenRouter.SDK
- **Version:** 1.0.0
- **Target Framework:** .NET 8.0
- **License:** Apache-2.0
- **Author:** Subhra Prakash Sarkar
- **Project URL:** https://github.com/SPSarkar88/OpenRouter.DotNet.SDK
- **Repository URL:** https://github.com/SPSarkar88/OpenRouter.DotNet.SDK

## ✨ Key Features

### Core Functionality
- ✅ **Type-safe API** - Full C# type safety with nullable reference types
- ✅ **Modern Async Patterns** - Built on async/await throughout
- ✅ **Native Streaming Support** - `IAsyncEnumerable<T>` for streaming responses
- ✅ **Concurrent Streaming** - Multiple independent consumers can read from the same stream simultaneously
- ✅ **7 Stream Patterns** - Text, Reasoning, Tool Calls, Tool Results, Messages, Responses, and Full Events
- ✅ **Built-in Resilience** - Retry policies powered by Polly
- ✅ **HTTP Hooks** - Extensible request/response hooks for logging and customization
- ✅ **.NET 8.0** - Built on the latest .NET with C# 12 features

### Comprehensive API Coverage

The SDK provides full access to all OpenRouter API services:

- **Analytics** - User activity and analytics tracking
- **API Keys Management** - Create, list, update, and delete API keys
- **Chat Completions** - Text generation with streaming support
- **Credits** - Credit balance and payment management
- **Embeddings** - Text embedding generation
- **Endpoints** - Model endpoint information and Zero Data Retention endpoints
- **Generations** - Generation metadata and usage tracking
- **Guardrails** - Content moderation and safety controls
- **Models** - Model listing and detailed information
- **OAuth** - OAuth2 PKCE authentication flow
- **Providers** - Provider information and capabilities
- **Beta Responses API** - Modern interface for chat completions

### Advanced Capabilities

#### Tool Orchestration
- Automatic tool/function calling with built-in orchestration
- Multi-turn conversation handling
- Comprehensive stop conditions (step count, token budget, cost limits, semantic completion)
- Support for complex chained LLM workflows

#### Streaming Patterns
All 7 streaming patterns with TypeScript SDK parity:
1. **Text Stream** - Raw text chunks
2. **Reasoning Stream** - Reasoning/thinking process
3. **Tool Calls Stream** - Tool invocations as they occur
4. **Tool Results Stream** - Tool execution results
5. **Messages Stream** - Complete message objects
6. **Responses Stream** - Full response objects
7. **Full Event Stream** - All SSE events

#### HTTP Customization
- Before/after request hooks
- Error handling hooks
- Custom header injection
- Request/response logging
- HTTP client access for advanced scenarios

### Error Handling
Specific exception types for different error scenarios:
- `UnauthorizedException` - Authentication failures
- `RateLimitException` - Rate limit errors with retry information
- `BadRequestException` - Invalid request errors
- `OpenRouterException` - General API errors

## 📚 Documentation

Comprehensive documentation included:
- Complete API reference for all 13 services
- Stop Conditions guide
- Reusable Stream patterns guide
- HTTP Hooks documentation
- Streaming Patterns implementation guide
- Configuration guide
- Quick Start guide

## 🎯 Example Projects

28 comprehensive example projects demonstrating:
- Simple and streaming completions
- Advanced chat parameters
- Tool/function calling
- Multi-turn orchestration
- OAuth authentication
- API key management
- Concurrent stream consumption
- All streaming patterns
- HTTP hooks and logging
- Real-world practical use cases

## 📋 Dependencies

- Microsoft.Extensions.Configuration.Abstractions (10.0.2)
- Microsoft.Extensions.Http (10.0.2)
- Microsoft.Extensions.Logging.Abstractions (10.0.2)
- Polly (8.6.5)
- RestSharp (113.1.0)
- System.Text.Json (10.0.2)

## 🚀 Getting Started

### Installation

```bash
dotnet add package OpenRouter.SDK
```

Or add to your `.csproj` file:

```xml
<PackageReference Include="OpenRouter.SDK" Version="1.0.0" />
```

### Quick Start

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

### Streaming Example

```csharp
await foreach (var chunk in client.CallModelStreamAsync(
    model: "openai/gpt-3.5-turbo",
    userMessage: "Write a short poem about coding."
))
{
    Console.Write(chunk);
}
```

## 🔗 Links

- **GitHub Repository:** https://github.com/SPSarkar88/OpenRouter.DotNet.SDK
- **NuGet Package:** https://www.nuget.org/packages/OpenRouter.SDK
- **OpenRouter API Docs:** https://openrouter.ai/docs
- **Get API Key:** https://openrouter.ai/keys

## 🙏 Acknowledgments

This SDK is converted from the official [OpenRouter TypeScript SDK](https://github.com/openrouter/typescript-sdk) with .NET-specific enhancements and features.

## 📄 License

Licensed under the Apache-2.0 License.

## 💬 Support & Feedback

- **Issues:** https://github.com/SPSarkar88/OpenRouter.DotNet.SDK/issues
- **Discussions:** https://github.com/SPSarkar88/OpenRouter.DotNet.SDK/discussions

---

**Note:** This is the initial stable release. Future versions will include additional features, improvements, and bug fixes based on community feedback.
