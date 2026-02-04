using OpenRouter.SDK.Core;
using OpenRouter.SDK.Models;

namespace OpenRouter.SDK.Services;

/// <summary>
/// Interface for Analytics service operations
/// Provides user activity tracking and analytics data
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Get user activity grouped by endpoint for the last 30 (completed) UTC days.
    /// Provisioning key required.
    /// </summary>
    /// <param name="date">Optional filter by a single UTC date in the last 30 days (YYYY-MM-DD format)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User activity data grouped by endpoint</returns>
    Task<GetUserActivityResponse> GetUserActivityAsync(string? date = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of Analytics service
/// Requires a provisioning key for authentication
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly IHttpClientService _httpClient;

    /// <summary>
    /// Constructor for AnalyticsService
    /// </summary>
    /// <param name="httpClient">HTTP client service for making API requests</param>
    public AnalyticsService(IHttpClientService httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<GetUserActivityResponse> GetUserActivityAsync(string? date = null, CancellationToken cancellationToken = default)
    {
        // Validate date format if provided
        if (!string.IsNullOrWhiteSpace(date))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(date, @"^\d{4}-\d{2}-\d{2}$"))
            {
                throw new ArgumentException("Date must be in YYYY-MM-DD format", nameof(date));
            }
        }

        // Build query string
        var path = "/activity";
        if (!string.IsNullOrWhiteSpace(date))
        {
            path += $"?date={Uri.EscapeDataString(date)}";
        }

        var response = await _httpClient.GetAsync<GetUserActivityResponse>(
            path,
            cancellationToken: cancellationToken);

        return response;
    }
}
