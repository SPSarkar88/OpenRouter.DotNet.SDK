using OpenRouter.SDK;
using OpenRouter.Examples.EnvConfig;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;

namespace OpenRouter.Examples.EnvConfig;

/// <summary>
/// Chained LLM Workflow Example
/// 
/// Demonstrates chaining multiple LLM calls together where:
/// - One model's output feeds into another model
/// - Different models specialized for different tasks
/// - Results aggregated and refined through multiple models
/// 
/// Pattern: Model A → Tool calls Model B → Results to Model A → Final output
/// 
/// Use cases:
/// - Expert consensus (multiple models vote on answer)
/// - Specialized pipelines (creative model → critic model → editor model)
/// - Translation chains (translate → verify → refine)
/// </summary>
public static class Example22_ChainedLLMWorkflow
{
    public static async Task RunAsync()
    {
        await CreativeWritingChainExample();
        await MultiModelConsensusExample();
        await TranslationChainExample();
        
        Console.WriteLine("\n\n🎉 All chained LLM workflow examples completed!");
    }

    /// <summary>
    /// Example 1: Creative Writing Pipeline
    /// Writer → Critic → Editor chain
    /// </summary>
    private static async Task CreativeWritingChainExample()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("  Chained LLM Example 1: Creative Writing Pipeline");
        Console.WriteLine("  Writer → Critic → Editor");
        Console.WriteLine("═══════════════════════════════════════════════════════\n");

        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);
        var outputContent = new System.Text.StringBuilder();
        outputContent.AppendLine("=== Creative Writing Pipeline - Generated Content ===");
        outputContent.AppendLine($"Timestamp: {DateTime.Now}\n");

        // Tool 1: Creative Writer
        var callCreativeWriter = new Tool<WriterParams, WriterResult>(
            name: "call_creative_writer",
            description: "Call a creative writing specialist model to generate content",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["prompt"] = JsonSchemaBuilder.String("What to write about"),
                    ["style"] = JsonSchemaBuilder.String("Writing style")
                },
                required: new List<string> { "prompt", "style" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\nSTEP 1 - Creative Writer (Turn {context?.NumberOfTurns ?? 0})");
                Console.WriteLine($"   Prompt: {parameters.Prompt}");
                Console.WriteLine($"   Style: {parameters.Style}");
                
                // Call a creative model
                var response = await client.Chat.CreateAsync(new ChatCompletionRequest
                {
                    Model = "openai/gpt-4o-mini",
                    Messages = new List<Message>
                    {
                        new UserMessage { Content = $"You are a creative writer. Write a short piece (100-150 words) about: {parameters.Prompt}. Use a {parameters.Style} style." }
                    },
                    Temperature = 0.9
                });
                
                var content = response.Choices?[0]?.Message?.Content ?? "";
                Console.WriteLine($"   Generated: {content.Substring(0, Math.Min(100, content.Length))}...");
                
                outputContent.AppendLine("\n--- STEP 1: Creative Writer ---");
                outputContent.AppendLine($"Prompt: {parameters.Prompt}");
                outputContent.AppendLine($"Style: {parameters.Style}");
                outputContent.AppendLine($"Generated Content:\n{content}\n");
                
                return new WriterResult
                {
                    Content = content,
                    WordCount = content.Split(' ').Length,
                    Model = "openai/gpt-4o-mini"
                };
            },
            requiresApproval: false
        );

        // Tool 2: Literary Critic
        var callLiteraryCritic = new Tool<CriticParams, CriticResult>(
            name: "call_literary_critic",
            description: "Call a critical analysis model to review content",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["content"] = JsonSchemaBuilder.String("Content to review")
                },
                required: new List<string> { "content" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\nSTEP 2 - Literary Critic (Turn {context?.NumberOfTurns ?? 0})");
                
                var response = await client.Chat.CreateAsync(new ChatCompletionRequest
                {
                    Model = "openai/gpt-4o",
                    Messages = new List<Message>
                    {
                        new UserMessage { Content = $@"You are a literary critic. Analyze this content and provide:
1. Strengths (2-3 points)
2. Weaknesses (2-3 points)
3. Specific suggestions for improvement

Content: {parameters.Content}" }
                    },
                    Temperature = 0.3
                });
                
                var critique = response.Choices?[0]?.Message?.Content ?? "";
                Console.WriteLine($"   Critique: {critique.Substring(0, Math.Min(150, critique.Length))}...");
                
                outputContent.AppendLine("\n--- STEP 2: Literary Critic ---");
                outputContent.AppendLine($"Critique:\n{critique}\n");
                
                return new CriticResult
                {
                    Critique = critique,
                    Model = "openai/gpt-4o-mini"
                };
            },
            requiresApproval: false
        );

        // Tool 3: Editor
        var callEditor = new Tool<EditorParams, EditorResult>(
            name: "call_editor",
            description: "Call an editor model to refine content based on critique",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["originalContent"] = JsonSchemaBuilder.String("Original content"),
                    ["critique"] = JsonSchemaBuilder.String("Critique to address")
                },
                required: new List<string> { "originalContent", "critique" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\nSTEP 3 - Editor (Turn {context?.NumberOfTurns ?? 0})");
                
                var response = await client.Chat.CreateAsync(new ChatCompletionRequest
                {
                    Model = "openai/gpt-4o-mini",
                    Messages = new List<Message>
                    {
                        new UserMessage { Content = $@"You are an editor. Revise this content based on the critique provided.

Original Content:
{parameters.OriginalContent}

Critique:
{parameters.Critique}

Provide the improved version." }
                    },
                    Temperature = 0.5
                });
                
                var revisedContent = response.Choices?[0]?.Message?.Content ?? "";
                Console.WriteLine($"   Revised: {revisedContent.Substring(0, Math.Min(100, revisedContent.Length))}...");
                
                outputContent.AppendLine("\n--- STEP 3: Editor (Final Version) ---");
                outputContent.AppendLine($"Refined Content:\n{revisedContent}\n");
                
                return new EditorResult
                {
                    RevisedContent = revisedContent,
                    Model = "openai/gpt-4o-mini"
                };
            },
            requiresApproval: false
        );

        // Orchestrate the chain
        var request = new BetaResponsesRequest
        {
            Model = "openai/gpt-4o-mini",
            Input = new List<ResponsesEasyInputMessage>
            {
                new()
                {
                    Role = "system",
                    Content = "You are a content production coordinator. Use the writer, critic, and editor tools in sequence to create polished content. Always call all three in order."
                },
                new()
                {
                    Role = "user",
                    Content = "Create a piece about 'The beauty of morning coffee' in a warm, reflective style. Use the writer to draft, critic to review, and editor to refine."
                }
            },
            Instructions = "You are a content production coordinator. Use the writer, critic, and editor tools in sequence to create polished content.",
            Temperature = 0.3,
            Store = false,
            ServiceTier = "auto"
        };

        var result = client.CallModel(request, new ITool[] { callCreativeWriter, callLiteraryCritic, callEditor });

        Console.WriteLine();
        await foreach (var textChunk in result.GetTextStreamAsync())
        {
            Console.Write(textChunk);
        }

        Console.WriteLine("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("Pipeline Complete!\n");
        
        // Write output to file in bin folder
        var binFolder = Path.Combine(AppContext.BaseDirectory);
        var outputFile = Path.Combine(binFolder, "Example22_CreativeWriting_Output.txt");
        await File.WriteAllTextAsync(outputFile, outputContent.ToString());
        Console.WriteLine($"Output saved to: {outputFile}\n");
    }

    /// <summary>
    /// Example 2: Multi-Model Consensus
    /// Query multiple specialized models and synthesize results
    /// </summary>
    private static async Task MultiModelConsensusExample()
    {
        Console.WriteLine("\n\nChained LLM Example 2: Multi-Model Consensus");
        Console.WriteLine("Query multiple specialized models and synthesize\n");

        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);
        var outputContent = new System.Text.StringBuilder();
        outputContent.AppendLine("=== Multi-Model Consensus - Generated Content ===");
        outputContent.AppendLine($"Timestamp: {DateTime.Now}\n");

        // Math specialist
        var askMathSpecialist = new Tool<QuestionParams, SpecialistResult>(
            name: "ask_math_specialist",
            description: "Ask a math-focused model for an answer",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["question"] = JsonSchemaBuilder.String("The question to ask")
                },
                required: new List<string> { "question" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\n🔢 Consulting Math Specialist...");
                
                var response = await client.Chat.CreateAsync(new ChatCompletionRequest
                {
                    Model = "openai/gpt-4o-mini",
                    Messages = new List<Message>
                    {
                        new SystemMessage { Content = "You are a mathematics expert. Provide precise, accurate answers." },
                        new UserMessage { Content = parameters.Question }
                    },
                    Temperature = 0.1
                });
                
                var answer = response.Choices?[0]?.Message?.Content ?? "";
                Console.WriteLine($"   Answer: {answer.Substring(0, Math.Min(100, answer.Length))}...");
                
                outputContent.AppendLine("\n--- Math Specialist ---");
                outputContent.AppendLine($"Question: {parameters.Question}");
                outputContent.AppendLine($"Answer:\n{answer}\n");
                
                return new SpecialistResult
                {
                    Answer = answer,
                    Specialist = "math",
                    Confidence = 0.95
                };
            });

        // Reasoning specialist
        var askReasoningSpecialist = new Tool<QuestionParams, SpecialistResult>(
            name: "ask_reasoning_specialist",
            description: "Ask a reasoning-focused model for an answer",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["question"] = JsonSchemaBuilder.String("The question to ask")
                },
                required: new List<string> { "question" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\n🧠 Consulting Reasoning Specialist...");
                
                var response = await client.Chat.CreateAsync(new ChatCompletionRequest
                {
                    Model = "openai/gpt-4o-mini",
                    Messages = new List<Message>
                    {
                        new UserMessage { Content = $"Think step by step to answer: {parameters.Question}" }
                    },
                    Temperature = 0.2
                });
                
                var answer = response.Choices?[0]?.Message?.Content ?? "";
                Console.WriteLine($"   Answer: {answer.Substring(0, Math.Min(100, answer.Length))}...");
                
                outputContent.AppendLine("\n--- Reasoning Specialist ---");
                outputContent.AppendLine($"Question: {parameters.Question}");
                outputContent.AppendLine($"Answer:\n{answer}\n");
                
                return new SpecialistResult
                {
                    Answer = answer,
                    Specialist = "reasoning",
                    Confidence = 0.90
                };
            });

        // General knowledge specialist
        var askGeneralKnowledge = new Tool<QuestionParams, SpecialistResult>(
            name: "ask_general_knowledge",
            description: "Ask a general knowledge model for an answer",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["question"] = JsonSchemaBuilder.String("The question to ask")
                },
                required: new List<string> { "question" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\n📚 Consulting General Knowledge Model...");
                
                var response = await client.Chat.CreateAsync(new ChatCompletionRequest
                {
                    Model = "openai/gpt-4o-mini",
                    Messages = new List<Message>
                    {
                        new UserMessage { Content = parameters.Question }
                    },
                    Temperature = 0.3
                });
                
                var answer = response.Choices?[0]?.Message?.Content ?? "";
                Console.WriteLine($"   Answer: {answer.Substring(0, Math.Min(100, answer.Length))}...");
                
                outputContent.AppendLine("\n--- General Knowledge Specialist ---");
                outputContent.AppendLine($"Question: {parameters.Question}");
                outputContent.AppendLine($"Answer:\n{answer}\n");
                
                return new SpecialistResult
                {
                    Answer = answer,
                    Specialist = "general",
                    Confidence = 0.85
                };
            });

        var request = new BetaResponsesRequest
        {
            Model = "openai/gpt-4o-mini",
            Input = new List<ResponsesEasyInputMessage>
            {
                new()
                {
                    Role = "user",
                    Content = @"You are a research coordinator. When asked a question:
1. Consult all three specialist models (math, reasoning, general knowledge)
2. Compare their answers
3. Synthesize the best answer based on their responses and confidence levels
4. Explain which specialist(s) you relied on most and why

Question: What is the best approach to solving the traveling salesman problem for 100 cities? Consult all specialists."
                }
            },
            Instructions = "You are a research coordinator. Use all three specialist tools to gather different perspectives, then synthesize their answers.",
            Temperature = 0.4,
            Store = false,
            ServiceTier = "auto"
        };

        Console.WriteLine("🔄 Orchestrating multi-model consensus...\n");

        var result = client.CallModel(request, new ITool[] { askMathSpecialist, askReasoningSpecialist, askGeneralKnowledge });

        Console.WriteLine();
        await foreach (var textChunk in result.GetTextStreamAsync())
        {
            Console.Write(textChunk);
        }

        Console.WriteLine("\n\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("Consensus Reached!\n");
        
        // Write output to file in bin folder
        var binFolder = Path.Combine(AppContext.BaseDirectory);
        var outputFile = Path.Combine(binFolder, "Example22_MultiModelConsensus_Output.txt");
        await File.WriteAllTextAsync(outputFile, outputContent.ToString());
        Console.WriteLine($"Output saved to: {outputFile}\n");
    }

    /// <summary>
    /// Example 3: Translation Chain
    /// Translate → Verify → Refine
    /// </summary>
    private static async Task TranslationChainExample()
    {
        Console.WriteLine("\n\nChained LLM Example 3: Translation Pipeline");
        Console.WriteLine("Translate -> Verify -> Refine\n");

        var apiKey = ExampleConfig.ApiKey;

        var client = new OpenRouterClient(apiKey);
        var outputContent = new System.Text.StringBuilder();
        outputContent.AppendLine("=== Translation Pipeline - Generated Content ===");
        outputContent.AppendLine($"Timestamp: {DateTime.Now}\n");

        // Translate tool
        var translateToFrench = new Tool<TranslateParams, TranslateResult>(
            name: "translate_to_french",
            description: "Translate text to French using a translation-specialized model",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["text"] = JsonSchemaBuilder.String("Text to translate")
                },
                required: new List<string> { "text" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\n🌍 Translating to French...");
                
                var response = await client.Chat.CreateAsync(new ChatCompletionRequest
                {
                    Model = "openai/gpt-4o-mini",
                    Messages = new List<Message>
                    {
                        new SystemMessage { Content = "You are a professional translator. Translate the following English text to French. Provide ONLY the translation." },
                        new UserMessage { Content = parameters.Text }
                    },
                    Temperature = 0.3
                });
                
                var translation = response.Choices?[0]?.Message?.Content ?? "";
                Console.WriteLine($"   Translation: {translation}");
                
                outputContent.AppendLine("\n--- STEP 1: Translation ---");
                outputContent.AppendLine($"Original Text: {parameters.Text}");
                outputContent.AppendLine($"French Translation:\n{translation}\n");
                
                return new TranslateResult
                {
                    Translation = translation,
                    SourceLanguage = "English",
                    TargetLanguage = "French"
                };
            });

        // Verify tool
        var verifyTranslation = new Tool<VerifyParams, VerifyResult>(
            name: "verify_translation",
            description: "Verify translation accuracy by translating back",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["original"] = JsonSchemaBuilder.String("Original text"),
                    ["translation"] = JsonSchemaBuilder.String("Translated text")
                },
                required: new List<string> { "original", "translation" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\n✓ Verifying translation...");
                
                var response = await client.Chat.CreateAsync(new ChatCompletionRequest
                {
                    Model = "openai/gpt-4o-mini",
                    Messages = new List<Message>
                    {
                        new UserMessage { Content = $"Translate this French text back to English: {parameters.Translation}" }
                    },
                    Temperature = 0.2
                });
                
                var backTranslation = response.Choices?[0]?.Message?.Content ?? "";
                Console.WriteLine($"   Back-translation: {backTranslation}");
                
                outputContent.AppendLine("\n--- STEP 2: Verification (Back-Translation) ---");
                outputContent.AppendLine($"French Text: {parameters.Translation}");
                outputContent.AppendLine($"Back to English:\n{backTranslation}\n");
                
                // Simple similarity check
                var similarity = backTranslation.ToLower().Contains(parameters.Original.ToLower().Split(' ')[0]) ? 0.9 : 0.7;
                
                return new VerifyResult
                {
                    BackTranslation = backTranslation,
                    SimilarityScore = similarity,
                    Accurate = similarity > 0.8
                };
            });

        // Refine tool
        var refineTranslation = new Tool<RefineParams, RefineResult>(
            name: "refine_translation",
            description: "Refine translation for naturalness and cultural context",
            inputSchema: JsonSchemaBuilder.CreateObjectSchema(
                new Dictionary<string, object>
                {
                    ["translation"] = JsonSchemaBuilder.String("Translation to refine")
                },
                required: new List<string> { "translation" }
            ),
            executeFunc: async (parameters, context) =>
            {
                Console.WriteLine($"\nRefining translation for naturalness...");
                
                var response = await client.Chat.CreateAsync(new ChatCompletionRequest
                {
                    Model = "openai/gpt-4o-mini",
                    Messages = new List<Message>
                    {
                        new UserMessage { Content = $"Improve this French translation to make it more natural and idiomatic while preserving the meaning: {parameters.Translation}" }
                    },
                    Temperature = 0.4
                });
                
                var refined = response.Choices?[0]?.Message?.Content ?? "";
                Console.WriteLine($"   Refined: {refined}");
                
                outputContent.AppendLine("\n--- STEP 3: Refinement (Final Version) ---");
                outputContent.AppendLine($"Original Translation: {parameters.Translation}");
                outputContent.AppendLine($"Refined Translation:\n{refined}\n");
                
                return new RefineResult { RefinedTranslation = refined };
            });

        var request = new BetaResponsesRequest
        {
            Model = "openai/gpt-4o-mini",
            Input = new List<ResponsesEasyInputMessage>
            {
                new()
                {
                    Role = "user",
                    Content = "Translate this text to French, verify accuracy, and refine for naturalness. Use all three tools in sequence: 'The quick brown fox jumps over the lazy dog.'"
                }
            },
            Instructions = "You are a translation coordinator. Use the translate, verify, and refine tools in sequence.",
            Store = false,
            ServiceTier = "auto"
        };

        var result = client.CallModel(request, new ITool[] { translateToFrench, verifyTranslation, refineTranslation });

        Console.WriteLine();
        await foreach (var textChunk in result.GetTextStreamAsync())
        {
            Console.Write(textChunk);
        }

        Console.WriteLine("\n\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("Translation Chain Complete!\n");
        
        // Write output to file in bin folder
        var binFolder = Path.Combine(AppContext.BaseDirectory);
        var outputFile = Path.Combine(binFolder, "Example22_Translation_Output.txt");
        await File.WriteAllTextAsync(outputFile, outputContent.ToString());
        Console.WriteLine($"Output saved to: {outputFile}\n");
    }

    // Supporting classes for all examples
    private class WriterParams { public string Prompt { get; set; } = ""; public string Style { get; set; } = ""; }
    private class WriterResult { public string Content { get; set; } = ""; public int WordCount { get; set; } public string Model { get; set; } = ""; }
    private class CriticParams { public string Content { get; set; } = ""; }
    private class CriticResult { public string Critique { get; set; } = ""; public string Model { get; set; } = ""; }
    private class EditorParams { public string OriginalContent { get; set; } = ""; public string Critique { get; set; } = ""; }
    private class EditorResult { public string RevisedContent { get; set; } = ""; public string Model { get; set; } = ""; }
    private class QuestionParams { public string Question { get; set; } = ""; }
    private class SpecialistResult { public string Answer { get; set; } = ""; public string Specialist { get; set; } = ""; public double Confidence { get; set; } }
    private class TranslateParams { public string Text { get; set; } = ""; }
    private class TranslateResult { public string Translation { get; set; } = ""; public string SourceLanguage { get; set; } = ""; public string TargetLanguage { get; set; } = ""; }
    private class VerifyParams { public string Original { get; set; } = ""; public string Translation { get; set; } = ""; }
    private class VerifyResult { public string BackTranslation { get; set; } = ""; public double SimilarityScore { get; set; } public bool Accurate { get; set; } }
    private class RefineParams { public string Translation { get; set; } = ""; }
    private class RefineResult { public string RefinedTranslation { get; set; } = ""; }
}



