using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using OpenRouter.SDK.Models;
using System.Text;
using System.Text.Json;

namespace OpenRouter.Examples.EnvConfig;

/// <summary>
/// Smart Research Assistant - A comprehensive example showcasing all OpenRouter SDK features
/// 
/// This example demonstrates:
/// 1. Service Discovery (Models, Providers, Credits)
/// 2. Chat Completions (Basic & Streaming)
/// 3. Embeddings (Document Similarity)
/// 4. Beta Responses API (Advanced Tool Orchestration)
/// 5. Function/Tool Calling
/// 6. Error Handling & Retry Logic
/// 7. Cost Tracking & Analytics
/// 8. Multi-model Selection & Fallback
/// 
/// Real-world Use Case: Research Assistant that can:
/// - Search and analyze information
/// - Generate structured reports
/// - Find similar documents
/// - Fact-check claims
/// - Create summaries with citations
/// </summary>
public static class SmartResearchAssistant
{
    private static readonly StringBuilder OutputLog = new();
    private static decimal TotalCost = 0m;

    public static async Task RunAsync()
    {
        var apiKey = ExampleConfig.ApiKey;
        var client = new OpenRouterClient(apiKey);

        OutputLog.AppendLine("=== Smart Research Assistant - Execution Log ===");
        OutputLog.AppendLine($"Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

        try
        {
            // Step 1: System Initialization & Discovery
            await Step1_InitializeAndDiscover(client);

            // Step 2: Multi-Source Research with Embeddings
            await Step2_DocumentSimilaritySearch(client);

            // Step 3: Intelligent Research with Tools
            await Step3_ResearchWithTools(client);

            // Step 4: Streaming Report Generation
            await Step4_StreamingReportGeneration(client);

            // Step 5: Analytics & Cost Tracking
            await Step5_AnalyticsAndCostTracking(client);

            // Save the complete output
            await SaveOutput();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            OutputLog.AppendLine($"\n[ERROR] {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Step 1: Initialize and discover available resources
    /// Demonstrates: Models API, Providers API, Credits API
    /// </summary>
    private static async Task Step1_InitializeAndDiscover(OpenRouterClient client)
    {
        Console.WriteLine("\nSTEP 1: System Initialization & Discovery\n");

        OutputLog.AppendLine("--- STEP 1: System Initialization & Discovery ---\n");

        try
        {
            // Check available credits
            Console.WriteLine("Checking account credits...");
            var credits = await client.Credits.GetCreditsAsync();
            Console.WriteLine($"   Total Credits: ${credits.TotalCredits:F4}");
            Console.WriteLine($"   Balance Remaining: ${credits.Balance:F4}");
            Console.WriteLine($"   Total Usage: ${credits.TotalUsage:F4}");
            OutputLog.AppendLine($"Total Credits: ${credits.TotalCredits:F4}");
            OutputLog.AppendLine($"Balance: ${credits.Balance:F4}");
            OutputLog.AppendLine($"Total Usage: ${credits.TotalUsage:F4}\n");

            // Get model count
            Console.WriteLine("\nFetching available models...");
            var modelCount = await client.Models.GetCountAsync();
            Console.WriteLine($"   Total Models Available: {modelCount.Count}");
            OutputLog.AppendLine($"Total Models Available: {modelCount.Count}\n");

            // List top recommended models for research tasks
            Console.WriteLine("\nRecommended models for research tasks:");
            var models = await client.Models.GetModelsAsync();
            var researchModels = models.Data
                .Where(m => m.ContextLength >= 32000 && m.Pricing?.Prompt != null)
                .OrderBy(m => m.Pricing.Prompt)
                .Take(5)
                .ToList();

            foreach (var model in researchModels)
            {
                Console.WriteLine($"   • {model.Id}");
                Console.WriteLine($"     Context: {model.ContextLength:N0} tokens");
                var promptCost = double.TryParse(model.Pricing?.Prompt, out var p) ? p * 1000000 : 0;
                var completionCost = double.TryParse(model.Pricing?.Completion, out var c) ? c * 1000000 : 0;
                Console.WriteLine($"     Cost: ${promptCost:F2}/M input, ${completionCost:F2}/M output");
                
                OutputLog.AppendLine($"Model: {model.Id}");
                OutputLog.AppendLine($"  Context: {model.ContextLength:N0}");
                OutputLog.AppendLine($"  Cost: ${promptCost:F2}/M in, ${completionCost:F2}/M out\n");
            }

            // List available providers
            Console.WriteLine("\nAvailable providers:");
            var providers = await client.Providers.ListAsync();
            var topProviders = providers.Data.Take(5);
            foreach (var provider in topProviders)
            {
                Console.WriteLine($"   • {provider.Name} ({provider.Slug})");
            }
            OutputLog.AppendLine($"\nProviders: {string.Join(", ", topProviders.Select(p => p.Name))}\n");

            Console.WriteLine("\nInitialization complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: {ex.Message}");
            OutputLog.AppendLine($"[WARNING] Initialization error: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Step 2: Document Similarity Search using Embeddings
    /// Demonstrates: Embeddings API, Semantic Search
    /// </summary>
    private static async Task Step2_DocumentSimilaritySearch(OpenRouterClient client)
    {
        Console.WriteLine("\n\nSTEP 2: Document Similarity Search\n");

        OutputLog.AppendLine("\n--- STEP 2: Document Similarity Search ---\n");

        try
        {
            // Sample research documents
            var documents = new List<string>
            {
                "Artificial Intelligence is transforming healthcare through predictive diagnostics and personalized medicine.",
                "Machine learning algorithms can analyze medical imaging to detect diseases earlier than traditional methods.",
                "Climate change is causing rising sea levels and more frequent extreme weather events worldwide.",
                "Renewable energy sources like solar and wind are becoming more cost-effective than fossil fuels.",
                "The human brain has approximately 86 billion neurons that form complex networks for cognition."
            };

            Console.WriteLine("📄 Creating embeddings for document corpus...");
            OutputLog.AppendLine("Document Corpus:\n");
            for (int i = 0; i < documents.Count; i++)
            {
                Console.WriteLine($"   Doc {i + 1}: {documents[i].Substring(0, Math.Min(60, documents[i].Length))}...");
                OutputLog.AppendLine($"{i + 1}. {documents[i]}");
            }

            // Generate embeddings
            var embeddingsResponse = await client.Embeddings.GenerateAsync(new EmbeddingRequest
            {
                Model = "openai/text-embedding-3-small",
                Input = documents
            });

            Console.WriteLine($"\nGenerated {embeddingsResponse.Data.Count} embeddings");
            OutputLog.AppendLine($"\nGenerated {embeddingsResponse.Data.Count} embeddings");

            // Search query
            var query = "How is AI used in medical diagnosis?";
            Console.WriteLine($"\nSearch query: \"{query}\"");
            OutputLog.AppendLine($"\nSearch Query: \"{query}\"");

            var queryEmbedding = await client.Embeddings.GenerateAsync(new EmbeddingRequest
            {
                Model = "openai/text-embedding-3-small",
                Input = new List<string> { query }
            });

            // Calculate cosine similarity
            var similarities = new List<(int index, double similarity, string doc)>();
            var queryVec = queryEmbedding.Data[0].Embedding as List<double> ?? new List<double>();

            for (int i = 0; i < embeddingsResponse.Data.Count; i++)
            {
                var docVec = embeddingsResponse.Data[i].Embedding as List<double> ?? new List<double>();
                var similarity = CosineSimilarity(queryVec, docVec);
                similarities.Add((i, similarity, documents[i]));
            }

            // Display results
            Console.WriteLine("\nSimilarity Rankings:");
            OutputLog.AppendLine("\nSimilarity Results:");
            var ranked = similarities.OrderByDescending(s => s.similarity).ToList();
            for (int i = 0; i < ranked.Count; i++)
            {
                Console.WriteLine($"   {i + 1}. Score: {ranked[i].similarity:F4} - Doc {ranked[i].index + 1}");
                Console.WriteLine($"      {ranked[i].doc.Substring(0, Math.Min(80, ranked[i].doc.Length))}...");
                
                OutputLog.AppendLine($"{i + 1}. Similarity: {ranked[i].similarity:F4}");
                OutputLog.AppendLine($"   {ranked[i].doc}\n");
            }

            Console.WriteLine("\nSemantic search complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in embeddings: {ex.Message}");
            OutputLog.AppendLine($"[ERROR] Embeddings failed: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Step 3: Research with Tool Calling
    /// Demonstrates: Beta Responses API, Function Calling, Multi-turn Orchestration
    /// </summary>
    private static async Task Step3_ResearchWithTools(OpenRouterClient client)
    {
        Console.WriteLine("\n\nSTEP 3: Intelligent Research with Tools\n");

        OutputLog.AppendLine("\n--- STEP 3: Intelligent Research with Tools ---\n");

        // Define research tools
        var searchTool = new Tool<SearchParams, SearchResult>(
            name: "search_knowledge_base",
            description: "Search a knowledge base for relevant information",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["query"] = JsonSchemaBuilder.String("The search query"),
                    ["category"] = JsonSchemaBuilder.String("Category to search in: technology, science, health, climate")
                },
                required: new List<string> { "query", "category" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\nSearching knowledge base...");
                Console.WriteLine($"   Query: {parameters.Query}");
                Console.WriteLine($"   Category: {parameters.Category}");
                
                OutputLog.AppendLine($"\n[TOOL CALL] search_knowledge_base");
                OutputLog.AppendLine($"  Query: {parameters.Query}");
                OutputLog.AppendLine($"  Category: {parameters.Category}");

                // Simulate knowledge base search
                await Task.Delay(500);
                
                var results = new List<string>
                {
                    $"Recent studies in {parameters.Category} show significant advancements related to {parameters.Query}.",
                    $"Peer-reviewed research indicates that {parameters.Query} has multiple applications in {parameters.Category}.",
                    $"Expert consensus in {parameters.Category} suggests that {parameters.Query} will continue to evolve."
                };

                var result = string.Join(" ", results);
                Console.WriteLine($"   Found: {results.Count} relevant sources");
                OutputLog.AppendLine($"  Results: {results.Count} sources found\n");

                return new SearchResult
                {
                    Results = results,
                    Count = results.Count,
                    Query = parameters.Query
                };
            }
        );

        var factCheckTool = new Tool<FactCheckParams, FactCheckResult>(
            name: "fact_check_claim",
            description: "Verify the accuracy of a factual claim",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["claim"] = JsonSchemaBuilder.String("The claim to fact-check")
                },
                required: new List<string> { "claim" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\n✓ Fact-checking claim...");
                Console.WriteLine($"   Claim: {parameters.Claim}");
                
                OutputLog.AppendLine($"\n[TOOL CALL] fact_check_claim");
                OutputLog.AppendLine($"  Claim: {parameters.Claim}");

                // Simulate fact-checking
                await Task.Delay(300);
                
                var verified = true;
                var confidence = 0.92;
                var sources = new List<string> { "Scientific Journal A", "Research Database B", "Expert Review C" };

                Console.WriteLine($"   Verified: {verified} (Confidence: {confidence:P0})");
                OutputLog.AppendLine($"  Verified: {verified}");
                OutputLog.AppendLine($"  Confidence: {confidence:P0}");
                OutputLog.AppendLine($"  Sources: {string.Join(", ", sources)}\n");

                return new FactCheckResult
                {
                    Verified = verified,
                    Confidence = confidence,
                    Sources = sources
                };
            }
        );

        var summarizeTool = new Tool<SummarizeParams, SummarizeResult>(
            name: "create_summary",
            description: "Create a structured summary with citations",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["content"] = JsonSchemaBuilder.String("Content to summarize"),
                    ["maxLength"] = JsonSchemaBuilder.Number("Maximum summary length in words")
                },
                required: new List<string> { "content" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\nCreating summary...");
                Console.WriteLine($"   Max length: {parameters.MaxLength} words");
                
                OutputLog.AppendLine($"\n[TOOL CALL] create_summary");
                OutputLog.AppendLine($"  Content length: {parameters.Content.Length} chars");
                OutputLog.AppendLine($"  Max length: {parameters.MaxLength} words");

                var response = await client.Chat.CreateAsync(new ChatCompletionRequest
                {
                    Model = "openai/gpt-4o-mini",
                    Messages = new List<Message>
                    {
                        new UserMessage { Content = $"Summarize this in {parameters.MaxLength} words or less:\n\n{parameters.Content}" }
                    },
                    Temperature = 0.3
                });

                var summary = response.Choices?[0]?.Message?.Content ?? "";
                Console.WriteLine($"   Generated summary: {summary.Length} characters");
                
                OutputLog.AppendLine($"  Summary: {summary}\n");

                return new SummarizeResult
                {
                    Summary = summary,
                    OriginalLength = parameters.Content.Split(' ').Length,
                    SummaryLength = summary.Split(' ').Length
                };
            }
        );

        // Execute research workflow
        var request = new BetaResponsesRequest
        {
            Model = "openai/gpt-4o-mini",
            Input = new List<ResponsesEasyInputMessage>
            {
                new()
                {
                    Role = "system",
                    Content = "You are a research assistant. Use the available tools to research a topic thoroughly, fact-check claims, and create a summary."
                },
                new()
                {
                    Role = "user",
                    Content = "Research the impact of artificial intelligence on healthcare. Search for information, verify key claims, and provide a summary."
                }
            },
            Instructions = "Use all three tools: search_knowledge_base, fact_check_claim, and create_summary to provide comprehensive research.",
            Temperature = 0.4,
            MaxOutputTokens = 1000,
            Store = false
        };

        Console.WriteLine("\nStarting research workflow...");
        var result = client.CallModel(request, new ITool[] { searchTool, factCheckTool, summarizeTool });

        Console.WriteLine("\n📄 Research Output:\n");
        var researchOutput = new StringBuilder();
        await foreach (var textChunk in result.GetTextStreamAsync())
        {
            Console.Write(textChunk);
            researchOutput.Append(textChunk);
        }

        OutputLog.AppendLine("\nFinal Research Output:");
        OutputLog.AppendLine(researchOutput.ToString());
        OutputLog.AppendLine("\n");

        Console.WriteLine("\n\nResearch workflow complete!");
    }

    /// <summary>
    /// Step 4: Streaming Report Generation
    /// Demonstrates: Chat Completions with Streaming, Provider Preferences
    /// </summary>
    private static async Task Step4_StreamingReportGeneration(OpenRouterClient client)
    {
        Console.WriteLine("\n\nSTEP 4: Streaming Report Generation\n");

        OutputLog.AppendLine("\n--- STEP 4: Streaming Report Generation ---\n");

        try
        {
            Console.WriteLine("Generating comprehensive research report...\n");

            var request = new ChatCompletionRequest
            {
                Model = "openai/gpt-4o-mini",
                Messages = new List<Message>
                {
                    new SystemMessage 
                    { 
                        Content = "You are a professional research report writer. Create well-structured, informative reports with clear sections and citations."
                    },
                    new UserMessage 
                    { 
                        Content = @"Create a brief research report on 'The Future of Renewable Energy' with these sections:
1. Executive Summary
2. Current State
3. Key Trends
4. Challenges
5. Conclusion

Keep it concise (about 300 words total)."
                    }
                },
                Temperature = 0.7,
                MaxTokens = 800,
                Stream = true,
                Provider = new ProviderPreferences
                {
                    AllowFallbacks = true,
                    Sort = "price" // Prefer lower cost models
                }
            };

            var report = new StringBuilder();
            var wordCount = 0;

            Console.WriteLine("═══════════════════════ REPORT ════════════════════════\n");

            await foreach (var chunk in client.Chat.CreateStreamAsync(request))
            {
                var content = chunk.Choices?[0]?.Delta?.Content;
                if (!string.IsNullOrEmpty(content))
                {
                    Console.Write(content);
                    report.Append(content);
                    wordCount += content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

                    // Note: Streaming chunks don't include usage in OpenRouter
                    // Usage is only available in the final non-streaming response
                }
            }

            Console.WriteLine("\n\n═══════════════════════════════════════════════════════\n");
            Console.WriteLine($"📈 Report Statistics:");
            Console.WriteLine($"   Word Count: {wordCount}");
            Console.WriteLine($"   Character Count: {report.Length}");
            
            OutputLog.AppendLine($"Report Generated:");
            OutputLog.AppendLine($"Word Count: {wordCount}");
            OutputLog.AppendLine($"Character Count: {report.Length}");
            OutputLog.AppendLine($"\nReport Content:\n{report}\n");

            Console.WriteLine("\n\nStreaming report generation complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in report generation: {ex.Message}");
            OutputLog.AppendLine($"[ERROR] Report generation failed: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Step 5: Analytics and Cost Tracking
    /// Demonstrates: Analytics API, Usage Tracking, Cost Management
    /// </summary>
    private static async Task Step5_AnalyticsAndCostTracking(OpenRouterClient client)
    {
        Console.WriteLine("\n\nSTEP 5: Analytics & Cost Tracking\n");

        OutputLog.AppendLine("\n--- STEP 5: Analytics & Cost Tracking ---\n");

        try
        {
            Console.WriteLine("Fetching usage analytics...");
            
            var analytics = await client.Analytics.GetUserActivityAsync();
            
            Console.WriteLine($"\n💰 Usage Summary:");
            Console.WriteLine($"   Total Requests: {analytics.Data?.Count ?? 0}");
            
            if (analytics.Data != null && analytics.Data.Any())
            {
                var totalUsage = analytics.Data.Sum(a => a.Usage);
                Console.WriteLine($"   Total Usage: ${totalUsage:F6}");
                
                var modelUsage = analytics.Data
                    .GroupBy(a => a.Model)
                    .Select(g => new { Model = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(5);

                Console.WriteLine($"\n📈 Top Models Used:");
                foreach (var usage in modelUsage)
                {
                    Console.WriteLine($"   • {usage.Model}: {usage.Count} requests");
                }

                OutputLog.AppendLine($"Total Requests: {analytics.Data.Count}");
                OutputLog.AppendLine($"Total Usage: ${totalUsage:F6}");
                OutputLog.AppendLine($"\nTop Models:");
                foreach (var usage in modelUsage)
                {
                    OutputLog.AppendLine($"  {usage.Model}: {usage.Count} requests");
                }
            }

            Console.WriteLine($"\n💵 Session Cost Estimate: ${TotalCost:F6}");
            OutputLog.AppendLine($"\nEstimated Session Cost: ${TotalCost:F6}");

            Console.WriteLine("\nAnalytics complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Note: Analytics may not be available for all account types");
            Console.WriteLine($"   Error: {ex.Message}");
            OutputLog.AppendLine($"[INFO] Analytics not available: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Save complete output log to file
    /// </summary>
    private static async Task SaveOutput()
    {
        OutputLog.AppendLine($"\nCompleted: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        OutputLog.AppendLine($"Total Estimated Cost: ${TotalCost:F6}");

        var outputPath = Path.Combine(AppContext.BaseDirectory, "SmartResearchAssistant_Output.txt");
        await File.WriteAllTextAsync(outputPath, OutputLog.ToString());
        
        Console.WriteLine($"\n\n📁 Complete execution log saved to:");
        Console.WriteLine($"   {outputPath}");
    }

    /// <summary>
    /// Calculate cosine similarity between two vectors
    /// </summary>
    private static double CosineSimilarity(List<double> vec1, List<double> vec2)
    {
        var dotProduct = vec1.Zip(vec2, (a, b) => a * b).Sum();
        var magnitude1 = Math.Sqrt(vec1.Sum(x => x * x));
        var magnitude2 = Math.Sqrt(vec2.Sum(x => x * x));
        return dotProduct / (magnitude1 * magnitude2);
    }

    // Supporting classes for tool parameters and results
    private class SearchParams
    {
        public string Query { get; set; } = "";
        public string Category { get; set; } = "";
    }

    private class SearchResult
    {
        public List<string> Results { get; set; } = new();
        public int Count { get; set; }
        public string Query { get; set; } = "";
    }

    private class FactCheckParams
    {
        public string Claim { get; set; } = "";
    }

    private class FactCheckResult
    {
        public bool Verified { get; set; }
        public double Confidence { get; set; }
        public List<string> Sources { get; set; } = new();
    }

    private class SummarizeParams
    {
        public string Content { get; set; } = "";
        public int MaxLength { get; set; } = 100;
    }

    private class SummarizeResult
    {
        public string Summary { get; set; } = "";
        public int OriginalLength { get; set; }
        public int SummaryLength { get; set; }
    }
}


