# OAuth

## Overview

OAuth2 PKCE authentication endpoints for secure user authentication flows

### Available Operations

* [CreateSHA256CodeChallenge](#createsha256codechallenge) - Generate a code challenge for PKCE
* [CreateAuthorizationUrl](#createauthorizationurl) - Generate authorization URL
* [CreateAuthCodeAsync](#createauthcodeasync) - Create authorization code
* [ExchangeAuthCodeForAPIKeyAsync](#exchangeauthcodeforapikeyasync) - Exchange authorization code for API key

## CreateSHA256CodeChallenge

Generate a SHA-256 code challenge for PKCE (Proof Key for Code Exchange) flow. This is used to securely authenticate users without exposing secrets.

### Example Usage

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

// Generate with automatic code verifier
var result = client.OAuth.CreateSHA256CodeChallenge();

Console.WriteLine($"Code Verifier: {result.CodeVerifier}");
Console.WriteLine($"Code Challenge: {result.CodeChallenge}");

// Store the code verifier securely - you'll need it later
```

### With Custom Code Verifier

```csharp
using OpenRouter.SDK;

var client = new OpenRouterClient("YOUR_API_KEY");

// Use your own code verifier (43-128 chars, [A-Za-z0-9-._~] only)
var customVerifier = "my-custom-verifier-string-here-must-be-43-to-128-chars";
var result = client.OAuth.CreateSHA256CodeChallenge(customVerifier);

Console.WriteLine($"Code Challenge: {result.CodeChallenge}");
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
var oauthService = serviceProvider.GetRequiredService<IOAuthService>();

var result = oauthService.CreateSHA256CodeChallenge();
```

### Parameters

| Parameter      | Type   | Required           | Description                                                                           |
| -------------- | ------ | ------------------ | ------------------------------------------------------------------------------------- |
| `codeVerifier` | string | :heavy_minus_sign: | Optional code verifier (43-128 characters). If not provided, one will be generated.   |

### Response

**CodeChallengeResult**

## CreateAuthorizationUrl

Generate an OAuth2 authorization URL for user authentication. Users should be redirected to this URL to authorize your application.

### Example Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

// First generate code challenge
var challenge = client.OAuth.CreateSHA256CodeChallenge();

// Create authorization URL
var request = new CreateAuthorizationUrlRequest
{
    CallbackUrl = "https://myapp.com/oauth/callback",
    CodeChallenge = challenge.CodeChallenge
};

var authUrl = client.OAuth.CreateAuthorizationUrl(request);

Console.WriteLine($"Redirect user to: {authUrl}");
Console.WriteLine($"Remember to save code verifier: {challenge.CodeVerifier}");

// Redirect user to authUrl in your application
```

### With Custom Base URL

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var challenge = client.OAuth.CreateSHA256CodeChallenge();

var request = new CreateAuthorizationUrlRequest
{
    CallbackUrl = "https://myapp.com/oauth/callback",
    CodeChallenge = challenge.CodeChallenge
};

var authUrl = client.OAuth.CreateAuthorizationUrl(
    request,
    baseUrl: "https://custom.openrouter.ai"
);

Console.WriteLine($"Authorization URL: {authUrl}");
```

### Parameters

| Parameter     | Type                            | Required           | Description                                      |
| ------------- | ------------------------------- | ------------------ | ------------------------------------------------ |
| `request`     | CreateAuthorizationUrlRequest   | :heavy_check_mark: | Authorization URL parameters                     |
| `baseUrl`     | string                          | :heavy_minus_sign: | Optional base URL (defaults to https://openrouter.ai) |

### Response

**string** - The complete authorization URL

## CreateAuthCodeAsync

Create an authorization code for the PKCE flow to generate a user-controlled API key.

### Example Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new CreateAuthCodeRequest
{
    CallbackUrl = "https://myapp.com/oauth/callback",
    CodeChallenge = "your_code_challenge_here",
    CodeChallengeMethod = "S256"
};

var result = await client.OAuth.CreateAuthCodeAsync(request);

Console.WriteLine($"Auth Code: {result.Code}");
Console.WriteLine($"Expires At: {result.ExpiresAt}");
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
var oauthService = serviceProvider.GetRequiredService<IOAuthService>();

var request = new CreateAuthCodeRequest
{
    CallbackUrl = "https://myapp.com/oauth/callback",
    CodeChallenge = "challenge"
};

var result = await oauthService.CreateAuthCodeAsync(request);
```

### Parameters

| Parameter           | Type                   | Required           | Description                          |
| ------------------- | ---------------------- | ------------------ | ------------------------------------ |
| `request`           | CreateAuthCodeRequest  | :heavy_check_mark: | Authorization code request           |
| `cancellationToken` | CancellationToken      | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<CreateAuthCodeResponse\>**

### Errors

| Error Type                  | Status Code | Content Type     |
| --------------------------- | ----------- | ---------------- |
| BadRequestException         | 400         | application/json |
| ForbiddenException          | 403         | application/json |
| InternalServerException     | 500         | application/json |
| OpenRouterException         | 4XX, 5XX    | \*/\*            |

## ExchangeAuthCodeForAPIKeyAsync

Exchange an authorization code from the PKCE flow for a user-controlled API key.

### Example Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new ExchangeAuthCodeRequest
{
    Code = "auth_code_abc123def456",
    CodeVerifier = "your_saved_code_verifier_here"
};

var result = await client.OAuth.ExchangeAuthCodeForAPIKeyAsync(request);

Console.WriteLine($"API Key: {result.ApiKey}");
Console.WriteLine($"User ID: {result.User.Id}");
Console.WriteLine($"User Email: {result.User.Email}");

// Save the API key - it won't be shown again!
```

### Complete OAuth Flow

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;

var client = new OpenRouterClient("YOUR_API_KEY");

// Step 1: Generate code challenge
var challenge = client.OAuth.CreateSHA256CodeChallenge();
var codeVerifier = challenge.CodeVerifier; // Save this!

// Step 2: Create authorization URL
var authRequest = new CreateAuthorizationUrlRequest
{
    CallbackUrl = "https://myapp.com/oauth/callback",
    CodeChallenge = challenge.CodeChallenge
};

var authUrl = client.OAuth.CreateAuthorizationUrl(authRequest);
Console.WriteLine($"Redirect user to: {authUrl}");

// Step 3: After user authorizes and you receive the code in your callback...
var authCode = "received_from_callback"; // Get from callback URL parameter

// Step 4: Exchange code for API key
var exchangeRequest = new ExchangeAuthCodeRequest
{
    Code = authCode,
    CodeVerifier = codeVerifier // Use saved verifier
};

var result = await client.OAuth.ExchangeAuthCodeForAPIKeyAsync(exchangeRequest);
Console.WriteLine($"New API Key: {result.ApiKey}");
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
var oauthService = serviceProvider.GetRequiredService<IOAuthService>();

var request = new ExchangeAuthCodeRequest
{
    Code = "auth_code",
    CodeVerifier = "verifier"
};

var result = await oauthService.ExchangeAuthCodeForAPIKeyAsync(request);
```

### Parameters

| Parameter           | Type                     | Required           | Description                          |
| ------------------- | ------------------------ | ------------------ | ------------------------------------ |
| `request`           | ExchangeAuthCodeRequest  | :heavy_check_mark: | Exchange request with code           |
| `cancellationToken` | CancellationToken        | :heavy_minus_sign: | Cancellation token for the operation |

### Response

**Task\<ExchangeAuthCodeResponse\>**

### Errors

| Error Type                  | Status Code | Content Type     |
| --------------------------- | ----------- | ---------------- |
| BadRequestException         | 400         | application/json |
| ForbiddenException          | 403         | application/json |
| InternalServerException     | 500         | application/json |
| OpenRouterException         | 4XX, 5XX    | \*/\*            |
