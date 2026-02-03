using OpenRouter.SDK.Core;
using OpenRouter.SDK.Models;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Implementation of chat service.
/// </summary>
public class ChatService : IChatService
{
    private readonly IHttpClientService _httpClient;
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client service.</param>
    public ChatService(IHttpClientService httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc/>
    public async Task<ChatCompletionResponse> CreateAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Ensure stream is not set for non-streaming
        var requestCopy = new ChatCompletionRequest
        {
            Model = request.Model,
            Messages = request.Messages,
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
            TopP = request.TopP,
            TopK = request.TopK,
            Stream = false, // Explicitly set to false
            Stop = request.Stop,
            PresencePenalty = request.PresencePenalty,
            FrequencyPenalty = request.FrequencyPenalty,
            ResponseFormat = request.ResponseFormat,
            Tools = request.Tools,
            ToolChoice = request.ToolChoice,
            Provider = request.Provider,
            Reasoning = request.Reasoning
        };

        var response = await _httpClient.PostJsonAsync<ChatCompletionRequest, ChatCompletionResponse>(
            "/chat/completions",
            requestCopy,
            null,
            cancellationToken);

        return response;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ChatCompletionChunk> CreateStreamAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Ensure stream is set for streaming
        var requestCopy = new ChatCompletionRequest
        {
            Model = request.Model,
            Messages = request.Messages,
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
            TopP = request.TopP,
            TopK = request.TopK,
            Stream = true, // Explicitly set to true
            Stop = request.Stop,
            PresencePenalty = request.PresencePenalty,
            FrequencyPenalty = request.FrequencyPenalty,
            ResponseFormat = request.ResponseFormat,
            Tools = request.Tools,
            ToolChoice = request.ToolChoice,
            Provider = request.Provider,
            Reasoning = request.Reasoning
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/chat/completions")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestCopy, ((HttpClientService)_httpClient).JsonOptions),
                Encoding.UTF8,
                "application/json")
        };

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!line.StartsWith("data: "))
                continue;

            var data = line.Substring(6); // Remove "data: " prefix

            if (data == "[DONE]")
                break;

            ChatCompletionChunk? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(data, ((HttpClientService)_httpClient).JsonOptions);
            }
            catch (JsonException)
            {
                // Skip malformed chunks
                continue;
            }

            if (chunk != null)
            {
                yield return chunk;
            }
        }
    }
}
