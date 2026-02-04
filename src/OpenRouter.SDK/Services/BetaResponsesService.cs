using OpenRouter.SDK.Core;
using OpenRouter.SDK.Models;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace OpenRouter.SDK.Services;
/// <summary>
/// Implementation of beta responses service.
/// </summary>
public class BetaResponsesService : IBetaResponsesService
{
    private readonly IHttpClientService _httpClient;
    /// <summary>
    /// Constructor for BetaResponsesService
    /// </summary>
    /// <param name="httpClient"></param>
    public BetaResponsesService(IHttpClientService httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }
    /// <summary>
    /// Sends a beta response request.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> A task that represents the asynchronous operation. The task result contains the beta responses response.</returns>
    public async Task<BetaResponsesResponse> SendAsync(
        BetaResponsesRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Ensure streaming is disabled for non-streaming requests
        var requestWithStream = new BetaResponsesRequest
        {
            Input = request.Input,
            Instructions = request.Instructions,
            Model = request.Model,
            Models = request.Models,
            Tools = request.Tools,
            ToolChoice = request.ToolChoice,
            ParallelToolCalls = request.ParallelToolCalls,
            MaxOutputTokens = request.MaxOutputTokens,
            Temperature = request.Temperature,
            TopP = request.TopP,
            TopK = request.TopK,
            PresencePenalty = request.PresencePenalty,
            FrequencyPenalty = request.FrequencyPenalty,
            MaxToolCalls = request.MaxToolCalls,
            Text = request.Text,
            Reasoning = request.Reasoning,
            Modalities = request.Modalities,
            Provider = request.Provider,
            Metadata = request.Metadata,
            Stream = false,
            Store = request.Store ?? false, // Required for /responses endpoint
            ServiceTier = request.ServiceTier ?? "auto", // Required for /responses endpoint
            User = request.User,
            SessionId = request.SessionId
        };

        var response = await _httpClient.PostJsonAsync<BetaResponsesRequest, BetaResponsesResponse>(
            "/responses",
            requestWithStream,
            cancellationToken: cancellationToken
        );

        return response;
    }
    /// <summary>
    /// Sends a beta response request and returns a stream of responses.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> An asynchronous stream of beta responses stream chunks.</returns>
    public async IAsyncEnumerable<BetaResponsesStreamChunk> SendStreamAsync(
        BetaResponsesRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Ensure streaming is enabled
        var requestWithStream = new BetaResponsesRequest
        {
            Input = request.Input,
            Instructions = request.Instructions,
            Model = request.Model,
            Models = request.Models,
            Tools = request.Tools,
            ToolChoice = request.ToolChoice,
            ParallelToolCalls = request.ParallelToolCalls,
            MaxOutputTokens = request.MaxOutputTokens,
            Temperature = request.Temperature,
            TopP = request.TopP,
            TopK = request.TopK,
            PresencePenalty = request.PresencePenalty,
            FrequencyPenalty = request.FrequencyPenalty,
            MaxToolCalls = request.MaxToolCalls,
            Text = request.Text,
            Reasoning = request.Reasoning,
            Modalities = request.Modalities,
            Provider = request.Provider,
            Metadata = request.Metadata,
            Stream = true,
            Store = request.Store ?? false, // Required for /responses endpoint
            ServiceTier = request.ServiceTier ?? "auto", // Required for /responses endpoint
            User = request.User,
            SessionId = request.SessionId
        };

        var requestMessage = await _httpClient.CreateRequestMessage(
            HttpMethod.Post,
            "/responses",
            requestWithStream
        );

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}). Error: {errorContent}");
        }

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

            BetaResponsesStreamChunk? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<BetaResponsesStreamChunk>(data);
            }
            catch (JsonException)
            {
                continue;
            }

            if (chunk != null)
            {
                yield return chunk;
            }
        }
    }
}
