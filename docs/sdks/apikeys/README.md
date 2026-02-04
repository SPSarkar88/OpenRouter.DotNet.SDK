# APIKeys

## Overview

API key management endpoints

### Available Operations

* [ListAsync](#listasync) - List API keys
* [CreateAsync](#createasync) - Create a new API key
* [UpdateAsync](#updateasync) - Update an API key
* [DeleteAsync](#deleteasync) - Delete an API key
* [GetAsync](#getasync) - Get a single API key
* [GetCurrentKeyMetadataAsync](#getcurrentkeymetadataasync) - Get current API key

## ListAsync

List all API keys for the authenticated user. [Provisioning key](/docs/guides/overview/auth/provisioning-api-keys) required.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_PROVISIONING_API_KEY");

// List all API keys
var result = await client.ApiKeys.ListAsync();

foreach (var apiKey in result.Data)
{
    Console.WriteLine($"Key: {apiKey.Name}, Hash: {apiKey.Hash}");
}

// List with pagination and include disabled keys
var pagedResult = await client.ApiKeys.ListAsync(
    includeDisabled: true,
    offset: 0
);
```

### Standalone Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.SDK;

var services = new ServiceCollection();
services.AddOpenRouter(options =>
{
    options.ApiKey = "YOUR_PROVISIONING_API_KEY";
});

var serviceProvider = services.BuildServiceProvider();
var apiKeysService = serviceProvider.GetRequiredService<IApiKeysService>();

var result = await apiKeysService.ListAsync();
```

### Parameters

| Parameter           | Type              | Required           | Description                                        |
| ------------------- | ----------------- | ------------------ | -------------------------------------------------- |
| `includeDisabled`   | bool?             | :heavy_minus_sign: | Whether to include disabled API keys in the list   |
| `offset`            | int?              | :heavy_minus_sign: | Pagination offset for the list                     |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation               |

### Response

**Task\<ListApiKeysResponse\>**

### Errors

| Error Type                    | Status Code | Content Type     |
| ----------------------------- | ----------- | ---------------- |
| UnauthorizedException         | 401         | application/json |
| TooManyRequestsException      | 429         | application/json |
| InternalServerException       | 500         | application/json |
| OpenRouterException           | 4XX, 5XX    | \*/\*            |

## CreateAsync

Create a new API key for the authenticated user. [Provisioning key](/docs/guides/overview/auth/provisioning-api-keys) required.

### Example Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_PROVISIONING_API_KEY");

var request = new CreateApiKeyRequest
{
    Name = "My New API Key",
    Limit = 10.00m, // $10 limit
    LimitReset = "monthly"
};

var result = await client.ApiKeys.CreateAsync(request);

// IMPORTANT: The actual API key is only shown once
Console.WriteLine($"New API Key: {result.Key}");
Console.WriteLine($"Key Hash: {result.Hash}");
Console.WriteLine($"Save this key now - it won't be shown again!");
```

### Standalone Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var services = new ServiceCollection();
services.AddOpenRouter(options =>
{
    options.ApiKey = "YOUR_PROVISIONING_API_KEY";
});

var serviceProvider = services.BuildServiceProvider();
var apiKeysService = serviceProvider.GetRequiredService<IApiKeysService>();

var request = new CreateApiKeyRequest { Name = "My New API Key" };
var result = await apiKeysService.CreateAsync(request);
```

### Parameters

| Parameter           | Type                  | Required           | Description                          |
| ------------------- | --------------------- | ------------------ | ------------------------------------ |
| `request`           | CreateApiKeyRequest   | :heavy_check_mark: | API key configuration                |
| `cancellationToken` | CancellationToken     | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<CreateApiKeyResponse\>**

### Errors

| Error Type                    | Status Code | Content Type     |
| ----------------------------- | ----------- | ---------------- |
| UnauthorizedException         | 401         | application/json |
| TooManyRequestsException      | 429         | application/json |
| InternalServerException       | 500         | application/json |
| OpenRouterException           | 4XX, 5XX    | \*/\*            |

## UpdateAsync

Update an existing API key. [Provisioning key](/docs/guides/overview/auth/provisioning-api-keys) required.

### Example Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_PROVISIONING_API_KEY");

var request = new UpdateApiKeyRequest
{
    Hash = "key_hash_here",
    Name = "Updated Key Name",
    Limit = 25.00m,
    Disabled = false
};

var result = await client.ApiKeys.UpdateAsync(request);

Console.WriteLine($"Updated: {result.Name}");
```

### Standalone Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var services = new ServiceCollection();
services.AddOpenRouter(options =>
{
    options.ApiKey = "YOUR_PROVISIONING_API_KEY";
});

var serviceProvider = services.BuildServiceProvider();
var apiKeysService = serviceProvider.GetRequiredService<IApiKeysService>();

var request = new UpdateApiKeyRequest { Hash = "key_hash", Name = "New Name" };
var result = await apiKeysService.UpdateAsync(request);
```

### Parameters

| Parameter           | Type                  | Required           | Description                          |
| ------------------- | --------------------- | ------------------ | ------------------------------------ |
| `request`           | UpdateApiKeyRequest   | :heavy_check_mark: | Update request with hash and fields  |
| `cancellationToken` | CancellationToken     | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<UpdateApiKeyResponse\>**

### Errors

| Error Type                    | Status Code | Content Type     |
| ----------------------------- | ----------- | ---------------- |
| UnauthorizedException         | 401         | application/json |
| TooManyRequestsException      | 429         | application/json |
| InternalServerException       | 500         | application/json |
| OpenRouterException           | 4XX, 5XX    | \*/\*            |

## DeleteAsync

Delete an existing API key. [Provisioning key](/docs/guides/overview/auth/provisioning-api-keys) required.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_PROVISIONING_API_KEY");

var result = await client.ApiKeys.DeleteAsync("key_hash_here");

Console.WriteLine($"Key deleted successfully");
```

### Standalone Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.SDK;

var services = new ServiceCollection();
services.AddOpenRouter(options =>
{
    options.ApiKey = "YOUR_PROVISIONING_API_KEY";
});

var serviceProvider = services.BuildServiceProvider();
var apiKeysService = serviceProvider.GetRequiredService<IApiKeysService>();

var result = await apiKeysService.DeleteAsync("key_hash");
```

### Parameters

| Parameter           | Type              | Required           | Description                              |
| ------------------- | ----------------- | ------------------ | ---------------------------------------- |
| `hash`              | string            | :heavy_check_mark: | The hash identifier of the API key       |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation     |

### Response

**Task\<DeleteApiKeyResponse\>**

### Errors

| Error Type                    | Status Code | Content Type     |
| ----------------------------- | ----------- | ---------------- |
| UnauthorizedException         | 401         | application/json |
| TooManyRequestsException      | 429         | application/json |
| InternalServerException       | 500         | application/json |
| OpenRouterException           | 4XX, 5XX    | \*/\*            |

## GetAsync

Get a single API key by hash. [Provisioning key](/docs/guides/overview/auth/provisioning-api-keys) required.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_PROVISIONING_API_KEY");

var result = await client.ApiKeys.GetAsync("key_hash_here");

Console.WriteLine($"Key Name: {result.Name}");
Console.WriteLine($"Limit: ${result.Limit}");
Console.WriteLine($"Usage: ${result.Usage}");
```

### Standalone Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.SDK;

var services = new ServiceCollection();
services.AddOpenRouter(options =>
{
    options.ApiKey = "YOUR_PROVISIONING_API_KEY";
});

var serviceProvider = services.BuildServiceProvider();
var apiKeysService = serviceProvider.GetRequiredService<IApiKeysService>();

var result = await apiKeysService.GetAsync("key_hash");
```

### Parameters

| Parameter           | Type              | Required           | Description                          |
| ------------------- | ----------------- | ------------------ | ------------------------------------ |
| `hash`              | string            | :heavy_check_mark: | The hash identifier of the API key   |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<GetApiKeyResponse\>**

### Errors

| Error Type                    | Status Code | Content Type     |
| ----------------------------- | ----------- | ---------------- |
| UnauthorizedException         | 401         | application/json |
| TooManyRequestsException      | 429         | application/json |
| InternalServerException       | 500         | application/json |
| OpenRouterException           | 4XX, 5XX    | \*/\*            |

## GetCurrentKeyMetadataAsync

Get information on the API key associated with the current authentication session.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

var result = await client.ApiKeys.GetCurrentKeyMetadataAsync();

Console.WriteLine($"Current Key: {result.Name}");
Console.WriteLine($"Rate Limit: {result.RateLimit.Limit} requests per {result.RateLimit.Interval}");
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
var apiKeysService = serviceProvider.GetRequiredService<IApiKeysService>();

var result = await apiKeysService.GetCurrentKeyMetadataAsync();
```

### Parameters

| Parameter           | Type              | Required           | Description                          |
| ------------------- | ----------------- | ------------------ | ------------------------------------ |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<GetCurrentApiKeyResponse\>**

### Errors

| Error Type                    | Status Code | Content Type     |
| ----------------------------- | ----------- | ---------------- |
| UnauthorizedException         | 401         | application/json |
| TooManyRequestsException      | 429         | application/json |
| InternalServerException       | 500         | application/json |
| OpenRouterException           | 4XX, 5XX    | \*/\*            |
