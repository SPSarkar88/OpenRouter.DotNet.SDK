# Reusable Stream (Concurrent Consumption)

The **ReusableStream** feature enables multiple independent consumers to read from the same streaming response concurrently. This matches the TypeScript SDK's `ReusableReadableStream` capability and allows efficient parallel processing of streaming data.

## Overview

When you receive a streaming response from the OpenRouter API, you might want to:
- Extract text content **and** count events simultaneously
- Process tool calls **while** streaming to the user
- Analyze reasoning content **alongside** response metadata

Without ReusableStream, you would need to buffer the entire response yourself or make separate API calls. With ReusableStream, create multiple independent consumers that all read from a single underlying stream.

## Basic Usage

```csharp
using OpenRouter.SDK;
using OpenRouter.SDK.Models;
using OpenRouter.SDK.Services;

var client = new OpenRouterClient("YOUR_API_KEY");

var request = new BetaResponsesRequest
{
    Model = "openai/gpt-4",
    Input = "Write a short story about AI"
};

var result = new ModelResult(client.Beta.Responses, request);

// Create two concurrent consumers
var textTask = Task.Run(async () =>
{
    var text = new StringBuilder();
    await foreach (var chunk in result.GetTextStreamAsync())
    {
        text.Append(chunk);
    }
    return text.ToString();
});

var eventCountTask = Task.Run(async () =>
{
    int count = 0;
    await foreach (var _ in result.GetFullStreamAsync())
    {
        count++;
    }
    return count;
});

// Wait for both to complete
await Task.WhenAll(textTask, eventCountTask);

Console.WriteLine($"Story: {textTask.Result}");
Console.WriteLine($"Total events: {eventCountTask.Result}");
```

## Available Stream Methods

### `GetTextStreamAsync()`
Extracts only text deltas from the response stream.

```csharp
await foreach (var textChunk in result.GetTextStreamAsync())
{
    Console.Write(textChunk);
}
```

### `GetReasoningStreamAsync()`
Extracts reasoning content (for models like o1 that support chain-of-thought).

```csharp
await foreach (var reasoning in result.GetReasoningStreamAsync())
{
    Console.WriteLine($"Thinking: {reasoning}");
}
```

### `GetToolCallsStreamAsync()`
Extracts tool call events from the stream.

```csharp
await foreach (var toolCall in result.GetToolCallsStreamAsync())
{
    Console.WriteLine($"Tool: {toolCall.Function.Name}");
}
```

### `GetFullStreamAsync()`
Returns all raw stream chunks without filtering.

```csharp
await foreach (var chunk in result.GetFullStreamAsync())
{
    // Process full chunk
    if (chunk.Usage != null)
        Console.WriteLine($"Tokens: {chunk.Usage.TotalTokens}");
}
```

## Advanced Patterns

### Three Independent Consumers

```csharp
var result = new ModelResult(client.Beta.Responses, request);

// Consumer 1: Collect full text
var textConsumer = Task.Run(async () =>
{
    var sb = new StringBuilder();
    await foreach (var chunk in result.GetTextStreamAsync())
        sb.Append(chunk);
    return sb.ToString();
});

// Consumer 2: Count chunks
var countConsumer = Task.Run(async () =>
{
    int count = 0;
    await foreach (var _ in result.GetTextStreamAsync())
        count++;
    return count;
});

// Consumer 3: Extract words
var wordConsumer = Task.Run(async () =>
{
    var words = new List<string>();
    var currentWord = new StringBuilder();
    
    await foreach (var chunk in result.GetTextStreamAsync())
    {
        foreach (var ch in chunk)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (currentWord.Length > 0)
                {
                    words.Add(currentWord.ToString());
                    currentWord.Clear();
                }
            }
            else
            {
                currentWord.Append(ch);
            }
        }
    }
    
    if (currentWord.Length > 0)
        words.Add(currentWord.ToString());
    
    return words;
});

await Task.WhenAll(textConsumer, countConsumer, wordConsumer);

Console.WriteLine($"Full Text: {textConsumer.Result}");
Console.WriteLine($"Chunk Count: {countConsumer.Result}");
Console.WriteLine($"Word Count: {wordConsumer.Result.Count}");
```

### Text and Reasoning Concurrently (for o1 models)

```csharp
var request = new BetaResponsesRequest
{
    Model = "openai/o1-preview",
    Input = "Solve this puzzle: What is the next number in the sequence 2, 4, 8, 16...?"
};

var result = new ModelResult(client.Beta.Responses, request);

var textTask = Task.Run(async () =>
{
    var text = new StringBuilder();
    await foreach (var chunk in result.GetTextStreamAsync())
    {
        text.Append(chunk);
        Console.Write(".");
    }
    return text.ToString();
});

var reasoningTask = Task.Run(async () =>
{
    var reasoning = new StringBuilder();
    await foreach (var chunk in result.GetReasoningStreamAsync())
    {
        reasoning.Append(chunk);
        Console.Write("*");
    }
    return reasoning.ToString();
});

await Task.WhenAll(textTask, reasoningTask);

Console.WriteLine($"\nAnswer: {textTask.Result}");
Console.WriteLine($"Reasoning: {reasoningTask.Result}");
```

## How It Works

### Architecture

The `ReusableStream<T>` class:
1. **Wraps** a single `IAsyncEnumerable<T>` source stream
2. **Buffers** items as they're consumed from the source
3. **Tracks** multiple independent consumer positions
4. **Shares** buffered data across all consumers
5. **Pumps** source stream lazily on first consumer

### Thread Safety

- Uses `SemaphoreSlim` for lazy stream initialization
- Uses `ConcurrentDictionary` for tracking consumer states
- Locks buffer access during append operations
- Each consumer maintains independent position and wait state

### Memory Management

- Buffer grows only as far as slowest consumer needs
- Completed consumers are removed from tracking
- Source stream is disposed when all consumers complete
- Buffer can be cleared once all consumers catch up

### Performance Characteristics

**Advantages:**
- ✅ Single API call serves multiple consumers
- ✅ Concurrent processing improves throughput
- ✅ Independent consumer pacing (fast consumers don't wait for slow ones)
- ✅ Thread-safe without excessive locking

**Considerations:**
- ⚠️ Buffer memory grows with stream length
- ⚠️ Slowest consumer determines minimum buffer retention
- ⚠️ Each consumer adds slight overhead for tracking

## Comparison with TypeScript SDK

The C# implementation provides equivalent functionality to TypeScript's `ReusableReadableStream`:

| Feature | TypeScript | C# |
|---------|-----------|-----|
| **Multiple Consumers** | ✅ `stream.tee()` | ✅ `CreateConsumer()` |
| **Independent Iteration** | ✅ Each reader | ✅ Each `IAsyncEnumerable` |
| **Buffering** | ✅ Internal buffer | ✅ `List<T>` buffer |
| **Lazy Pump** | ✅ On first read | ✅ On first consumer |
| **Thread Safety** | ➖ Single-threaded JS | ✅ Full multi-threading |
| **Consumer Cleanup** | ✅ Auto cleanup | ✅ Auto cleanup |

### C# Specific Enhancements

1. **Type Safety**: Full generic typing with `ReusableStream<BetaResponsesStreamChunk>`
2. **Async Patterns**: Native `IAsyncEnumerable<T>` instead of async iterators
3. **Multi-threading**: True concurrent execution across CPU cores
4. **Cancellation**: Built-in `CancellationToken` support
5. **Extension Methods**: Convenient helpers on `ModelResult`

## Examples

See [Example27.ConcurrentStreams](../Examples/Example27.ConcurrentStreams/) for comprehensive examples:

- **Example 1**: Concurrent text and full stream consumption
- **Example 2**: Three independent text stream consumers
- **Example 3**: Text and reasoning streams (o1 models)
- **Example 4**: Performance comparison (sequential vs concurrent)

## Best Practices

### ✅ Do

- Create consumers before starting iteration
- Use `Task.WhenAll` to wait for all consumers
- Consider memory constraints for long streams
- Use appropriate stream methods (`GetTextStreamAsync` vs `GetFullStreamAsync`)

### ❌ Don't

- Create thousands of consumers (overhead adds up)
- Hold consumers open indefinitely (prevents buffer cleanup)
- Mix sync and async iteration patterns
- Assume ordering between consumers (they're independent)

## Error Handling

All consumers see the same errors from the source stream:

```csharp
try
{
    var textTask = Task.Run(async () =>
    {
        await foreach (var chunk in result.GetTextStreamAsync())
            Console.Write(chunk);
    });
    
    var countTask = Task.Run(async () =>
    {
        int count = 0;
        await foreach (var _ in result.GetFullStreamAsync())
            count++;
        return count;
    });
    
    await Task.WhenAll(textTask, countTask);
}
catch (OpenRouterException ex)
{
    Console.WriteLine($"Stream error: {ex.Message}");
    // Both consumers will see the same exception
}
```

## API Reference

### `ReusableStream<T>`

```csharp
public class ReusableStream<T>
{
    // Constructor
    public ReusableStream(IAsyncEnumerable<T> source);
    
    // Create an independent consumer
    public IAsyncEnumerable<T> CreateConsumer();
    
    // Start pumping source stream (called automatically)
    public void StartPump();
}
```

### `ModelResult` Extensions

```csharp
public class ModelResult
{
    // Get text-only stream consumer
    public async IAsyncEnumerable<string> GetTextStreamAsync();
    
    // Get reasoning content stream consumer
    public async IAsyncEnumerable<string> GetReasoningStreamAsync();
    
    // Get tool calls stream consumer
    public async IAsyncEnumerable<BetaToolCall> GetToolCallsStreamAsync();
    
    // Get full raw stream consumer
    public async IAsyncEnumerable<BetaResponsesStreamChunk> GetFullStreamAsync();
}
```

## See Also

- [Example27.ConcurrentStreams](../Examples/Example27.ConcurrentStreams/) - Complete examples
- [QUICKSTART.md](QUICKSTART.md) - Basic streaming usage
- [README.md](../README.md) - Main SDK documentation
