using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using System.Text.Json;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;

namespace OpenRouter.Examples.EnvConfig;

/// <summary>
/// Orchestrated AI Workflow Example
/// 
/// Demonstrates advanced workflow orchestration with:
/// - Dynamic parameter resolution (model selection, temperature adjustment)
/// - Stop conditions (budget limits, turn limits, success criteria)
/// - Workflow state management
/// - Adaptive behavior based on context
/// 
/// Use case: Content creation workflow with quality checks and iterative refinement
/// </summary>
public static class Example21_OrchestratedWorkflow
{
    // Workflow state tracker
    private class WorkflowState
    {
        public int DraftsCreated { get; set; }
        public int QualityScore { get; set; }
        public int TokensUsed { get; set; }
        public int MaxTokens { get; set; } = 10000;
        public string Phase { get; set; } = "drafting"; // drafting, refinement, finalization
    }

    private static readonly WorkflowState State = new();

    public static async Task RunAsync()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("  Orchestrated AI Workflow Example");
        Console.WriteLine("  Content Creation with Quality Assurance");
        Console.WriteLine("═══════════════════════════════════════════════════════\n");

        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        // Reset state
        State.DraftsCreated = 0;
        State.QualityScore = 0;
        State.TokensUsed = 0;
        State.Phase = "drafting";

        // Tool 1: Content Quality Analyzer
        var analyzeQuality = new Tool<QualityParams, QualityResult>(
            name: "analyze_content_quality",
            description: "Analyze the quality of written content and provide a score",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["content"] = JsonSchemaBuilder.String("The content to analyze"),
                    ["criteria"] = JsonSchemaBuilder.Array(
                        JsonSchemaBuilder.String("Quality criterion"),
                        "Quality criteria to check")
                },
                required: new List<string> { "content", "criteria" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\nTurn {context?.NumberOfTurns ?? 0}: Analyzing content quality...");
                
                await Task.Delay(1000);
                
                // Simulate quality analysis
                var score = new Random().Next(70, 101);
                State.QualityScore = score;
                
                var feedback = score >= 85 
                    ? "Excellent quality - ready for publication"
                    : score >= 75
                    ? "Good quality - minor improvements suggested"
                    : "Needs refinement - significant improvements required";
                
                Console.WriteLine($"   Quality Score: {score}/100");
                Console.WriteLine($"   Feedback: {feedback}");
                
                var passedCount = (int)(parameters.Criteria.Count * (score / 100.0));
                
                return new QualityResult
                {
                    Score = score,
                    Feedback = feedback,
                    PassedCriteria = parameters.Criteria.Take(passedCount).ToList(),
                    FailedCriteria = parameters.Criteria.Skip(passedCount).ToList()
                };
            });

        // Tool 2: Content Improver
        var improveContent = new Tool<ImproveParams, ImproveResult>(
            name: "improve_content",
            description: "Improve content based on specific aspects",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["content"] = JsonSchemaBuilder.String("Original content"),
                    ["aspects"] = JsonSchemaBuilder.Array(
                        JsonSchemaBuilder.String("Aspect to improve"),
                        "Aspects to improve")
                },
                required: new List<string> { "content", "aspects" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\nTurn {context?.NumberOfTurns ?? 0}: Improving content...");
                Console.WriteLine($"   Aspects: {string.Join(", ", parameters.Aspects)}");
                
                await Task.Delay(1200);
                
                State.DraftsCreated++;
                
                return new ImproveResult
                {
                    ImprovedContent = parameters.Content + " [IMPROVED - enhanced clarity and engagement]",
                    AspectsAddressed = parameters.Aspects,
                    DraftNumber = State.DraftsCreated
                };
            });

        // Tool 3: SEO Optimizer
        var optimizeSEO = new Tool<SEOParams, SEOResult>(
            name: "optimize_seo",
            description: "Optimize content for search engines",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["content"] = JsonSchemaBuilder.String("Content to optimize"),
                    ["targetKeywords"] = JsonSchemaBuilder.Array(
                        JsonSchemaBuilder.String("Keyword"),
                        "Target keywords")
                },
                required: new List<string> { "content", "targetKeywords" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\nTurn {context?.NumberOfTurns ?? 0}: Optimizing for SEO...");
                Console.WriteLine($"   Keywords: {string.Join(", ", parameters.TargetKeywords)}");
                
                await Task.Delay(800);
                
                var keywordDensity = parameters.TargetKeywords.ToDictionary(
                    k => k, 
                    k => Math.Round(new Random().NextDouble() * 3, 2)
                );
                
                return new SEOResult
                {
                    OptimizedContent = parameters.Content,
                    KeywordDensity = keywordDensity,
                    SEOScore = new Random().Next(80, 101)
                };
            });

        // Create dynamic request with adaptive parameters
        var dynamicRequest = new DynamicBetaResponsesRequest
        {
            // Dynamic model selection based on workflow phase
            Model = new DynamicParameter<string>(ctx =>
            {
                // Use configured model from .env file
                var model = ExampleConfig.ModelName;
                Console.WriteLine($"Phase: {State.Phase} -> Using model: {model}");
                return model;
            }),
            
            // Dynamic temperature based on phase
            Temperature = new DynamicParameter<double?>(ctx =>
            {
                var temp = State.Phase switch
                {
                    "drafting" => 0.8,
                    "refinement" => 0.4,
                    "finalization" => 0.2,
                    _ => 0.5
                };
                Console.WriteLine($"Temperature: {temp} (Phase: {State.Phase})");
                return temp;
            }),
            
            // Base request - wrapped in DynamicParameter (required for DynamicBetaResponsesRequest)
            Input = new DynamicParameter<List<ResponsesInputItem>>(ctx => new List<ResponsesInputItem>
            {
                new()
                {
                    Type = "text",
                    Text = "Write a blog post about 'The Future of AI in Software Development'."
                }
            }),
            
            // Add instructions separately
            Instructions = new DynamicParameter<string?>(ctx => 
                "You are a professional content creator. Use the provided tools to analyze quality, improve content, and optimize for SEO.")
        };

        // Define stop conditions
        var stopConditions = new List<StopCondition>
        {
            // Stop if quality threshold met
            new(async (steps) =>
            {
                if (State.QualityScore >= 85)
                {
                    Console.WriteLine("\nStop condition: Quality threshold met");
                    return true;
                }
                return false;
            }),
            
            // Stop if budget exceeded
            new(async (steps) =>
            {
                if (State.TokensUsed >= State.MaxTokens)
                {
                    Console.WriteLine("\nStop condition: Budget exceeded");
                    return true;
                }
                return false;
            }),
            
            // Stop after max iterations
            new(async (steps) =>
            {
                if (State.DraftsCreated >= 3)
                {
                    Console.WriteLine("\nStop condition: Max iterations reached");
                    return true;
                }
                return false;
            }),
            
            // Safety: max turns
            StopConditions.StepCountIs(12)
        };

        Console.WriteLine("Starting orchestrated workflow...\n");

        // Call the model with dynamic parameters
        var result = client.CallModelDynamic(
            dynamicRequest,
            new ITool[] { analyzeQuality, improveContent, optimizeSEO },
            maxTurns: 15,
            stopConditions: stopConditions
        );

        int currentTurn = 0;

        // Monitor the workflow
        await foreach (var chunk in result.GetFullStreamAsync())
        {
            if (chunk.Delta != null)
            {
                var item = chunk.Delta;
                if (item.Type == "function_call" && item.FunctionCall != null)
                {
                    currentTurn++;
                    var fcJson = JsonSerializer.Serialize(item.FunctionCall);
                    using var doc = JsonDocument.Parse(fcJson);
                    var root = doc.RootElement;
                    var name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "unknown";
                    Console.WriteLine($"\n━━━ Turn {currentTurn} ━━━");
                    Console.WriteLine($"📞 Calling: {name}");
                }
                else if (item.Type == "output_text")
                {
                    Console.Write(item.Text);
                }
            }
        }

        Console.WriteLine("\n\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("  Workflow Summary");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

        var orchestration = await result.GetOrchestrationResultAsync();
        
        Console.WriteLine($"Workflow Statistics:");
        Console.WriteLine($"   Total turns: {orchestration?.AllResponses.Count ?? 0}");
        Console.WriteLine($"   Tools executed: {orchestration?.ToolExecutionResults.Count ?? 0}");
        Console.WriteLine($"   Drafts created: {State.DraftsCreated}");
        Console.WriteLine($"   Final quality score: {State.QualityScore}/100");
        Console.WriteLine($"   Final phase: {State.Phase}");
        Console.WriteLine($"   Budget used: {State.TokensUsed}/{State.MaxTokens} tokens");
        Console.WriteLine($"   Stopped by condition: {(orchestration?.StoppedByCondition ?? false ? "Yes" : "No (natural completion)")}");

        Console.WriteLine($"\nTool Execution Timeline:");
        for (int i = 0; i < (orchestration?.ToolExecutionResults.Count ?? 0); i++)
        {
            var toolResult = orchestration!.ToolExecutionResults[i];
            Console.WriteLine($"   {i + 1}. {toolResult.ToolName}");
        }

        Console.WriteLine($"\nFinal Content:\n");
        Console.WriteLine(await result.GetTextAsync());
    }

    // Supporting classes
    private class QualityParams
    {
        public string Content { get; set; } = "";
        public List<string> Criteria { get; set; } = new();
    }

    private class QualityResult
    {
        public int Score { get; set; }
        public string Feedback { get; set; } = "";
        public List<string> PassedCriteria { get; set; } = new();
        public List<string> FailedCriteria { get; set; } = new();
    }

    private class ImproveParams
    {
        public string Content { get; set; } = "";
        public List<string> Aspects { get; set; } = new();
    }

    private class ImproveResult
    {
        public string ImprovedContent { get; set; } = "";
        public List<string> AspectsAddressed { get; set; } = new();
        public int DraftNumber { get; set; }
    }

    private class SEOParams
    {
        public string Content { get; set; } = "";
        public List<string> TargetKeywords { get; set; } = new();
    }

    private class SEOResult
    {
        public string OptimizedContent { get; set; } = "";
        public Dictionary<string, double> KeywordDensity { get; set; } = new();
        public int SEOScore { get; set; }
    }
}



