# Credits

## Overview

Credit management endpoints for checking balance and adding credits

### Available Operations

* [GetCreditsAsync](#getcreditsasync) - Get remaining credits
* [CreateCoinbaseChargeAsync](#createcoinbasechargeasync) - Create a Coinbase charge for crypto payment

## GetCreditsAsync

Get total credits purchased and used for the authenticated user. [Provisioning key](/docs/guides/overview/auth/provisioning-api-keys) required.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_PROVISIONING_API_KEY");

var result = await client.Credits.GetCreditsAsync();

Console.WriteLine($"Total Credits: ${result.TotalCredits}");
Console.WriteLine($"Total Usage: ${result.TotalUsage}");
Console.WriteLine($"Balance: ${result.Balance}");
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
var creditsService = serviceProvider.GetRequiredService<ICreditsService>();

var result = await creditsService.GetCreditsAsync();
```

### Parameters

| Parameter           | Type              | Required           | Description                          |
| ------------------- | ----------------- | ------------------ | ------------------------------------ |
| `cancellationToken` | CancellationToken | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<GetCreditsResponse\>**

### Errors

| Error Type                  | Status Code | Content Type     |
| --------------------------- | ----------- | ---------------- |
| UnauthorizedException       | 401         | application/json |
| ForbiddenException          | 403         | application/json |
| InternalServerException     | 500         | application/json |
| OpenRouterException         | 4XX, 5XX    | \*/\*            |

## CreateCoinbaseChargeAsync

Create a Coinbase charge for crypto payment to add credits to your account.

### Example Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new CreateCoinbaseChargeRequest
{
    Amount = 100,
    Sender = "0x1234567890123456789012345678901234567890"
};

var result = await client.Credits.CreateCoinbaseChargeAsync(request);

Console.WriteLine($"Payment URL: {result.HostedUrl}");
Console.WriteLine($"Charge ID: {result.Id}");
Console.WriteLine($"Amount: ${result.Pricing.Local.Amount} {result.Pricing.Local.Currency}");
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
var creditsService = serviceProvider.GetRequiredService<ICreditsService>();

var request = new CreateCoinbaseChargeRequest
{
    Amount = 50,
    Sender = "0x..."
};

var result = await creditsService.CreateCoinbaseChargeAsync(request);
```

### Parameters

| Parameter           | Type                          | Required           | Description                          |
| ------------------- | ----------------------------- | ------------------ | ------------------------------------ |
| `request`           | CreateCoinbaseChargeRequest   | :heavy_check_mark: | Charge creation request              |
| `cancellationToken` | CancellationToken             | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<CoinbaseChargeResponse\>**

### Errors

| Error Type                  | Status Code | Content Type     |
| --------------------------- | ----------- | ---------------- |
| BadRequestException         | 400         | application/json |
| UnauthorizedException       | 401         | application/json |
| InternalServerException     | 500         | application/json |
| OpenRouterException         | 4XX, 5XX    | \*/\*            |
