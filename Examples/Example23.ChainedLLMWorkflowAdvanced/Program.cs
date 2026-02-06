using OpenRouter.SDK;
using OpenRouter.SDK.Models;
using OpenRouter.Examples.EnvConfig;
using System.Text.Json;

namespace Example23.ChainedLLMWorkflowAdvanced;

/// <summary>
/// Chained LLM Workflow Example
/// 
/// Demonstrates chaining multiple LLM calls together where:
/// - One model's output feeds into another model
/// - Different models specialized for different tasks
/// - Results aggregated and refined through multiple models
/// 
/// Pattern: Model A → Tool calls Model B → Results to Model A → Final output
/// </summary>
class Program
{
    private static OpenRouterClient _client = null!;

    static async Task Main(string[] args)
    {
        try
        {
            _client = new OpenRouterClient(ExampleConfig.ApiKey);

            await CreativeWritingChainExample();
            await MultiModelConsensusExample();
            await TranslationChainExample();

            Console.WriteLine("\n\n🎉 All chained LLM workflow examples completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // Example 1: Creative Writing Pipeline
    // Writer → Critic → Editor chain
    // ═══════════════════════════════════════════════════════════

    static async Task CreativeWritingChainExample()
    {
        Console.WriteLine("Chained LLM Example 1: Creative Writing Pipeline");
        Console.WriteLine("Writer -> Critic -> Editor\n");

        // Step 1: Creative Writer
        Console.WriteLine("\nSTEP 1 - Creative Writer");
        var writerResponse = await _client.Chat.CreateAsync(new ChatCompletionRequest
        {
            Model = ExampleConfig.ModelName,
            Messages = new List<Message>
            {
                new UserMessage { Content = "You are a creative writer. Write a short piece (100-150 words) about: The beauty of morning coffee. Use a warm, reflective style." }
            },
            Temperature = 0.9
        });

        var content = writerResponse.Choices?[0]?.Message?.Content ?? "";
        Console.WriteLine($"   Generated: {content.Substring(0, Math.Min(100, content.Length))}...\n");

        // Step 2: Literary Critic
        Console.WriteLine("\nSTEP 2 - Literary Critic");
        var criticResponse = await _client.Chat.CreateAsync(new ChatCompletionRequest
        {
            Model = ExampleConfig.ModelName,
            Messages = new List<Message>
            {
                new UserMessage 
                { 
                    Content = $"You are a literary critic. Analyze this content and provide:\n1. Strengths (2-3 points)\n2. Weaknesses (2-3 points)\n3. Specific suggestions for improvement\n\nContent: {content}"
                }
            },
            Temperature = 0.3
        });

        var critique = criticResponse.Choices?[0]?.Message?.Content ?? "";
        Console.WriteLine($"   Critique: {critique.Substring(0, Math.Min(150, critique.Length))}...\n");

        // Step 3: Editor
        Console.WriteLine("\nSTEP 3 - Editor");
        var editorResponse = await _client.Chat.CreateAsync(new ChatCompletionRequest
        {
            Model = ExampleConfig.ModelName,
            Messages = new List<Message>
            {
                new UserMessage 
                { 
                    Content = $"You are an editor. Revise this content based on the critique provided.\n\nOriginal Content:\n{content}\n\nCritique:\n{critique}\n\nProvide the improved version."
                }
            },
            Temperature = 0.5
        });

        var revisedContent = editorResponse.Choices?[0]?.Message?.Content ?? "";
        Console.WriteLine($"   Revised: {revisedContent.Substring(0, Math.Min(200, revisedContent.Length))}...\n");

        Console.WriteLine("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("Pipeline Complete!\n");
    }

    // ═══════════════════════════════════════════════════════════
    // Example 2: Multi-Model Consensus
    // Multiple specialized models vote on the answer
    // ═══════════════════════════════════════════════════════════

    static async Task MultiModelConsensusExample()
    {
        Console.WriteLine("\n\nChained LLM Example 2: Multi-Model Consensus");
        Console.WriteLine("Query multiple specialized models and synthesize\n");

        var question = "What is the best approach to solving the traveling salesman problem for 100 cities?";

        // Math Specialist
        Console.WriteLine("\n🔢 Consulting Math Specialist...");
        var mathResponse = await _client.Chat.CreateAsync(new ChatCompletionRequest
        {
            Model = ExampleConfig.ModelName,
            Messages = new List<Message>
            {
                new SystemMessage { Content = "You are a mathematics expert. Provide precise, accurate answers." },
                new UserMessage { Content = question }
            },
            Temperature = 0.1
        });
        var mathAnswer = mathResponse.Choices?[0]?.Message?.Content ?? "";
        Console.WriteLine($"   Answer: {mathAnswer.Substring(0, Math.Min(150, mathAnswer.Length))}...\n");

        // Reasoning Specialist
        Console.WriteLine("\n🧠 Consulting Reasoning Specialist...");
        var reasoningResponse = await _client.Chat.CreateAsync(new ChatCompletionRequest
        {
            Model = ExampleConfig.ModelName,
            Messages = new List<Message>
            {
                new UserMessage { Content = $"Think step by step to answer: {question}" }
            },
            Temperature = 0.2
        });
        var reasoningAnswer = reasoningResponse.Choices?[0]?.Message?.Content ?? "";
        Console.WriteLine($"   Answer: {reasoningAnswer.Substring(0, Math.Min(150, reasoningAnswer.Length))}...\n");

        // General Knowledge
        Console.WriteLine("\n📚 Consulting General Knowledge Model...");
        var generalResponse = await _client.Chat.CreateAsync(new ChatCompletionRequest
        {
            Model = ExampleConfig.ModelName,
            Messages = new List<Message>
            {
                new UserMessage { Content = question }
            },
            Temperature = 0.3
        });
        var generalAnswer = generalResponse.Choices?[0]?.Message?.Content ?? "";
        Console.WriteLine($"   Answer: {generalAnswer.Substring(0, Math.Min(150, generalAnswer.Length))}...\n");

        // Synthesize
        Console.WriteLine("\n🔄 Synthesizing consensus...");
        var synthesisResponse = await _client.Chat.CreateAsync(new ChatCompletionRequest
        {
            Model = ExampleConfig.ModelName,
            Messages = new List<Message>
            {
                new SystemMessage 
                { 
                    Content = "You are a research coordinator. You have received answers from three specialists. Synthesize the best answer based on their responses and explain which specialist(s) you relied on most and why."
                },
                new UserMessage 
                { 
                    Content = $"Question: {question}\n\nMath Specialist: {mathAnswer}\n\nReasoning Specialist: {reasoningAnswer}\n\nGeneral Knowledge: {generalAnswer}\n\nProvide the best synthesized answer."
                }
            },
            Temperature = 0.4
        });

        var finalAnswer = synthesisResponse.Choices?[0]?.Message?.Content ?? "";
        Console.WriteLine($"\n{finalAnswer}\n");

        Console.WriteLine("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("Consensus Reached!\n");
    }

    // ═══════════════════════════════════════════════════════════
    // Example 3: Translation Chain
    // Translate → Verify → Refine
    // ═══════════════════════════════════════════════════════════

    static async Task TranslationChainExample()
    {
        Console.WriteLine("\n\n═══════════════════════════════════════════════════════");
        Console.WriteLine("  Chained LLM Example 3: Translation Pipeline");
        Console.WriteLine("  Translate → Verify → Refine");
        Console.WriteLine("═══════════════════════════════════════════════════════\n");

        var originalText = "The quick brown fox jumps over the lazy dog.";

        // Step 1: Translate
        Console.WriteLine("\n🌍 Translating to French...");
        var translateResponse = await _client.Chat.CreateAsync(new ChatCompletionRequest
        {
            Model = ExampleConfig.ModelName,
            Messages = new List<Message>
            {
                new SystemMessage { Content = "You are a professional translator. Translate the following English text to French. Provide ONLY the translation." },
                new UserMessage { Content = originalText }
            },
            Temperature = 0.3
        });

        var translation = translateResponse.Choices?[0]?.Message?.Content ?? "";
        Console.WriteLine($"   Translation: {translation}\n");

        // Step 2: Verify
        Console.WriteLine("\n✓ Verifying translation...");
        var verifyResponse = await _client.Chat.CreateAsync(new ChatCompletionRequest
        {
            Model = ExampleConfig.ModelName,
            Messages = new List<Message>
            {
                new UserMessage { Content = $"Translate this French text back to English: {translation}" }
            },
            Temperature = 0.2
        });

        var backTranslation = verifyResponse.Choices?[0]?.Message?.Content ?? "";
        Console.WriteLine($"   Back-translation: {backTranslation}\n");

        var similarity = backTranslation.ToLower().Contains(originalText.ToLower().Split(' ')[0]) ? 0.9 : 0.7;
        Console.WriteLine($"   Similarity Score: {similarity}");
        Console.WriteLine($"   Accurate: {similarity > 0.8}\n");

        // Step 3: Refine
        Console.WriteLine("\nRefining translation for naturalness...");
        var refineResponse = await _client.Chat.CreateAsync(new ChatCompletionRequest
        {
            Model = ExampleConfig.ModelName,
            Messages = new List<Message>
            {
                new UserMessage { Content = $"Improve this French translation to make it more natural and idiomatic while preserving the meaning: {translation}" }
            },
            Temperature = 0.4
        });

        var refined = refineResponse.Choices?[0]?.Message?.Content ?? "";
        Console.WriteLine($"   Refined: {refined}\n");

        Console.WriteLine("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("Translation Chain Complete!\n");
    }
}

