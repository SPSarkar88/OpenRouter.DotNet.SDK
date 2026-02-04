# Chat

## Overview

Chat completion endpoints for sending messages to AI models and receiving responses. Supports both streaming and non-streaming modes.

### Available Operations

* [CreateAsync](#createasync) - Create a chat completion
* [CreateStreamAsync](#createstreamasync) - Create a streaming chat completion

## CreateAsync

Sends a request for a model response for the given chat conversation. Returns the complete response at once.

### Example Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new ChatCompletionRequest
{
    Model = "openai/gpt-4",
    Messages = new List<ChatMessage>
    {
        new ChatMessage
        {
            Role = "user",
            Content = "What is the capital of France?"
        }
    },
    MaxTokens = 100,
    Temperature = 0.7
};

var response = await client.Chat.CreateAsync(request);

Console.WriteLine(response.Choices[0].Message.Content);
```

### Advanced Example with Tools

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new ChatCompletionRequest
{
    Model = "openai/gpt-4",
    Messages = new List<ChatMessage>
    {
        new ChatMessage
        {
            Role = "user",
            Content = "What's the weather like in Paris?"
        }
    },
    Tools = new List<Tool>
    {
        new Tool
        {
            Type = "function",
            Function = new ToolFunction
            {
                Name = "get_weather",
                Description = "Get the current weather for a location",
                Parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        location = new { type = "string" },
                        unit = new { type = "string", @enum = new[] { "celsius", "fahrenheit" } }
                    },
                    required = new[] { "location" }
                }
            }
        }
    }
};

var response = await client.Chat.CreateAsync(request);
```

### Standalone Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var services = new ServiceCollection();
services.AddOpenRouter(options =>
{
    options.ApiKey = "YOUR_API_KEY";
});

var serviceProvider = services.BuildServiceProvider();
var chatService = serviceProvider.GetRequiredService<IChatService>();

var request = new ChatCompletionRequest
{
    Model = "openai/gpt-4",
    Messages = new List<ChatMessage>
    {
        new ChatMessage { Role = "user", Content = "Hello!" }
    }
};

var response = await chatService.CreateAsync(request);
```

### Parameters

| Parameter           | Type                    | Required           | Description                          |
| ------------------- | ----------------------- | ------------------ | ------------------------------------ |
| `request`           | ChatCompletionRequest   | :heavy_check_mark: | The chat completion request          |
| `cancellationToken` | CancellationToken       | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<ChatCompletionResponse\>**

### Errors

| Error Type                    | Status Code       | Content Type     |
| ----------------------------- | ----------------- | ---------------- |
| ChatException                 | 400, 401, 429     | application/json |
| ChatException                 | 500               | application/json |
| OpenRouterException           | 4XX, 5XX          | \*/\*            |

## CreateStreamAsync

Sends a request for a model response for the given chat conversation with streaming enabled. Returns responses as they are generated.

### Example Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new ChatCompletionRequest
{
    Model = "openai/gpt-4",
    Messages = new List<ChatMessage>
    {
        new ChatMessage
        {
            Role = "user",
            Content = "Write a short story about a robot."
        }
    },
    MaxTokens = 500,
    Temperature = 0.8
};

await foreach (var chunk in client.Chat.CreateStreamAsync(request))
{
    if (chunk.Choices?.Count > 0 && chunk.Choices[0].Delta?.Content != null)
    {
        Console.Write(chunk.Choices[0].Delta.Content);
    }
}
```

### Advanced Streaming with Cancellation

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");
var cts = new CancellationTokenSource();

// Cancel after 10 seconds
cts.CancelAfter(TimeSpan.FromSeconds(10));

var request = new ChatCompletionRequest
{
    Model = "openai/gpt-4",
    Messages = new List<ChatMessage>
    {
        new ChatMessage { Role = "user", Content = "Explain quantum computing." }
    }
};

try
{
    await foreach (var chunk in client.Chat.CreateStreamAsync(request, cts.Token))
    {
        if (chunk.Choices?.Count > 0 && chunk.Choices[0].Delta?.Content != null)
        {
            Console.Write(chunk.Choices[0].Delta.Content);
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nStream cancelled.");
}
```

### Standalone Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var services = new ServiceCollection();
services.AddOpenRouter(options =>
{
    options.ApiKey = "YOUR_API_KEY";
});

var serviceProvider = services.BuildServiceProvider();
var chatService = serviceProvider.GetRequiredService<IChatService>();

var request = new ChatCompletionRequest
{
    Model = "openai/gpt-4",
    Messages = new List<ChatMessage>
    {
        new ChatMessage { Role = "user", Content = "Hello!" }
    }
};

await foreach (var chunk in chatService.CreateStreamAsync(request))
{
    // Process chunk
}
```

### Parameters

| Parameter           | Type                    | Required           | Description                          |
| ------------------- | ----------------------- | ------------------ | ------------------------------------ |
| `request`           | ChatCompletionRequest   | :heavy_check_mark: | The chat completion request          |
| `cancellationToken` | CancellationToken       | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**IAsyncEnumerable\<ChatCompletionChunk\>**

### Errors

| Error Type                    | Status Code       | Content Type     |
| ----------------------------- | ----------------- | ---------------- |
| ChatException                 | 400, 401, 429     | application/json |
| ChatException                 | 500               | application/json |
| OpenRouterException           | 4XX, 5XX          | \*/\*            |
