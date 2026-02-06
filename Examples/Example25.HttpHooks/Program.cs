using OpenRouter.SDK;
using OpenRouter.SDK.Models;
using OpenRouter.Examples.EnvConfig;
using System.Text;
using System.Diagnostics;

namespace Example25.HttpHooks;

/// <summary>
/// Example demonstrating HTTP request/response hooks for logging and monitoring.
/// Shows both simple event-based hooks and advanced IHttpClientService access.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== OpenRouter SDK - HTTP Hooks Example ===\n");

        var apiKey = ExampleConfig.ApiKey;
        var modelName = ExampleConfig.ModelName;

        // Run all examples
        await Example1_SimpleEventHooks(apiKey, modelName);
        Console.WriteLine("\n" + new string('-', 80) + "\n");

        await Example2_DetailedRequestLogging(apiKey, modelName);
        Console.WriteLine("\n" + new string('-', 80) + "\n");

        await Example3_ErrorHandling(apiKey, modelName);
        Console.WriteLine("\n" + new string('-', 80) + "\n");

        await Example4_PerformanceMetrics(apiKey, modelName);
        Console.WriteLine("\n" + new string('-', 80) + "\n");

        await Example5_AdvancedHooksWithHttpClient(apiKey, modelName);
        Console.WriteLine("\n" + new string('-', 80) + "\n");

        await Example6_ConditionalLogging(apiKey, modelName);

        Console.WriteLine("\n=== All Examples Completed ===");
    }

    /// <summary>
    /// Example 1: Simple event-based hooks (matches README example)
    /// </summary>
    static async Task Example1_SimpleEventHooks(string apiKey, string modelName)
    {
        Console.WriteLine("Example 1: Simple Event Hooks");
        Console.WriteLine("Demonstrates basic BeforeRequest and AfterResponse events\n");

        var client = new OpenRouterClient(apiKey);

        // Log all requests
        client.BeforeRequest += async (request) =>
        {
            Console.WriteLine($"📤 Sending request to: {request.RequestUri}");
            Console.WriteLine($"   Method: {request.Method}");
            await Task.CompletedTask;
        };

        // Log all responses
        client.AfterResponse += async (response) =>
        {
            Console.WriteLine($"📥 Received response: {response.StatusCode}");
            Console.WriteLine($"   Content Type: {response.Content.Headers.ContentType}");
            await Task.CompletedTask;
        };

        // Make a simple request
        var result = await client.CallModelAsync(
            model: "openai/gpt-3.5-turbo",
            userMessage: "Say 'Hello from hooks!' in one sentence."
        );

        Console.WriteLine($"\n✅ Response: {result}");
    }

    /// <summary>
    /// Example 2: Detailed request logging
    /// </summary>
    static async Task Example2_DetailedRequestLogging(string apiKey, string modelName)
    {
        Console.WriteLine("Example 2: Detailed Request Logging");
        Console.WriteLine("Shows how to log complete request details\n");

        var client = new OpenRouterClient(apiKey);

        client.BeforeRequest += async (request) =>
        {
            Console.WriteLine($"📋 REQUEST DETAILS:");
            Console.WriteLine($"   URL: {request.RequestUri}");
            Console.WriteLine($"   Method: {request.Method}");
            Console.WriteLine($"   Headers:");

            foreach (var header in request.Headers)
            {
                Console.WriteLine($"     {header.Key}: {string.Join(", ", header.Value)}");
            }

            if (request.Content != null)
            {
                var content = await request.Content.ReadAsStringAsync();
                Console.WriteLine($"   Body Preview: {content.Substring(0, Math.Min(100, content.Length))}...");
            }

            Console.WriteLine();
            await Task.CompletedTask;
        };

        client.AfterResponse += async (response) =>
        {
            Console.WriteLine($"📋 RESPONSE DETAILS:");
            Console.WriteLine($"   Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"   Headers:");

            foreach (var header in response.Headers)
            {
                Console.WriteLine($"     {header.Key}: {string.Join(", ", header.Value)}");
            }

            Console.WriteLine();
            await Task.CompletedTask;
        };

        var result = await client.Models.GetCountAsync();
        Console.WriteLine($"✅ Total models available: {result.Count}");
    }

    /// <summary>
    /// Example 3: Error handling with OnError hook
    /// </summary>
    static async Task Example3_ErrorHandling(string apiKey, string modelName)
    {
        Console.WriteLine("Example 3: Error Handling");
        Console.WriteLine("Demonstrates error hook for logging failures\n");

        var client = new OpenRouterClient(apiKey);

        client.OnError += async (exception, request) =>
        {
            Console.WriteLine($"❌ ERROR OCCURRED:");
            Console.WriteLine($"   Request: {request.Method} {request.RequestUri}");
            Console.WriteLine($"   Exception: {exception.GetType().Name}");
            Console.WriteLine($"   Message: {exception.Message}");
            await Task.CompletedTask;
        };

        try
        {
            // This should work fine
            var result = await client.CallModelAsync(
                model: "openai/gpt-3.5-turbo",
                userMessage: "Hello!"
            );
            Console.WriteLine($"✅ Success: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Caught exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Example 4: Performance metrics tracking
    /// </summary>
    static async Task Example4_PerformanceMetrics(string apiKey, string modelName)
    {
        Console.WriteLine("Example 4: Performance Metrics");
        Console.WriteLine("Track request timing and performance\n");

        var client = new OpenRouterClient(apiKey);
        var stopwatch = new Stopwatch();

        client.BeforeRequest += async (request) =>
        {
            stopwatch.Restart();
            var uri = request.RequestUri?.IsAbsoluteUri == true 
                ? request.RequestUri.PathAndQuery 
                : request.RequestUri?.ToString();
            Console.WriteLine($"⏱️  Request started: {request.Method} {uri}");
            await Task.CompletedTask;
        };

        client.AfterResponse += async (response) =>
        {
            stopwatch.Stop();
            Console.WriteLine($"⏱️  Request completed in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"   Status: {response.StatusCode}");

            // Check for rate limit headers
            if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remaining))
            {
                Console.WriteLine($"   Rate Limit Remaining: {string.Join(", ", remaining)}");
            }

            await Task.CompletedTask;
        };

        var models = await client.Models.GetModelsAsync();
        Console.WriteLine($"\n✅ Retrieved {models.Data.Count} models");
    }

    /// <summary>
    /// Example 5: Advanced hooks using IHttpClientService directly
    /// </summary>
    static async Task Example5_AdvancedHooksWithHttpClient(string apiKey, string modelName)
    {
        Console.WriteLine("Example 5: Advanced Hooks via IHttpClientService");
        Console.WriteLine("Direct access to HTTP client for advanced scenarios\n");

        var client = new OpenRouterClient(apiKey);

        // Access the HTTP client service directly for advanced hooks
        if (client.HttpClient != null)
        {
            Console.WriteLine("✅ HTTP Client Service is accessible");

            // Add a custom header modifier hook
            client.HttpClient.AddBeforeRequestHook(async (request) =>
            {
                Console.WriteLine($"🔧 Advanced hook: Adding custom tracking header");
                request.Headers.Add("X-Custom-Tracking-Id", Guid.NewGuid().ToString());
                return request; // Can modify and return the request
            });

            // Add response validation hook
            client.HttpClient.AddResponseHook(async (response, request) =>
            {
                Console.WriteLine($"🔍 Advanced hook: Validating response");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"   ⚠️  Non-success status: {response.StatusCode}");
                }
                else
                {
                    Console.WriteLine($"   ✅ Success status: {response.StatusCode}");
                }
                await Task.CompletedTask;
            });

            // Add error logging hook
            client.HttpClient.AddErrorHook(async (exception, request) =>
            {
                Console.WriteLine($"🚨 Advanced error hook triggered");
                Console.WriteLine($"   Exception Type: {exception.GetType().Name}");
                Console.WriteLine($"   Failed Request: {request.RequestUri}");
                // Could log to external service here
                await Task.CompletedTask;
            });
        }

        var result = await client.CallModelAsync(
            model: "openai/gpt-3.5-turbo",
            userMessage: "What is 2+2?"
        );

        Console.WriteLine($"\n✅ Response: {result}");
    }

    /// <summary>
    /// Example 6: Conditional logging based on request type
    /// </summary>
    static async Task Example6_ConditionalLogging(string apiKey, string modelName)
    {
        Console.WriteLine("Example 6: Conditional Logging");
        Console.WriteLine("Log only specific types of requests\n");

        var client = new OpenRouterClient(apiKey);

        client.BeforeRequest += async (request) =>
        {
            var uriString = request.RequestUri?.IsAbsoluteUri == true 
                ? request.RequestUri.PathAndQuery 
                : request.RequestUri?.ToString();
            
            // Only log chat completion requests
            if (uriString?.Contains("/chat/completions") == true)
            {
                Console.WriteLine($"💬 Chat request: {request.Method} {uriString}");
            }
            // Only log model listing requests
            else if (uriString?.Contains("/models") == true)
            {
                Console.WriteLine($"📚 Models request: {request.Method} {uriString}");
            }
            await Task.CompletedTask;
        };

        client.AfterResponse += async (response) =>
        {
            // Log slow requests
            if (response.Headers.TryGetValues("X-Response-Time", out var times))
            {
                Console.WriteLine($"⚡ Response time: {string.Join(", ", times)}");
            }
            await Task.CompletedTask;
        };

        // Make different types of requests
        Console.WriteLine("Making chat request...");
        await client.CallModelAsync(
            model: "openai/gpt-3.5-turbo",
            userMessage: "Hi!"
        );

        Console.WriteLine("\nMaking models list request...");
        await client.Models.GetCountAsync();

        Console.WriteLine("\n✅ Conditional logging example complete");
    }

}
