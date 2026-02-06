using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using OpenRouter.SDK.Models;

Console.WriteLine("===========================================");
Console.WriteLine("Example 18: API Keys Management");
Console.WriteLine("===========================================\n");

await Example18.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example18
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        Console.WriteLine("=== Example 18: API Keys Service - Manage API Keys ===\n");

        try
        {
            // NOTE: These examples are for demonstration only
            // You need a PROVISIONING KEY to use the API Keys service
            // Regular API keys cannot manage other API keys
            
            Console.WriteLine("API Keys Service allows you to:");
            Console.WriteLine("1. List all API keys for your account");
            Console.WriteLine("2. Create new API keys with spending limits");
            Console.WriteLine("3. Update existing API keys (disable, change limits)");
            Console.WriteLine("4. Delete API keys");
            Console.WriteLine("5. Get detailed usage statistics");
            Console.WriteLine("6. Get current key metadata");
            Console.WriteLine();
            
            // Example: Create a new API key with spending limits
            Console.WriteLine("Creating a new API key with daily spending limit:");
            var createRequest = new CreateApiKeyRequest
            {
                Name = "Test API Key",
                Limit = 10.0, // $10 USD limit
                LimitReset = LimitReset.Daily, // Reset daily at midnight UTC
                IncludeByokInLimit = false // Don't include BYOK usage in limit
            };
            
            // Uncomment if you have a provisioning key:
            // var createResponse = await client.ApiKeys.CreateAsync(createRequest);
            // Console.WriteLine($"Created API Key: {createResponse.Key}");
            // Console.WriteLine($"Hash: {createResponse.Data.Hash}");
            // Console.WriteLine($"Limit: ${createResponse.Data.Limit}");
            Console.WriteLine("(Requires provisioning key - example only)");
            Console.WriteLine();
            
            // Example: List all API keys
            Console.WriteLine("Listing API keys:");
            // var listResponse = await client.ApiKeys.ListAsync(includeDisabled: false);
            Console.WriteLine("(Requires provisioning key - example only)");
            Console.WriteLine();
            
            // Example: Update an API key
            Console.WriteLine("Updating an API key:");
            var updateRequest = new UpdateApiKeyRequest
            {
                Hash = "example-hash-123",
                Name = "Updated Key Name",
                Limit = 25.0, // Increase limit to $25
                Disabled = false
            };
            // var updateResponse = await client.ApiKeys.UpdateAsync(updateRequest);
            Console.WriteLine("(Requires provisioning key - example only)");
            Console.WriteLine();
            
            // Example: Get current key metadata
            Console.WriteLine("Getting current API key metadata:");
            // var currentKey = await client.ApiKeys.GetCurrentKeyMetadataAsync();
            Console.WriteLine("(Works with regular API key)");
            Console.WriteLine();
            
            Console.WriteLine("✓ API Keys Service provides comprehensive key management!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Keys example error: {ex.Message}");
        }
    }
}


