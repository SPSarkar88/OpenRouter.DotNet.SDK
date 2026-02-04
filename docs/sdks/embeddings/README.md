# Embeddings

## Overview

Text embedding endpoints for generating vector representations of text

### Available Operations

* [GenerateAsync](#generateasync) - Submit an embedding request
* [ListModelsAsync](#listmodelsasync) - List all embeddings models

## GenerateAsync

Submits an embedding request to the embeddings router to generate vector representations of text.

### Example Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new EmbeddingRequest
{
    Model = "text-embedding-ada-002",
    Input = "The quick brown fox jumps over the lazy dog"
};

var response = await client.Embeddings.GenerateAsync(request);

Console.WriteLine($"Embedding dimensions: {response.Data[0].Embedding.Length}");
Console.WriteLine($"Model used: {response.Model}");
Console.WriteLine($"Total tokens: {response.Usage.TotalTokens}");
```

### Multiple Text Inputs

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new EmbeddingRequest
{
    Model = "text-embedding-ada-002",
    Input = new List<string>
    {
        "First text to embed",
        "Second text to embed",
        "Third text to embed"
    }
};

var response = await client.Embeddings.GenerateAsync(request);

foreach (var embedding in response.Data)
{
    Console.WriteLine($"Index {embedding.Index}: {embedding.Embedding.Length} dimensions");
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
var embeddingsService = serviceProvider.GetRequiredService<IEmbeddingsService>();

var request = new EmbeddingRequest
{
    Model = "text-embedding-ada-002",
    Input = "Sample text"
};

var response = await embeddingsService.GenerateAsync(request);
```

### Parameters

| Parameter           | Type              | Required           | Description                          |
| ------------------- | ----------------- | ------------------ | ------------------------------------ |
| `request`           | EmbeddingRequest  | :heavy_check_mark: | The embedding request                |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<EmbeddingResponse\>**

### Errors

| Error Type                    | Status Code | Content Type     |
| ----------------------------- | ----------- | ---------------- |
| BadRequestException           | 400         | application/json |
| UnauthorizedException         | 401         | application/json |
| PaymentRequiredException      | 402         | application/json |
| NotFoundException             | 404         | application/json |
| TooManyRequestsException      | 429         | application/json |
| InternalServerException       | 500         | application/json |
| BadGatewayException           | 502         | application/json |
| ServiceUnavailableException   | 503         | application/json |
| EdgeNetworkTimeoutException   | 524         | application/json |
| ProviderOverloadedException   | 529         | application/json |
| OpenRouterException           | 4XX, 5XX    | \*/\*            |

## ListModelsAsync

Returns a list of all available embeddings models and their properties.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var response = await client.Embeddings.ListModelsAsync();

Console.WriteLine($"Available embedding models: {response.Data.Count}");

foreach (var model in response.Data)
{
    Console.WriteLine($"Model: {model.Id}");
    Console.WriteLine($"  Context Length: {model.ContextLength}");
    Console.WriteLine($"  Pricing: ${model.Pricing.Prompt} per token");
}
```

### Filter and Sort Models

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var response = await client.Embeddings.ListModelsAsync();

// Find cheapest model
var cheapestModel = response.Data
    .OrderBy(m => m.Pricing.Prompt)
    .First();

Console.WriteLine($"Cheapest model: {cheapestModel.Id}");
Console.WriteLine($"Price: ${cheapestModel.Pricing.Prompt} per token");

// Find models with large context
var largeContextModels = response.Data
    .Where(m => m.ContextLength >= 8192)
    .ToList();

Console.WriteLine($"\nModels with 8K+ context: {largeContextModels.Count}");
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
var embeddingsService = serviceProvider.GetRequiredService<IEmbeddingsService>();

var response = await embeddingsService.ListModelsAsync();
```

### Parameters

| Parameter           | Type              | Required           | Description                          |
| ------------------- | ----------------- | ------------------ | ------------------------------------ |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<ModelsResponse\>**

### Errors

| Error Type                  | Status Code | Content Type     |
| --------------------------- | ----------- | ---------------- |
| InternalServerException     | 500         | application/json |
| OpenRouterException         | 4XX, 5XX    | \*/\*            |
