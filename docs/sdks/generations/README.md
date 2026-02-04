# Generations

## Overview

Generation metadata and usage information

### Available Operations

* [GetGenerationAsync](#getgenerationasync) - Get request and usage metadata for a generation

## GetGenerationAsync

Get request and usage metadata for a generation, including cost, token usage, and other information.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var result = await client.Generations.GetGenerationAsync("gen_abc123xyz");

Console.WriteLine($"Generation ID: {result.Id}");
Console.WriteLine($"Model: {result.Model}");
Console.WriteLine($"Total Cost: ${result.TotalCost}");
Console.WriteLine($"Native Tokens (Prompt): {result.NativeTokensPrompt}");
Console.WriteLine($"Native Tokens (Completion): {result.NativeTokensCompletion}");
Console.WriteLine($"Created At: {result.CreatedAt}");
```

### Advanced Usage - Tracking Costs

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

// First, make a chat completion
var chatRequest = new ChatCompletionRequest
{
    Model = "openai/gpt-4",
    Messages = new List<ChatMessage>
    {
        new ChatMessage { Role = "user", Content = "Hello!" }
    }
};

var chatResponse = await client.Chat.CreateAsync(chatRequest);
var generationId = chatResponse.Id;

// Then get the generation metadata to see costs
var generation = await client.Generations.GetGenerationAsync(generationId);

Console.WriteLine($"Request cost ${generation.TotalCost}");
Console.WriteLine($"Prompt tokens: {generation.NativeTokensPrompt}");
Console.WriteLine($"Completion tokens: {generation.NativeTokensCompletion}");
Console.WriteLine($"Total tokens: {generation.NativeTokensPrompt + generation.NativeTokensCompletion}");
```

### Standalone Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.SDK;

var services = new ServiceCollection();
services.AddOpenRouter(options =>
{
    options.ApiKey = "YOUR_API_KEY";
});

var serviceProvider = services.BuildServiceProvider();
var generationsService = serviceProvider.GetRequiredService<IGenerationsService>();

var result = await generationsService.GetGenerationAsync("gen_abc123xyz");
```

### Parameters

| Parameter           | Type              | Required           | Description                               |
| ------------------- | ----------------- | ------------------ | ----------------------------------------- |
| `generationId`      | string            | :heavy_check_mark: | The unique identifier of the generation   |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation      |

### Response

**Task\<GenerationResponse\>**

### Errors

| Error Type                  | Status Code | Content Type     |
| --------------------------- | ----------- | ---------------- |
| BadRequestException         | 400         | application/json |
| UnauthorizedException       | 401         | application/json |
| NotFoundException           | 404         | application/json |
| InternalServerException     | 500         | application/json |
| OpenRouterException         | 4XX, 5XX    | \*/\*            |
