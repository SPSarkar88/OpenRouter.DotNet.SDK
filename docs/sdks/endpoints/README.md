# Endpoints

## Overview

Model endpoint information

### Available Operations

* [ListAsync](#listasync) - List endpoints for a specific model
* [ListZdrEndpointsAsync](#listzrdendpointsasync) - List ZDR endpoints

## ListAsync

Lists the endpoints for a specific model, identified by author and slug.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var result = await client.Endpoints.ListAsync(
    author: "openai",
    slug: "gpt-4"
);

Console.WriteLine($"Model: {result.Model}");
foreach (var endpoint in result.Endpoints)
{
    Console.WriteLine($"Endpoint: {endpoint.Url}");
    Console.WriteLine($"  Provider: {endpoint.Provider}");
    Console.WriteLine($"  Status: {endpoint.Status}");
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
var endpointsService = serviceProvider.GetRequiredService<IEndpointsService>();

var result = await endpointsService.ListAsync("openai", "gpt-4");
```

### Parameters

| Parameter           | Type              | Required           | Description                          |
| ------------------- | ----------------- | ------------------ | ------------------------------------ |
| `author`            | string            | :heavy_check_mark: | The author of the model              |
| `slug`              | string            | :heavy_check_mark: | The slug identifier of the model     |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<ModelEndpointsResponse\>**

### Errors

| Error Type                  | Status Code | Content Type     |
| --------------------------- | ----------- | ---------------- |
| BadRequestException         | 400         | application/json |
| NotFoundException           | 404         | application/json |
| InternalServerException     | 500         | application/json |
| OpenRouterException         | 4XX, 5XX    | \*/\*            |

## ListZdrEndpointsAsync

Lists the ZDR (Zero Data Retention) endpoints.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var result = await client.Endpoints.ListZdrEndpointsAsync();

Console.WriteLine($"ZDR Endpoints: {result.Endpoints.Count}");

foreach (var endpoint in result.Endpoints)
{
    Console.WriteLine($"Endpoint: {endpoint.Url}");
    Console.WriteLine($"  Provider: {endpoint.Provider}");
    Console.WriteLine($"  Models: {string.Join(", ", endpoint.SupportedModels)}");
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
var endpointsService = serviceProvider.GetRequiredService<IEndpointsService>();

var result = await endpointsService.ListZdrEndpointsAsync();
```

### Parameters

| Parameter           | Type              | Required           | Description                          |
| ------------------- | ----------------- | ------------------ | ------------------------------------ |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<ZdrEndpointsResponse\>**

### Errors

| Error Type                  | Status Code | Content Type     |
| --------------------------- | ----------- | ---------------- |
| InternalServerException     | 500         | application/json |
| OpenRouterException         | 4XX, 5XX    | \*/\*            |
