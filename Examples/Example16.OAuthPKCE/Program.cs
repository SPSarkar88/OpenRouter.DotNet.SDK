using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using OpenRouter.SDK.Models;

Console.WriteLine("===========================================");
Console.WriteLine("Example 16: OAuth PKCE Authentication");
Console.WriteLine("===========================================\n");

await Example16.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example16
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 16: OAuth Service - PKCE Authentication Flow ===\n");

        try
        {
            // Step 1: Generate code challenge for PKCE
            Console.WriteLine("Step 1: Generate PKCE code challenge");
            var pkceChallenge = client.OAuth.CreateSHA256CodeChallenge();
            Console.WriteLine($"Code Verifier (store securely): {pkceChallenge.CodeVerifier}");
            Console.WriteLine($"Code Challenge: {pkceChallenge.CodeChallenge}");
            
            // Step 2: Create authorization URL
            Console.WriteLine("\nStep 2: Create authorization URL");
            var authUrlRequest = new CreateAuthorizationUrlRequest
            {
                CallbackUrl = "https://your-app.com/callback",
                CodeChallenge = pkceChallenge.CodeChallenge,
                CodeChallengeMethod = CodeChallengeMethod.S256,
                Limit = 10.0 // Optional: Credit limit for the API key
            };
            
            var authorizationUrl = client.OAuth.CreateAuthorizationUrl(authUrlRequest);
            Console.WriteLine($"Authorization URL: {authorizationUrl}");
            Console.WriteLine("Redirect user to this URL for authentication");
            
            // Step 3: After user authorizes and you receive the auth code...
            Console.WriteLine("\nStep 3: Exchange authorization code for API key");
            Console.WriteLine("(This would happen after user authorizes and redirects back)");
            
            // Example exchange request (would use actual code from redirect)
            var exchangeRequest = new ExchangeAuthCodeRequest
            {
                Code = "auth-code-from-redirect",
                CodeVerifier = pkceChallenge.CodeVerifier,
                CodeChallengeMethod = CodeChallengeMethod.S256
            };
            
            // var apiKeyResponse = await client.OAuth.ExchangeAuthCodeForAPIKeyAsync(exchangeRequest);
            // Console.WriteLine($"API Key: {apiKeyResponse.Key}");
            // Console.WriteLine($"User ID: {apiKeyResponse.UserId}");
            
            Console.WriteLine("Note: Complete flow requires actual OAuth redirect handling");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OAuth example (demonstration only): {ex.Message}");
        }
    }
}


