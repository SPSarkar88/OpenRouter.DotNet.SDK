using OpenRouter.SDK;
using OpenRouter.SDK.Models;

// Example 1: Simple text completion
Console.WriteLine("=== Example 1: Simple Text Completion ===\n");

var client = new OpenRouterClient("YOUR_API_KEY_HERE");

var response = await client.CallModelAsync(
    model: "openai/gpt-3.5-turbo",
    userMessage: "What is the capital of France?",
    systemMessage: "You are a helpful assistant."
);

Console.WriteLine($"Response: {response}\n");

// Example 2: Streaming completion
Console.WriteLine("=== Example 2: Streaming Completion ===\n");

Console.Write("Response: ");
await foreach (var chunk in client.CallModelStreamAsync(
    model: "openai/gpt-3.5-turbo",
    userMessage: "Write a short poem about coding."
))
{
    Console.Write(chunk);
}
Console.WriteLine("\n");

// Example 3: Using chat service directly
Console.WriteLine("=== Example 3: Using Chat Service Directly ===\n");

var chatRequest = new ChatCompletionRequest
{
    Model = "openai/gpt-3.5-turbo",
    Messages = new List<Message>
    {
        new SystemMessage { Content = "You are a helpful coding assistant." },
        new UserMessage { Content = "Explain what async/await means in C#." }
    },
    Temperature = 0.7,
    MaxTokens = 500
};

var chatResponse = await client.Chat.CreateAsync(chatRequest);
Console.WriteLine($"Response: {chatResponse.Choices[0].Message.Content}\n");
Console.WriteLine($"Tokens used: {chatResponse.Usage?.TotalTokens ?? 0}\n");

// Example 4: List available models
Console.WriteLine("=== Example 4: List Available Models ===\n");

var modelsResponse = await client.Models.GetModelsAsync();
Console.WriteLine($"Found {modelsResponse.Data.Count} models\n");
Console.WriteLine("First 5 models:");
foreach (var model in modelsResponse.Data.Take(5))
{
    Console.WriteLine($"  - {model.Id} (Context: {model.ContextLength} tokens)");
}
Console.WriteLine();

// Example 5: Advanced chat with custom parameters
Console.WriteLine("=== Example 5: Advanced Chat with Custom Parameters ===\n");

var advancedRequest = new ChatCompletionRequest
{
    Model = "anthropic/claude-3-sonnet",
    Messages = new List<Message>
    {
        new UserMessage { Content = "Give me 3 random numbers between 1 and 100." }
    },
    Temperature = 1.0,
    MaxTokens = 100,
    TopP = 0.9,
    FrequencyPenalty = 0.5
};

var advancedResponse = await client.Chat.CreateAsync(advancedRequest);
Console.WriteLine($"Response: {advancedResponse.Choices[0].Message.Content}\n");

Console.WriteLine("All examples completed!");

// Example 6: Generate Embeddings
Console.WriteLine("=== Example 6: Generate Embeddings ===\n");

var embeddingRequest = new EmbeddingRequest
{
    Input = "OpenRouter provides access to multiple AI models",
    Model = "text-embedding-ada-002"
};

try
{
    var embeddingResponse = await client.Embeddings.GenerateAsync(embeddingRequest);
    Console.WriteLine($"Generated embedding with {embeddingResponse.Data.Count} vector(s)");
    
    if (embeddingResponse.Data.Count > 0 && embeddingResponse.Data[0].Embedding is System.Text.Json.JsonElement element)
    {
        var vectorLength = element.GetArrayLength();
        Console.WriteLine($"Vector dimension: {vectorLength}");
    }
    
    if (embeddingResponse.Usage != null)
    {
        Console.WriteLine($"Tokens used: {embeddingResponse.Usage.TotalTokens}");
        if (embeddingResponse.Usage.Cost.HasValue)
        {
            Console.WriteLine($"Cost: ${embeddingResponse.Usage.Cost:F6}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error generating embeddings: {ex.Message}");
}
Console.WriteLine();

// Example 7: List Providers
Console.WriteLine("=== Example 7: List Providers ===");
try
{
    var providersResponse = await client.Providers.ListAsync();
    
    Console.WriteLine($"Found {providersResponse.Data.Count} providers:");
    foreach (var provider in providersResponse.Data.Take(5))
    {
        Console.WriteLine($"- {provider.Name} ({provider.Slug})");
        if (!string.IsNullOrEmpty(provider.StatusPageUrl))
        {
            Console.WriteLine($"  Status: {provider.StatusPageUrl}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error listing providers: {ex.Message}");
}
Console.WriteLine();

// Example 8: List Endpoints for a Model
Console.WriteLine("=== Example 8: List Endpoints for a Model ===");
try
{
    var endpointsResponse = await client.Endpoints.ListAsync("openai", "gpt-3.5-turbo");
    
    Console.WriteLine($"Model: {endpointsResponse.Name}");
    Console.WriteLine($"Description: {endpointsResponse.Description}");
    Console.WriteLine($"Architecture:");
    Console.WriteLine($"  Tokenizer: {endpointsResponse.Architecture.Tokenizer}");
    Console.WriteLine($"  Modality: {endpointsResponse.Architecture.Modality}");
    Console.WriteLine($"\nAvailable Endpoints: {endpointsResponse.Endpoints.Count}");
    
    foreach (var endpoint in endpointsResponse.Endpoints.Take(3))
    {
        Console.WriteLine($"\n- {endpoint.Name}");
        Console.WriteLine($"  Provider: {endpoint.ProviderName}");
        Console.WriteLine($"  Context Length: {endpoint.ContextLength:N0} tokens");
        Console.WriteLine($"  Pricing:");
        Console.WriteLine($"    Prompt: ${endpoint.Pricing.Prompt}/token");
        Console.WriteLine($"    Completion: ${endpoint.Pricing.Completion}/token");
        
        if (endpoint.UptimeLast30m.HasValue)
        {
            Console.WriteLine($"  Uptime (30m): {endpoint.UptimeLast30m.Value * 100:F2}%");
        }
        
        if (endpoint.LatencyLast30m != null && endpoint.LatencyLast30m.P50.HasValue)
        {
            Console.WriteLine($"  Latency (median): {endpoint.LatencyLast30m.P50.Value:F1}ms");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error listing endpoints: {ex.Message}");
}
Console.WriteLine();

// Example 9: List ZDR Endpoints
Console.WriteLine("=== Example 9: List ZDR Endpoints ===");
try
{
    var zdrResponse = await client.Endpoints.ListZdrEndpointsAsync();
    
    Console.WriteLine($"Found {zdrResponse.Data.Count} ZDR endpoints:");
    foreach (var endpoint in zdrResponse.Data.Take(5))
    {
        Console.WriteLine($"- {endpoint.Name}");
        Console.WriteLine($"  Model: {endpoint.ModelId}");
        Console.WriteLine($"  Provider: {endpoint.ProviderName}");
        Console.WriteLine($"  Supports Caching: {endpoint.SupportsImplicitCaching}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error listing ZDR endpoints: {ex.Message}");
}
Console.WriteLine();

// Example 10: Beta Responses API (Modern Structured API)
Console.WriteLine("=== Example 10: Beta Responses API ===");
try
{
    var betaRequest = new OpenRouter.SDK.Models.BetaResponsesRequest
    {
        Input = "What is the capital of France? Provide a brief explanation.",
        Instructions = "You are a helpful geography teacher. Be concise and educational.",
        Model = "openai/gpt-3.5-turbo",
        MaxOutputTokens = 150,
        Temperature = 0.7
    };

    var betaResponse = await client.Beta.Responses.SendAsync(betaRequest);
    
    Console.WriteLine($"Response ID: {betaResponse.Id}");
    Console.WriteLine($"Model: {betaResponse.Model}");
    Console.WriteLine($"Status: {betaResponse.Status}");
    Console.WriteLine($"\nOutput:");
    
    foreach (var output in betaResponse.Output)
    {
        if (output.Type == "text" && output.Text != null)
        {
            Console.WriteLine(output.Text);
        }
    }
    
    if (betaResponse.Usage != null)
    {
        Console.WriteLine($"\nTokens: {betaResponse.Usage.TotalTokens}");
        if (betaResponse.Usage.Cost.HasValue)
        {
            Console.WriteLine($"Cost: ${betaResponse.Usage.Cost:F6}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error with Beta Responses API: {ex.Message}");
}
Console.WriteLine();

// Example 11: Beta Responses with Tools
Console.WriteLine("=== Example 11: Beta Responses with Tools ===");
try
{
    var weatherTool = new OpenRouter.SDK.Models.ResponsesFunctionTool
    {
        Name = "get_weather",
        Description = "Get the current weather for a location",
        Parameters = new Dictionary<string, object?>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>
            {
                ["location"] = new { type = "string", description = "City name" },
                ["units"] = new { type = "string", @enum = new[] { "celsius", "fahrenheit" }, description = "Temperature units" }
            },
            ["required"] = new[] { "location" }
        }
    };

    var betaRequestWithTools = new OpenRouter.SDK.Models.BetaResponsesRequest
    {
        Input = "What's the weather like in Tokyo?",
        Model = "openai/gpt-3.5-turbo",
        Tools = new List<OpenRouter.SDK.Models.ResponsesFunctionTool> { weatherTool },
        MaxOutputTokens = 150
    };

    var betaResponseWithTools = await client.Beta.Responses.SendAsync(betaRequestWithTools);
    
    Console.WriteLine($"Response with tools:");
    Console.WriteLine($"Model: {betaResponseWithTools.Model}");
    Console.WriteLine($"Status: {betaResponseWithTools.Status}");
    Console.WriteLine($"Tools provided: {betaResponseWithTools.Tools?.Count ?? 0}");
    
    foreach (var output in betaResponseWithTools.Output)
    {
        Console.WriteLine($"Output type: {output.Type}");
        if (output.Text != null)
        {
            Console.WriteLine($"Text: {output.Text}");
        }
        if (output.FunctionCall != null)
        {
            Console.WriteLine($"Function call detected!");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error with Beta Responses tools: {ex.Message}");
}
Console.WriteLine();

// Example 12: Tool System - Simple Weather Tool with Automatic Execution
Console.WriteLine("=== Example 12: Tool System - Automatic Weather Tool Execution ===\n");

try
{
    // Define a weather tool with execute function
    var weatherToolSchema = JsonSchemaBuilder.CreateObjectSchema(
        new Dictionary<string, object>
        {
            ["location"] = JsonSchemaBuilder.String("The city to get weather for"),
            ["units"] = JsonSchemaBuilder.String("Temperature units", new List<string> { "celsius", "fahrenheit" })
        },
        new List<string> { "location" });

    var weatherTool = new Tool<WeatherToolInput, WeatherToolOutput>(
        name: "get_weather",
        description: "Get the current weather for a specific location",
        inputSchema: weatherToolSchema,
        executeFunc: async (input, context) =>
        {
            Console.WriteLine($"  [Tool Executing] Getting weather for {input.Location}...");
            
            // Simulate API call
            await Task.Delay(100);
            
            return new WeatherToolOutput
            {
                Location = input.Location,
                Temperature = 72,
                Condition = "Sunny",
                Units = input.Units ?? "fahrenheit"
            };
        });

    // Use CallModelWithTools for automatic tool execution
    var result = client.CallModelWithTools(
        model: "openai/gpt-4",
        userMessage: "What's the weather like in San Francisco?",
        tools: new[] { weatherTool },
        systemMessage: "You are a helpful weather assistant.");

    // Get just the text (tool will execute automatically)
    var text = await result.GetTextAsync();
    Console.WriteLine($"Final response: {text}");

    // Get tool execution results
    var toolResults = await result.GetToolExecutionResultsAsync();
    Console.WriteLine($"\nTool executions: {toolResults.Count}");
    foreach (var tr in toolResults)
    {
        Console.WriteLine($"  - {tr.ToolName}: {(tr.IsSuccess ? "Success" : "Failed")}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error with tool execution: {ex.Message}");
}
Console.WriteLine();

// Example 13: Tool System - Calculator with Multiple Tools
Console.WriteLine("=== Example 13: Tool System - Multi-Tool Calculator ===\n");

try
{
    // Define calculator tools
    var numberSchema = JsonSchemaBuilder.CreateObjectSchema(
        new Dictionary<string, object>
        {
            ["x"] = JsonSchemaBuilder.Number("First number"),
            ["y"] = JsonSchemaBuilder.Number("Second number")
        },
        new List<string> { "x", "y" });

    var addTool = new Tool<CalcInput, CalcOutput>(
        name: "add",
        description: "Add two numbers",
        inputSchema: numberSchema,
        executeFunc: async (input, _) => new CalcOutput { Result = input.X + input.Y });

    var multiplyTool = new Tool<CalcInput, CalcOutput>(
        name: "multiply",
        description: "Multiply two numbers",
        inputSchema: numberSchema,
        executeFunc: async (input, _) => new CalcOutput { Result = input.X * input.Y });

    var tools = new ITool[] { addTool, multiplyTool };

    var calcResult = client.CallModelWithTools(
        model: "openai/gpt-4",
        userMessage: "What is (5 + 3) * 2?",
        tools: tools,
        systemMessage: "You are a calculator assistant. Use the provided tools to solve math problems.");

    var calcText = await calcResult.GetTextAsync();
    Console.WriteLine($"Calculation result: {calcText}");

    var allResponses = await calcResult.GetAllResponsesAsync();
    Console.WriteLine($"Total conversation turns: {allResponses.Count}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error with calculator: {ex.Message}");
}
Console.WriteLine();

// Example 14: Tool System - Using ModelResult Consumption Patterns
Console.WriteLine("=== Example 14: Tool System - ModelResult Consumption Patterns ===\n");

try
{
    var searchSchema = JsonSchemaBuilder.CreateObjectSchema(
        new Dictionary<string, object>
        {
            ["query"] = JsonSchemaBuilder.String("Search query")
        },
        new List<string> { "query" });

    var searchTool = new Tool<SearchInput, SearchOutput>(
        name: "web_search",
        description: "Search the web for information",
        inputSchema: searchSchema,
        executeFunc: async (input, _) =>
        {
            Console.WriteLine($"  [Searching] Query: {input.Query}");
            await Task.Delay(50);
            return new SearchOutput 
            { 
                Results = new List<string> 
                { 
                    "Result 1: Sample information",
                    "Result 2: More details" 
                } 
            };
        });

    var searchRequest = new BetaResponsesRequest
    {
        Model = "openai/gpt-4",
        Input = new List<ResponsesInputItem>
        {
            new() { Type = "text", Text = "Search for latest AI developments" }
        }
    };

    var modelResult = client.CallModel(searchRequest, new[] { searchTool });

    // Pattern 1: Get just text
    Console.WriteLine("Pattern 1 - GetTextAsync():");
    var justText = await modelResult.GetTextAsync();
    Console.WriteLine($"  {justText}");

    // Pattern 2: Get full response
    Console.WriteLine("\nPattern 2 - GetResponseAsync():");
    var fullResponse = await modelResult.GetResponseAsync();
    Console.WriteLine($"  Model: {fullResponse.Model}");
    Console.WriteLine($"  Status: {fullResponse.Status}");
    Console.WriteLine($"  Output items: {fullResponse.Output.Count}");

    // Pattern 3: Get tool results
    Console.WriteLine("\nPattern 3 - GetToolExecutionResultsAsync():");
    var toolExecResults = await modelResult.GetToolExecutionResultsAsync();
    foreach (var tr in toolExecResults)
    {
        Console.WriteLine($"  Tool: {tr.ToolName}, Success: {tr.IsSuccess}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error with consumption patterns: {ex.Message}");
}
Console.WriteLine();

// Example 15: Generations Service - Get Generation Metadata
Console.WriteLine("=== Example 15: Generations Service - Get Generation Metadata ===\n");

// Note: This requires a valid generation ID from a previous API call
// You can get this from the response headers or tracking your requests
try
{
    var generationId = "your-generation-id-here";
    var generationMetadata = await client.Generations.GetGenerationAsync(generationId);
    
    Console.WriteLine($"Generation ID: {generationMetadata.Data.Id}");
    Console.WriteLine($"Model: {generationMetadata.Data.Model}");
    Console.WriteLine($"Total Cost: ${generationMetadata.Data.TotalCost:F6}");
    Console.WriteLine($"Provider: {generationMetadata.Data.ProviderName ?? "N/A"}");
    Console.WriteLine($"Tokens (Prompt/Completion): {generationMetadata.Data.TokensPrompt}/{generationMetadata.Data.TokensCompletion}");
    Console.WriteLine($"Latency: {generationMetadata.Data.Latency}ms");
    Console.WriteLine($"Created At: {generationMetadata.Data.CreatedAt}");
    
    if (generationMetadata.Data.CacheDiscount.HasValue)
    {
        Console.WriteLine($"Cache Discount: ${generationMetadata.Data.CacheDiscount.Value:F6}");
    }
    
    if (generationMetadata.Data.Streamed.HasValue)
    {
        Console.WriteLine($"Streamed: {generationMetadata.Data.Streamed.Value}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Note: Generation metadata requires a valid generation ID from a previous request.");
    Console.WriteLine($"Error: {ex.Message}");
}
Console.WriteLine();

// Example 16: OAuth Service - PKCE Authentication Flow
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
Console.WriteLine();

// Example 17: Async Parameter Resolution - Context-Aware Dynamic Parameters
Console.WriteLine("=== Example 17: Async Parameter Resolution - Context-Aware Dynamic Parameters ===\n");

try
{
    // Simulate user preferences that would be fetched from a database
    async Task<double> FetchUserTemperaturePreference()
    {
        await Task.Delay(10); // Simulate async database call
        return 0.9; // User prefers creative responses
    }

    // Create a request with dynamic parameters that adapt based on conversation context
    var dynamicRequest = new DynamicBetaResponsesRequest
    {
        // Model switching: start with cheap model, upgrade to expensive after 2 turns
        Model = new DynamicParameter<string>(ctx =>
            ctx.NumberOfTurns > 2 ? "openai/gpt-4-turbo" : "openai/gpt-3.5-turbo"),
        
        // Async temperature: fetch from user preferences
        Temperature = new DynamicParameter<double?>(async ctx =>
        {
            var userPref = await FetchUserTemperaturePreference();
            // Reduce temperature on error for more deterministic responses
            return ctx.HasError ? 0.0 : userPref;
        }),
        
        // Adaptive max tokens: reduce if budget is running low
        MaxOutputTokens = new DynamicParameter<int?>(ctx =>
        {
            var tokensUsed = ctx.TotalTokensUsed ?? 0;
            if (tokensUsed > 5000) return 500;  // Low budget remaining
            if (tokensUsed > 2000) return 1000; // Medium budget
            return 2000; // Full budget available
        }),
        
        // Input message
        Input = new List<ResponsesInputItem>
        {
            new() { Type = "text", Text = "Explain quantum computing in simple terms." }
        }
    };

    // Call the model with dynamic parameters
    var result = client.CallModelDynamic(dynamicRequest);
    var dynamicResponse = await result.GetResponseAsync();
    
    Console.WriteLine($"Model used: {dynamicResponse.Model}");
    Console.WriteLine($"Response: {dynamicResponse.Output?[0].Text}");
    Console.WriteLine($"Tokens used: {dynamicResponse.Usage?.TotalTokens ?? 0}");
    Console.WriteLine();
    
    // Example showing how parameters change over turns in a multi-turn conversation
    Console.WriteLine("Simulating multi-turn conversation with adaptive parameters:");
    Console.WriteLine();
    
    // Turn 1: NumberOfTurns=1, TotalTokensUsed=0
    // Expected: gpt-3.5-turbo, temp=0.9, maxTokens=2000
    Console.WriteLine("Turn 1: cheap model, high creativity, full budget");
    
    // Turn 3: NumberOfTurns=3, TotalTokensUsed=2500
    // Expected: gpt-4-turbo, temp=0.9, maxTokens=1000
    Console.WriteLine("Turn 3: expensive model (upgraded), high creativity, medium budget");
    
    // Turn 5: NumberOfTurns=5, TotalTokensUsed=6000, HasError=false
    // Expected: gpt-4-turbo, temp=0.9, maxTokens=500
    Console.WriteLine("Turn 5: expensive model, high creativity, low budget (budget constrained)");
    
    Console.WriteLine();
    Console.WriteLine("✓ Parameters adapt automatically to conversation context!");
}
catch (Exception ex)
{
    Console.WriteLine($"Async parameter resolution example error: {ex.Message}");
}
Console.WriteLine();

// Example 18: API Keys Service - Manage API Keys
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
    // Console.WriteLine($"Limit Reset: {createResponse.Data.LimitResetValue}");
    Console.WriteLine("(Requires provisioning key - example only)");
    Console.WriteLine();
    
    // Example: List all API keys
    Console.WriteLine("Listing API keys:");
    // var listResponse = await client.ApiKeys.ListAsync(includeDisabled: false);
    // foreach (var key in listResponse.Data)
    // {
    //     Console.WriteLine($"- {key.Name} ({key.Hash})");
    //     Console.WriteLine($"  Usage: ${key.Usage:F4}");
    //     Console.WriteLine($"  Limit Remaining: ${key.LimitRemaining:F2}");
    //     Console.WriteLine($"  Disabled: {key.Disabled}");
    // }
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
    // Console.WriteLine($"Current Key: {currentKey.Data.Name}");
    // Console.WriteLine($"Total Usage: ${currentKey.Data.Usage:F4}");
    // Console.WriteLine($"Daily Usage: ${currentKey.Data.UsageDaily:F4}");
    // Console.WriteLine($"Weekly Usage: ${currentKey.Data.UsageWeekly:F4}");
    // Console.WriteLine($"Monthly Usage: ${currentKey.Data.UsageMonthly:F4}");
    Console.WriteLine("(Works with regular API key)");
    Console.WriteLine();
    
    Console.WriteLine("✓ API Keys Service provides comprehensive key management!");
}
catch (Exception ex)
{
    Console.WriteLine($"API Keys example error: {ex.Message}");
}
Console.WriteLine();

Console.WriteLine("All examples completed!");

// Helper classes for tool examples
public class WeatherToolInput
{
    public required string Location { get; set; }
    public string? Units { get; set; }
}

public class WeatherToolOutput
{
    public required string Location { get; set; }
    public int Temperature { get; set; }
    public required string Condition { get; set; }
    public required string Units { get; set; }
}

public class CalcInput
{
    public double X { get; set; }
    public double Y { get; set; }
}

public class CalcOutput
{
    public double Result { get; set; }
}

public class SearchInput
{
    public required string Query { get; set; }
}

public class SearchOutput
{
    public required List<string> Results { get; set; }
}
