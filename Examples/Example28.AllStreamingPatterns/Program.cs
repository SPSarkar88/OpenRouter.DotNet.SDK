using OpenRouter.SDK;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;
using OpenRouter.Examples.EnvConfig;

var apiKey = ExampleConfig.ApiKey;
var modelName = ExampleConfig.ModelName;

var client = new OpenRouterClient(apiKey);

Console.WriteLine("========================================");
Console.WriteLine("OpenRouter SDK - All Streaming Patterns");
Console.WriteLine("========================================\n");

await Example1_MultipleTextConsumers();
await Example2_ReasoningStream();
await Example3_AllStreamsConcurrently();

Console.WriteLine("\n========================================");
Console.WriteLine("All examples completed!");
Console.WriteLine("========================================");

async Task Example1_MultipleTextConsumers()
{
    Console.WriteLine("\n📌 Example 1: Multiple Text Stream Consumers");
    Console.WriteLine("───────────────────────────────────────────────");
    Console.WriteLine("Three different consumers reading the same text stream\n");

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "Write a haiku about programming",
        Stream = true
    };

    var result = new ModelResult(client.Beta.Responses, request);

    // Consumer 1: Collect full text
    var textTask = Task.Run(async () =>
    {
        var text = new System.Text.StringBuilder();
        await foreach (var chunk in result.GetTextStreamAsync())
        {
            text.Append(chunk);
            Console.Write(".");
        }
        Console.WriteLine($"\n✅ Text Consumer: {text.Length} chars");
        return text.ToString();
    });

    // Consumer 2: Count chunks
    var chunkTask = Task.Run(async () =>
    {
        int count = 0;
        await foreach (var chunk in result.GetTextStreamAsync())
        {
            count++;
        }
        Console.WriteLine($"✅ Chunk Counter: {count} chunks");
        return count;
    });

    // Consumer 3: Process full stream events
    var eventTask = Task.Run(async () =>
    {
        int eventCount = 0;
        await foreach (var chunk in result.GetFullStreamAsync())
        {
            eventCount++;
        }
        Console.WriteLine($"✅ Event Consumer: {eventCount} events");
        return eventCount;
    });

    await Task.WhenAll(textTask, chunkTask, eventTask);

    Console.WriteLine($"\n📊 Results:");
    Console.WriteLine($"   Text: \"{textTask.Result}\"");
    Console.WriteLine($"   Chunks: {chunkTask.Result}");
    Console.WriteLine($"   Events: {eventTask.Result}");
}

async Task Example2_ReasoningStream()
{
    Console.WriteLine("\n📌 Example 2: Reasoning Stream (o1 models)");
    Console.WriteLine("────────────────────────────────────────────────");
    Console.WriteLine("Consume text and reasoning streams concurrently\n");

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "Solve: If x + 5 = 12, what is x?",
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

    Console.WriteLine($"\n✅ Text: \"{textTask.Result}\"");
    Console.WriteLine($"✅ Reasoning: {(string.IsNullOrEmpty(reasoningTask.Result) ? "(not available for this model)" : $"\"{reasoningTask.Result}\"")}");
}

async Task Example3_AllStreamsConcurrently()
{
    Console.WriteLine("\n📌 Example 3: All Streams Concurrently");
    Console.WriteLine("────────────────────────────────────────");
    Console.WriteLine("Demonstrate all available stream types\n");

    var request = new BetaResponsesRequest
    {
        Model = modelName,
        Input = "List 3 benefits of concurrent programming",
        Stream = true
    };

    var result = new ModelResult(client.Beta.Responses, request);

    // Start all consumers concurrently
    var consumers = new[]
    {
        Task.Run(async () =>
        {
            var text = new System.Text.StringBuilder();
            await foreach (var chunk in result.GetTextStreamAsync())
            {
                text.Append(chunk);
            }
            Console.WriteLine($"✅ GetTextStreamAsync: {text.Length} chars");
        }),

        Task.Run(async () =>
        {
            int count = 0;
            await foreach (var chunk in result.GetReasoningStreamAsync())
            {
                count++;
            }
            Console.WriteLine($"✅ GetReasoningStreamAsync: {count} chunks");
        }),

        Task.Run(async () =>
        {
            int count = 0;
            await foreach (var chunk in result.GetToolCallsStreamAsync())
            {
                count++;
            }
            Console.WriteLine($"✅ GetToolCallsStreamAsync: {count} calls");
        }),

        Task.Run(async () =>
        {
            int count = 0;
            await foreach (var chunk in result.GetToolStreamAsync())
            {
                count++;
            }
            Console.WriteLine($"✅ GetToolStreamAsync: {count} results");
        }),

        Task.Run(async () =>
        {
            int count = 0;
            await foreach (var message in result.GetNewMessagesStreamAsync())
            {
                count++;
            }
            Console.WriteLine($"✅ GetNewMessagesStreamAsync: {count} messages");
        }),

        Task.Run(async () =>
        {
            int count = 0;
            await foreach (var response in result.GetFullResponsesStreamAsync())
            {
                count++;
            }
            Console.WriteLine($"✅ GetFullResponsesStreamAsync: {count} responses");
        }),

        Task.Run(async () =>
        {
            int count = 0;
            await foreach (var chunk in result.GetFullStreamAsync())
            {
                count++;
            }
            Console.WriteLine($"✅ GetFullStreamAsync: {count} events");
        })
    };

    await Task.WhenAll(consumers);

    Console.WriteLine($"\n🎉 All 7 stream types consumed concurrently!");
}
