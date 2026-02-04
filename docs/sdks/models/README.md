# Models

## Overview

Model information endpoints

### Available Operations

* [GetModelsAsync](#getmodelsasync) - List all models and their properties
* [GetCountAsync](#getcountasync) - Get total count of available models
* [GetModelsForUserAsync](#getmodelsforuserasync) - List models filtered by user preferences

## GetModelsAsync

List all models and their properties, including pricing, context length, and capabilities.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var result = await client.Models.GetModelsAsync();

Console.WriteLine($"Total models: {result.Data.Count}");

foreach (var model in result.Data)
{
    Console.WriteLine($"\nModel: {model.Id}");
    Console.WriteLine($"  Name: {model.Name}");
    Console.WriteLine($"  Context Length: {model.ContextLength}");
    Console.WriteLine($"  Prompt Price: ${model.Pricing.Prompt}");
    Console.WriteLine($"  Completion Price: ${model.Pricing.Completion}");
}
```

### Filter Models by Capabilities

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var result = await client.Models.GetModelsAsync();

// Find models with function calling support
var functionCallModels = result.Data
    .Where(m => m.SupportedGenerationMethods?.Contains("function_call") == true)
    .ToList();

Console.WriteLine($"Models with function calling: {functionCallModels.Count}");

// Find cheapest model with >100k context
var cheapLargeContext = result.Data
    .Where(m => m.ContextLength >= 100000)
    .OrderBy(m => m.Pricing.Prompt)
    .FirstOrDefault();

if (cheapLargeContext != null)
{
    Console.WriteLine($"\nCheapest 100K+ context model: {cheapLargeContext.Id}");
    Console.WriteLine($"Price: ${cheapLargeContext.Pricing.Prompt} per token");
}
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
var modelsService = serviceProvider.GetRequiredService<IModelsService>();

var result = await modelsService.GetModelsAsync();
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

## GetCountAsync

Get total count of available models.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var result = await client.Models.GetCountAsync();

Console.WriteLine($"Total available models: {result.Count}");
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
var modelsService = serviceProvider.GetRequiredService<IModelsService>();

var result = await modelsService.GetCountAsync();
```

### Parameters

| Parameter           | Type              | Required           | Description                          |
| ------------------- | ----------------- | ------------------ | ------------------------------------ |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<ModelsCountResponse\>**

### Errors

| Error Type                  | Status Code | Content Type     |
| --------------------------- | ----------- | ---------------- |
| InternalServerException     | 500         | application/json |
| OpenRouterException         | 4XX, 5XX    | \*/\*            |

## GetModelsForUserAsync

List models filtered by user provider preferences, privacy settings, and guardrails.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var result = await client.Models.GetModelsForUserAsync();

Console.WriteLine($"Models available for your account: {result.Data.Count}");

foreach (var model in result.Data)
{
    Console.WriteLine($"Model: {model.Id}");
    Console.WriteLine($"  Enabled: {model.Enabled}");
    Console.WriteLine($"  Privacy: {model.Privacy}");
}
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
var modelsService = serviceProvider.GetRequiredService<IModelsService>();

var result = await modelsService.GetModelsForUserAsync();
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
| UnauthorizedException       | 401         | application/json |
| InternalServerException     | 500         | application/json |
| OpenRouterException         | 4XX, 5XX    | \*/\*            |
