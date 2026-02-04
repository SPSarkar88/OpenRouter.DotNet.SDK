# Responses

## Overview

Beta Responses API - modern structured response interface with enhanced capabilities

### Available Operations

* [SendAsync](#sendasync) - Send a request to the Beta Responses API
* [SendStreamAsync](#sendstreamasync) - Send a streaming request to the Beta Responses API

## SendAsync

Send a request to the Beta Responses API for structured, non-streaming responses. This API provides a more modern interface with enhanced capabilities over the standard chat completions endpoint.

### Example Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new BetaResponsesRequest
{
    Model = "openai/gpt-4",
    Messages = new List<ChatMessage>
    {
        new ChatMessage
        {
            Role = "user",
            Content = "Explain quantum entanglement in simple terms."
        }
    },
    MaxTokens = 200,
    Temperature = 0.7
};

var response = await client.BetaResponses.SendAsync(request);

Console.WriteLine($"Response: {response.Content}");
Console.WriteLine($"Model: {response.Model}");
Console.WriteLine($"Tokens Used: {response.Usage.TotalTokens}");
```

### With Tools

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new BetaResponsesRequest
{
    Model = "openai/gpt-4",
    Messages = new List<ChatMessage>
    {
        new ChatMessage
        {
            Role = "user",
            Content = "What's the weather in Tokyo?"
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
                Description = "Get current weather for a location",
                Parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        location = new { type = "string", description = "City name" },
                        unit = new { type = "string", @enum = new[] { "celsius", "fahrenheit" } }
                    },
                    required = new[] { "location" }
                }
            }
        }
    }
};

var response = await client.BetaResponses.SendAsync(request);

if (response.ToolCalls?.Any() == true)
{
    foreach (var toolCall in response.ToolCalls)
    {
        Console.WriteLine($"Tool: {toolCall.Function.Name}");
        Console.WriteLine($"Arguments: {toolCall.Function.Arguments}");
    }
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
var responsesService = serviceProvider.GetRequiredService<IBetaResponsesService>();

var request = new BetaResponsesRequest
{
    Model = "openai/gpt-4",
    Messages = new List<ChatMessage>
    {
        new ChatMessage { Role = "user", Content = "Hello!" }
    }
};

var response = await responsesService.SendAsync(request);
```

### Parameters

| Parameter           | Type                   | Required           | Description                          |
| ------------------- | ---------------------- | ------------------ | ------------------------------------ |
| `request`           | BetaResponsesRequest   | :heavy_check_mark: | The responses request                |
| `cancellationToken` | CancellationToken      | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<BetaResponsesResponse\>**

### Errors

| Error Type                    | Status Code       | Content Type     |
| ----------------------------- | ----------------- | ---------------- |
| BadRequestException           | 400               | application/json |
| UnauthorizedException         | 401               | application/json |
| PaymentRequiredException      | 402               | application/json |
| NotFoundException             | 404               | application/json |
| TooManyRequestsException      | 429               | application/json |
| InternalServerException       | 500               | application/json |
| BadGatewayException           | 502               | application/json |
| ServiceUnavailableException   | 503               | application/json |
| OpenRouterException           | 4XX, 5XX          | \*/\*            |

## SendStreamAsync

Send a streaming request to the Beta Responses API. Returns responses as they are generated in real-time.

### Example Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new BetaResponsesRequest
{
    Model = "openai/gpt-4",
    Messages = new List<ChatMessage>
    {
        new ChatMessage
        {
            Role = "user",
            Content = "Write a creative story about a time-traveling scientist."
        }
    },
    MaxTokens = 1000,
    Temperature = 0.9,
    Stream = true
};

Console.Write("Response: ");

await foreach (var chunk in client.BetaResponses.SendStreamAsync(request))
{
    if (!string.IsNullOrEmpty(chunk.Content))
    {
        Console.Write(chunk.Content);
    }
    
    // Check for tool calls in stream
    if (chunk.ToolCalls?.Any() == true)
    {
        foreach (var toolCall in chunk.ToolCalls)
        {
            Console.WriteLine($"\nTool called: {toolCall.Function.Name}");
        }
    }
}

Console.WriteLine();
```

### With Cancellation

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");
var cts = new CancellationTokenSource();

// Cancel after 30 seconds
cts.CancelAfter(TimeSpan.FromSeconds(30));

var request = new BetaResponsesRequest
{
    Model = "openai/gpt-4",
    Messages = new List<ChatMessage>
    {
        new ChatMessage
        {
            Role = "user",
            Content = "Explain the history of the universe."
        }
    },
    Stream = true
};

try
{
    await foreach (var chunk in client.BetaResponses.SendStreamAsync(request, cts.Token))
    {
        if (!string.IsNullOrEmpty(chunk.Content))
        {
            Console.Write(chunk.Content);
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nStream cancelled.");
}
```

### Real-time UI Updates

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;
using System.Text;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new BetaResponsesRequest
{
    Model = "openai/gpt-4",
    Messages = new List<ChatMessage>
    {
        new ChatMessage { Role = "user", Content = "Describe artificial intelligence." }
    },
    Stream = true
};

var fullResponse = new StringBuilder();

await foreach (var chunk in client.BetaResponses.SendStreamAsync(request))
{
    if (!string.IsNullOrEmpty(chunk.Content))
    {
        fullResponse.Append(chunk.Content);
        
        // Update UI in real-time
        // UpdateUI(fullResponse.ToString());
        Console.Write(chunk.Content);
    }
}

Console.WriteLine($"\n\nFull response length: {fullResponse.Length} characters");
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
var responsesService = serviceProvider.GetRequiredService<IBetaResponsesService>();

var request = new BetaResponsesRequest
{
    Model = "openai/gpt-4",
    Messages = new List<ChatMessage>
    {
        new ChatMessage { Role = "user", Content = "Hello!" }
    },
    Stream = true
};

await foreach (var chunk in responsesService.SendStreamAsync(request))
{
    // Process chunk
}
```

### Parameters

| Parameter           | Type                   | Required           | Description                          |
| ------------------- | ---------------------- | ------------------ | ------------------------------------ |
| `request`           | BetaResponsesRequest   | :heavy_check_mark: | The responses request with stream enabled |
| `cancellationToken` | CancellationToken      | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**IAsyncEnumerable\<BetaResponsesStreamChunk\>**

### Errors

| Error Type                    | Status Code       | Content Type     |
| ----------------------------- | ----------------- | ---------------- |
| BadRequestException           | 400               | application/json |
| UnauthorizedException         | 401               | application/json |
| PaymentRequiredException      | 402               | application/json |
| NotFoundException             | 404               | application/json |
| TooManyRequestsException      | 429               | application/json |
| InternalServerException       | 500               | application/json |
| BadGatewayException           | 502               | application/json |
| ServiceUnavailableException   | 503               | application/json |
| OpenRouterException           | 4XX, 5XX          | \*/\*            |
