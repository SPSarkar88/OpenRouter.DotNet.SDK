using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using OpenRouter.SDK.Models;

Console.WriteLine("===========================================");
Console.WriteLine("Example 17: Async Parameter Resolution");
Console.WriteLine("===========================================\n");

await Example17.RunAsync();

Console.WriteLine("\n===========================================");
Console.WriteLine("Example completed!");
Console.WriteLine("===========================================");

public static class Example17
{
    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

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
                    ctx.NumberOfTurns > 2 ? ExampleConfig.ModelName : ExampleConfig.ModelName),
                
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
                
                // Input with dynamic wrapper (required for DynamicBetaResponsesRequest)
                Input = new DynamicParameter<List<ResponsesInputItem>>(ctx => new List<ResponsesInputItem>
                {
                    new() { Type = "text", Text = "Explain quantum computing in simple terms." }
                }),
                
                // Instructions (system message)
                Instructions = new DynamicParameter<string?>(ctx => "You are a helpful science educator.")
            };

            // Call the model with dynamic parameters
            var result = client.CallModelDynamic(dynamicRequest);
            var dynamicResponse = await result.GetResponseAsync();
            
            Console.WriteLine($"Model used: {dynamicResponse.Model}");
            
            // Extract text from response (handle both "text" and "message" types)
            string? responseText = null;
            if (dynamicResponse.Output?.Count > 0)
            {
                var output = dynamicResponse.Output[0];
                if (output.Type == "message" && output.Content != null)
                {
                    foreach (var content in output.Content)
                    {
                        if (content.Type == "output_text" && !string.IsNullOrEmpty(content.Text))
                        {
                            responseText = content.Text;
                            break;
                        }
                    }
                }
                else if (output.Type == "text" && !string.IsNullOrEmpty(output.Text))
                {
                    responseText = output.Text;
                }
            }
            
            Console.WriteLine($"Response: {responseText?.Substring(0, Math.Min(150, responseText.Length))}...");
            Console.WriteLine($"Tokens used: {dynamicResponse.Usage?.TotalTokens ?? 0}");
            Console.WriteLine();
            
            Console.WriteLine("Parameters adapt automatically to conversation context!");
            Console.WriteLine("Turn 1: gpt-3.5-turbo, temp=0.9, maxTokens=2000");
            Console.WriteLine("Turn 3: gpt-4-turbo (upgraded), temp=0.9, maxTokens=1000");
            Console.WriteLine("Turn 5: gpt-4-turbo, temp=0.9, maxTokens=500 (budget constrained)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Async parameter resolution example error: {ex.Message}");
        }
    }
}


