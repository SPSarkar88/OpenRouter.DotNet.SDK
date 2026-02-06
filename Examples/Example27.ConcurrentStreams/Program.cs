using OpenRouter.SDK;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;
using OpenRouter.Examples.EnvConfig;
using System.Diagnostics;

var apiKey = ExampleConfig.ApiKey;
var modelName = ExampleConfig.ModelName;

var client = new OpenRouterClient(apiKey);

Console.WriteLine("========================================");
Console.WriteLine("OpenRouter SDK - Concurrent Stream Consumers");
Console.WriteLine("========================================\n");

await Example1_ConcurrentTextAndFullStream();
await Example2_MultipleTextConsumers();
await Example3_TextAndReasoningConcurrently();
await Example4_PerformanceComparison();

Console.WriteLine("\n========================================");
Console.WriteLine("All examples completed!");
Console.WriteLine("========================================");

async Task Example1_ConcurrentTextAndFullStream()
{
    Console.WriteLine("\n📌 Example 1: Concurrent Text and Full Stream Consumers");
    Console.WriteLine("────────────────────────────────────────────────────────");
    Console.WriteLine("Demonstrates consuming text stream and full stream simultaneously\n");

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "Write a short poem about coding (2 lines)",
        Stream = true
    };

    var result = new ModelResult(client.Beta.Responses, request);

    // Start both consumers concurrently
    var textTask = Task.Run(async () =>
    {
        Console.WriteLine("📝 Text Consumer started...");
        var text = new System.Text.StringBuilder();
        
        await foreach (var chunk in result.GetTextStreamAsync())
        {
            text.Append(chunk);
            Console.Write(".");
        }
        
        Console.WriteLine($"\n📝 Text Consumer complete: {text.Length} characters");
        return text.ToString();
    });

    var fullStreamTask = Task.Run(async () =>
    {
        Console.WriteLine("🔄 Full Stream Consumer started...");
        int eventCount = 0;
        
        await foreach (var chunk in result.GetFullStreamAsync())
        {
            eventCount++;
        }
        
        Console.WriteLine($"🔄 Full Stream Consumer complete: {eventCount} events");
        return eventCount;
    });

    // Wait for both to complete
    await Task.WhenAll(textTask, fullStreamTask);
    
    Console.WriteLine($"\n✅ Both consumers completed successfully!");
    Console.WriteLine($"   Text length: {textTask.Result.Length} chars");
    Console.WriteLine($"   Total events: {fullStreamTask.Result}");
}

async Task Example2_MultipleTextConsumers()
{
    Console.WriteLine("\n📌 Example 2: Multiple Text Stream Consumers");
    Console.WriteLine("───────────────────────────────────────────────");
    Console.WriteLine("Three independent consumers reading the same text stream\n");

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "Count from 1 to 5",
        Stream = true
    };

    var result = new ModelResult(client.Beta.Responses, request);

    // Create three consumers with different processing
    var consumer1 = Task.Run(async () =>
    {
        var text = new System.Text.StringBuilder();
        await foreach (var chunk in result.GetTextStreamAsync())
        {
            text.Append(chunk);
        }
        Console.WriteLine($"Consumer 1 collected: {text.Length} chars");
        return text.ToString();
    });

    var consumer2 = Task.Run(async () =>
    {
        var chunkCount = 0;
        await foreach (var chunk in result.GetTextStreamAsync())
        {
            chunkCount++;
        }
        Console.WriteLine($"Consumer 2 counted: {chunkCount} chunks");
        return chunkCount;
    });

    var consumer3 = Task.Run(async () =>
    {
        var words = new List<string>();
        var currentWord = new System.Text.StringBuilder();
        
        await foreach (var chunk in result.GetTextStreamAsync())
        {
            foreach (var c in chunk)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (currentWord.Length > 0)
                    {
                        words.Add(currentWord.ToString());
                        currentWord.Clear();
                    }
                }
                else
                {
                    currentWord.Append(c);
                }
            }
        }
        
        if (currentWord.Length > 0)
        {
            words.Add(currentWord.ToString());
        }
        
        Console.WriteLine($"Consumer 3 extracted: {words.Count} words");
        return words.Count;
    });

    await Task.WhenAll(consumer1, consumer2, consumer3);
    
    Console.WriteLine($"\n✅ All three consumers completed independently!");
    Console.WriteLine($"   Full text: \"{consumer1.Result}\"");
    Console.WriteLine($"   Chunk count: {consumer2.Result}");
    Console.WriteLine($"   Word count: {consumer3.Result}");
}

async Task Example3_TextAndReasoningConcurrently()
{
    Console.WriteLine("\n📌 Example 3: Text and Reasoning Streams Concurrently");
    Console.WriteLine("──────────────────────────────────────────────────────");
    Console.WriteLine("For models that support reasoning (like o1), consume both streams\n");

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "What is 2+2?",
        Stream = true
    };

    var result = new ModelResult(client.Beta.Responses, request);

    var textTask = Task.Run(async () =>
    {
        Console.WriteLine("📝 Collecting text...");
        var text = new System.Text.StringBuilder();
        
        await foreach (var chunk in result.GetTextStreamAsync())
        {
            text.Append(chunk);
        }
        
        return text.ToString();
    });

    var reasoningTask = Task.Run(async () =>
    {
        Console.WriteLine("🧠 Collecting reasoning...");
        var reasoning = new System.Text.StringBuilder();
        
        await foreach (var chunk in result.GetReasoningStreamAsync())
        {
            reasoning.Append(chunk);
        }
        
        return reasoning.ToString();
    });

    await Task.WhenAll(textTask, reasoningTask);
    
    Console.WriteLine($"\n✅ Both streams consumed concurrently!");
    Console.WriteLine($"   Text: \"{textTask.Result}\"");
    Console.WriteLine($"   Reasoning: \"{(string.IsNullOrEmpty(reasoningTask.Result) ? "(none)" : reasoningTask.Result)}\"");
}

async Task Example4_PerformanceComparison()
{
    Console.WriteLine("\n📌 Example 4: Performance - Sequential vs Concurrent");
    Console.WriteLine("───────────────────────────────────────────────────");
    Console.WriteLine("Comparing sequential vs concurrent stream consumption\n");

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "Write three sentences about parallel processing",
        Stream = true
    };

    // Sequential approach (traditional)
    Console.WriteLine("🔄 Sequential consumption (traditional approach):");
    var sw1 = Stopwatch.StartNew();
    
    var result1 = new ModelResult(client.Beta.Responses, request);
    
    var text1 = new System.Text.StringBuilder();
    await foreach (var chunk in result1.GetTextStreamAsync())
    {
        text1.Append(chunk);
        await Task.Delay(1); // Simulate processing
    }
    
    var fullStream1 = new List<object>();
    await foreach (var chunk in result1.GetFullStreamAsync())
    {
        fullStream1.Add(chunk);
        await Task.Delay(1); // Simulate processing
    }
    
    sw1.Stop();
    Console.WriteLine($"   Time: {sw1.ElapsedMilliseconds}ms");

    // Concurrent approach (new ReusableStream)
    Console.WriteLine("\n⚡ Concurrent consumption (ReusableStream):");
    var sw2 = Stopwatch.StartNew();
    
    var result2 = new ModelResult(client.Beta.Responses, request);
    
    var textTask2 = Task.Run(async () =>
    {
        var text = new System.Text.StringBuilder();
        await foreach (var chunk in result2.GetTextStreamAsync())
        {
            text.Append(chunk);
            await Task.Delay(1); // Simulate processing
        }
        return text.ToString();
    });

    var fullStreamTask2 = Task.Run(async () =>
    {
        var items = new List<object>();
        await foreach (var chunk in result2.GetFullStreamAsync())
        {
            items.Add(chunk);
            await Task.Delay(1); // Simulate processing
        }
        return items.Count;
    });

    await Task.WhenAll(textTask2, fullStreamTask2);
    
    sw2.Stop();
    Console.WriteLine($"   Time: {sw2.ElapsedMilliseconds}ms");
    
    Console.WriteLine($"\n✅ Performance comparison:");
    Console.WriteLine($"   Sequential: {sw1.ElapsedMilliseconds}ms");
    Console.WriteLine($"   Concurrent: {sw2.ElapsedMilliseconds}ms");
    Console.WriteLine($"   Speedup: ~{(double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds:F2}x");
}
