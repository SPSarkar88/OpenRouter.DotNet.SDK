using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using System.Text.Json;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;

namespace OpenRouter.Examples.EnvConfig;

/// <summary>
/// Multi-Turn Tool Orchestration Example
/// 
/// Demonstrates automatic multi-turn conversation with tool execution.
/// The model decides when to use tools and when to stop.
/// 
/// Pattern: User request → Model calls tools → Tools execute → Results fed back → Model continues or stops
/// </summary>
public static class Example20_MultiTurnOrchestration
{
    public static async Task RunAsync()
    {
        Console.WriteLine("Multi-Turn Tool Orchestration Example\n");

        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);

        // Define tools for a research assistant
        var searchTool = new Tool<SearchParams, SearchResult>(
            name: "search_web",
            description: "Search the web for information",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["query"] = JsonSchemaBuilder.String("The search query"),
                    ["numResults"] = JsonSchemaBuilder.Integer("Number of results to return")
                },
                required: new List<string> { "query" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\nTurn {context?.NumberOfTurns ?? 0}: Searching for \"{parameters.Query}\"...");
                
                await Task.Delay(1000); // Simulate search
                
                var results = new List<SearchResultItem>
                {
                    new() { Title = "OpenRouter Documentation", Snippet = "OpenRouter provides unified access to 300+ AI models..." },
                    new() { Title = "AI Model Comparison", Snippet = "Compare costs and capabilities across different providers..." },
                    new() { Title = "Best Practices Guide", Snippet = "Learn how to optimize your AI applications..." },
                };

                return new SearchResult
                {
                    Results = results.Take(parameters.NumResults ?? 3).ToList(),
                    Query = parameters.Query
                };
            });

        var extractFactsTool = new Tool<ExtractFactsParams, ExtractFactsResult>(
            name: "extract_facts",
            description: "Extract key facts from text data",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["text"] = JsonSchemaBuilder.String("The text to analyze")
                },
                required: new List<string> { "text" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\n📊 Turn {context?.NumberOfTurns ?? 0}: Extracting facts from text...");
                
                await Task.Delay(800);
                
                return new ExtractFactsResult
                {
                    Facts = new List<string>
                    {
                        "OpenRouter aggregates AI models from multiple providers",
                        "Supports 300+ language models",
                        "Provides unified API access",
                        "Includes cost optimization features"
                    },
                    WordCount = parameters.Text.Split(' ').Length
                };
            });

        var summarizeTool = new Tool<SummarizeParams, SummarizeResult>(
            name: "summarize_content",
            description: "Create a concise summary of content",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["content"] = JsonSchemaBuilder.String("Content to summarize"),
                    ["maxLength"] = JsonSchemaBuilder.Integer("Maximum summary length in words")
                },
                required: new List<string> { "content" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\n✍️ Turn {context?.NumberOfTurns ?? 0}: Creating summary...");
                
                await Task.Delay(600);
                
                return new SummarizeResult
                {
                    Summary = "OpenRouter is a unified platform that provides access to over 300 AI models from various providers, offering cost optimization and a standardized API.",
                    OriginalLength = parameters.Content.Split(' ').Length,
                    SummaryLength = 24
                };
            });

        // Create the request
        var request = new BetaResponsesRequest
        {
            Model = ExampleConfig.ModelName,
            Input = "Research OpenRouter and provide a comprehensive summary. Search for information, extract key facts, and create a final summary.",
            Store = false,
            ServiceTier = "auto"
        };

        Console.WriteLine("🤖 Starting multi-turn orchestration...\n");

        // Call the model with tools
        var result = client.CallModel(request, new ITool[] { searchTool, extractFactsTool, summarizeTool }, maxTurns: 10);

        // Get the orchestration details first (this executes the full workflow)
        var orchestration = await result.GetOrchestrationResultAsync();
        
        Console.WriteLine($"\n📊 Orchestration complete!");
        Console.WriteLine($"Total turns: {orchestration?.AllResponses.Count ?? 0}");
        Console.WriteLine($"Tools executed: {orchestration?.ToolExecutionResults.Count ?? 0}");
        Console.WriteLine($"Stopped by condition: {orchestration?.StoppedByCondition ?? false}");
        
        Console.WriteLine("\n📝 Tool execution summary:");
        for (int i = 0; i < (orchestration?.ToolExecutionResults.Count ?? 0); i++)
        {
            var toolResult = orchestration!.ToolExecutionResults[i];
            Console.WriteLine($"  {i + 1}. {toolResult.ToolName} - {(toolResult.Error != null ? "❌ Error" : "✅ Success")}");
        }

        Console.WriteLine("\n✨ Final answer:");
        
        // Get the final text from the last response
        var lastResponse = orchestration?.AllResponses.LastOrDefault();
        if (lastResponse?.Output != null)
        {
            foreach (var output in lastResponse.Output)
            {
                if (output.Type == "message" && output.Content != null)
                {
                    foreach (var content in output.Content)
                    {
                        if (content.Type == "output_text" && !string.IsNullOrEmpty(content.Text))
                        {
                            Console.WriteLine(content.Text);
                        }
                    }
                }
                else if (output.Type == "text" && !string.IsNullOrEmpty(output.Text))
                {
                    Console.WriteLine(output.Text);
                }
            }
        }
    }

    // Supporting classes
    private class SearchParams
    {
        public string Query { get; set; } = "";
        public int? NumResults { get; set; }
    }

    private class SearchResult
    {
        public List<SearchResultItem> Results { get; set; } = new();
        public string Query { get; set; } = "";
    }

    private class SearchResultItem
    {
        public string Title { get; set; } = "";
        public string Snippet { get; set; } = "";
    }

    private class ExtractFactsParams
    {
        public string Text { get; set; } = "";
    }

    private class ExtractFactsResult
    {
        public List<string> Facts { get; set; } = new();
        public int WordCount { get; set; }
    }

    private class SummarizeParams
    {
        public string Content { get; set; } = "";
        public int? MaxLength { get; set; }
    }

    private class SummarizeResult
    {
        public string Summary { get; set; } = "";
        public int OriginalLength { get; set; }
        public int SummaryLength { get; set; }
    }
}



