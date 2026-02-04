using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace OpenRouter.SDK.Core;

/// <summary>
/// A reusable stream that allows multiple consumers to read from the same source stream
/// concurrently while it's actively streaming, without forcing consumers to wait for full buffering.
/// 
/// Key features:
/// - Multiple concurrent consumers with independent read positions
/// - New consumers can attach while streaming is active
/// - Efficient memory management with automatic cleanup
/// - Each consumer can read at their own pace
/// - Thread-safe for concurrent consumption
/// </summary>
/// <typeparam name="T">Type of items in the stream</typeparam>
public class ReusableStream<T>
{
    private readonly List<T> _buffer = new();
    private readonly ConcurrentDictionary<int, ConsumerState> _consumers = new();
    private readonly object _bufferLock = new();
    private int _nextConsumerId = 0;
    private IAsyncEnumerator<T>? _sourceEnumerator;
    private bool _sourceComplete = false;
    private Exception? _sourceError = null;
    private bool _pumpStarted = false;
    private Task? _pumpTask;

    /// <summary>
    /// Create a new reusable stream from a source async enumerable
    /// </summary>
    /// <param name="source">Source stream to buffer and multiplex</param>
    public ReusableStream(IAsyncEnumerable<T> source)
    {
        Source = source;
    }

    /// <summary>
    /// The source stream being multiplexed
    /// </summary>
    public IAsyncEnumerable<T> Source { get; }

    /// <summary>
    /// Create a new consumer that can independently iterate over the stream.
    /// Multiple consumers can be created and will all receive the same data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for this consumer</param>
    /// <returns>Async enumerable for this consumer</returns>
    public async IAsyncEnumerable<T> CreateConsumer([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var consumerId = Interlocked.Increment(ref _nextConsumerId);
        var state = new ConsumerState
        {
            Position = 0,
            Cancelled = false,
            WaitingTcs = null
        };

        _consumers.TryAdd(consumerId, state);

        // Start pumping the source stream if not already started
        if (!_pumpStarted)
        {
            StartPump();
        }

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                T? value;
                bool hasValue;

                lock (_bufferLock)
                {
                    // Check if we have buffered data at this position
                    if (state.Position < _buffer.Count)
                    {
                        value = _buffer[state.Position];
                        state.Position++;
                        hasValue = true;
                    }
                    else
                    {
                        hasValue = false;
                        value = default;
                    }
                }

                if (hasValue)
                {
                    yield return value!;
                    continue;
                }

                // No buffered data available
                // Check if source is complete
                if (_sourceComplete)
                {
                    // Check one more time for any final buffered data
                    lock (_bufferLock)
                    {
                        if (state.Position < _buffer.Count)
                        {
                            value = _buffer[state.Position];
                            state.Position++;
                            yield return value!;
                            continue;
                        }
                    }

                    // Truly done
                    break;
                }

                // Check for errors
                if (_sourceError != null)
                {
                    throw _sourceError;
                }

                // Wait for more data
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                state.WaitingTcs = tcs;

                // Double-check conditions after setting up the wait
                // This handles race conditions where data arrived between our check and setting up the wait
                lock (_bufferLock)
                {
                    if (state.Position < _buffer.Count || _sourceComplete || _sourceError != null)
                    {
                        tcs.TrySetResult(true);
                    }
                }

                // Wait for notification or cancellation
                using var registration = cancellationToken.Register(() => tcs.TrySetCanceled());
                await tcs.Task;

                state.WaitingTcs = null;
            }
        }
        finally
        {
            // Clean up consumer
            state.Cancelled = true;
            _consumers.TryRemove(consumerId, out _);
        }
    }

    /// <summary>
    /// Start pumping data from the source stream into the buffer
    /// </summary>
    private void StartPump()
    {
        lock (_bufferLock)
        {
            if (_pumpStarted)
            {
                return;
            }
            _pumpStarted = true;
        }

        _sourceEnumerator = Source.GetAsyncEnumerator();
        _pumpTask = Task.Run(async () =>
        {
            try
            {
                while (await _sourceEnumerator.MoveNextAsync())
                {
                    var item = _sourceEnumerator.Current;

                    lock (_bufferLock)
                    {
                        _buffer.Add(item);
                    }

                    // Notify waiting consumers
                    NotifyAllConsumers();
                }

                _sourceComplete = true;
                NotifyAllConsumers();
            }
            catch (Exception ex)
            {
                _sourceError = ex;
                NotifyAllConsumers();
            }
            finally
            {
                if (_sourceEnumerator != null)
                {
                    await _sourceEnumerator.DisposeAsync();
                }
            }
        });
    }

    /// <summary>
    /// Notify all waiting consumers that new data is available
    /// </summary>
    private void NotifyAllConsumers()
    {
        foreach (var consumer in _consumers.Values)
        {
            var tcs = consumer.WaitingTcs;
            if (tcs != null)
            {
                if (_sourceError != null)
                {
                    tcs.TrySetException(_sourceError);
                }
                else
                {
                    tcs.TrySetResult(true);
                }
            }
        }
    }

    /// <summary>
    /// Cancel all consumers and stop pumping
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        // Cancel all consumers
        foreach (var consumer in _consumers.Values)
        {
            consumer.Cancelled = true;
            consumer.WaitingTcs?.TrySetCanceled();
        }
        _consumers.Clear();

        // Dispose source enumerator
        if (_sourceEnumerator != null)
        {
            await _sourceEnumerator.DisposeAsync();
        }

        // Wait for pump task to complete
        if (_pumpTask != null)
        {
            try
            {
                await _pumpTask;
            }
            catch
            {
                // Ignore exceptions during cleanup
            }
        }
    }

    /// <summary>
    /// State for a single consumer
    /// </summary>
    private class ConsumerState
    {
        public int Position { get; set; }
        public bool Cancelled { get; set; }
        public TaskCompletionSource<bool>? WaitingTcs { get; set; }
    }
}
