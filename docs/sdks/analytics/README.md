# Analytics

## Overview

Analytics and usage endpoints

### Available Operations

* [GetUserActivityAsync](#getuseractivityasync) - Get user activity grouped by endpoint

## GetUserActivityAsync

Returns user activity data grouped by endpoint for the last 30 (completed) UTC days. [Provisioning key](/docs/guides/overview/auth/provisioning-api-keys) required.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

// Get all activity for the last 30 days
var result = await client.Analytics.GetUserActivityAsync();

Console.WriteLine($"Total requests: {result.Data.Count}");
foreach (var activity in result.Data)
{
    Console.WriteLine($"Endpoint: {activity.Endpoint}, Requests: {activity.Requests}");
}

// Get activity for a specific date
var dateResult = await client.Analytics.GetUserActivityAsync(date: "2024-01-15");
Console.WriteLine($"Activity for 2024-01-15: {dateResult.Data.Count} endpoints");
```

### Standalone Usage

Using dependency injection:

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.SDK;

var services = new ServiceCollection();
services.AddOpenRouter(options =>
{
    options.ApiKey = "YOUR_API_KEY";
});

var serviceProvider = services.BuildServiceProvider();
var analyticsService = serviceProvider.GetRequiredService<IAnalyticsService>();

var result = await analyticsService.GetUserActivityAsync();
```

### Parameters

| Parameter          | Type                  | Required            | Description                                                                     |
| ------------------ | --------------------- | ------------------- | ------------------------------------------------------------------------------- |
| `date`             | string                | :heavy_minus_sign:  | Optional filter by a single UTC date in the last 30 days (YYYY-MM-DD format)   |
| `cancellationToken`| CancellationToken     | :heavy_minus_sign:  | Cancellation token for the operation                                            |

### Response

**Task\<GetUserActivityResponse\>**

### Errors

| Error Type                          | Status Code     | Content Type       |
| ----------------------------------- | --------------- | ------------------ |
| BadRequestException                 | 400             | application/json   |
| UnauthorizedException               | 401             | application/json   |
| ForbiddenException                  | 403             | application/json   |
| InternalServerException             | 500             | application/json   |
| OpenRouterException                 | 4XX, 5XX        | \*/\*              |
