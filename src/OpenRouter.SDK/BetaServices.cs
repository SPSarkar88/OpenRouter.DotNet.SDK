using OpenRouter.SDK.Core;
using OpenRouter.SDK.Services;

namespace OpenRouter.SDK;

/// <summary>
/// Container for Beta API services
/// </summary>
public class BetaServices
{
    private readonly IBetaResponsesService _responses;

    /// <summary>
    /// Initializes a new instance of the <see cref="BetaServices"/> class for direct instantiation.
    /// </summary>
    /// <param name="httpClient">The HTTP client service.</param>
    public BetaServices(IHttpClientService httpClient)
    {
        _responses = new BetaResponsesService(httpClient);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BetaServices"/> class for dependency injection.
    /// </summary>
    /// <param name="responsesService">The beta responses service.</param>
    public BetaServices(IBetaResponsesService responsesService)
    {
        _responses = responsesService ?? throw new ArgumentNullException(nameof(responsesService));
    }

    /// <summary>
    /// Gets the Beta Responses service for modern structured response API
    /// </summary>
    public IBetaResponsesService Responses => _responses;
}
