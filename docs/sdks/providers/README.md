# Providers

## Overview

Provider information endpoints

### Available Operations

* [ListAsync](#listasync) - List all providers

## ListAsync

List all providers available on OpenRouter, including their status and supported models.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var result = await client.Providers.ListAsync();

Console.WriteLine($"Total providers: {result.Data.Count}");

foreach (var provider in result.Data)
{
    Console.WriteLine($"\nProvider: {provider.Id}");
    Console.WriteLine($"  Name: {provider.Name}");
    Console.WriteLine($"  Status: {provider.Status}");
    Console.WriteLine($"  Models: {provider.ModelCount}");
}
```

### Filter Providers by Status

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var result = await client.Providers.ListAsync();

// Find active providers
var activeProviders = result.Data
    .Where(p => p.Status == "active")
    .ToList();

Console.WriteLine($"Active providers: {activeProviders.Count}");

foreach (var provider in activeProviders)
{
    Console.WriteLine($"- {provider.Name}");
}

// Find providers with most models
var topProvider = result.Data
    .OrderByDescending(p => p.ModelCount)
    .FirstOrDefault();

if (topProvider != null)
{
    Console.WriteLine($"\nProvider with most models: {topProvider.Name}");
    Console.WriteLine($"Model count: {topProvider.ModelCount}");
}
```

### Get Provider Details

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var result = await client.Providers.ListAsync();

// Find specific provider
var openaiProvider = result.Data
    .FirstOrDefault(p => p.Id == "openai");

if (openaiProvider != null)
{
    Console.WriteLine($"Provider: {openaiProvider.Name}");
    Console.WriteLine($"Website: {openaiProvider.Website}");
    Console.WriteLine($"Description: {openaiProvider.Description}");
    Console.WriteLine($"Available Models: {openaiProvider.ModelCount}");
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
var providersService = serviceProvider.GetRequiredService<IProvidersService>();

var result = await providersService.ListAsync();
```

### Parameters

| Parameter           | Type              | Required           | Description                          |
| ------------------- | ----------------- | ------------------ | ------------------------------------ |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<ProvidersResponse\>**

### Errors

| Error Type                  | Status Code | Content Type     |
| --------------------------- | ----------- | ---------------- |
| InternalServerException     | 500         | application/json |
| OpenRouterException         | 4XX, 5XX    | \*/\*            |
