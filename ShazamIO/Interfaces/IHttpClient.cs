using System.Text.Json;

namespace ShazamIO.Interfaces;

/// <summary>
/// Interface for HTTP client operations.
/// </summary>
public interface IHttpClient : IDisposable
{
    /// <summary>
    /// Sends an HTTP request and returns the response as a JSON document.
    /// </summary>
    /// <param name="method">HTTP method (GET or POST)</param>
    /// <param name="url">Request URL</param>
    /// <param name="headers">Optional headers dictionary</param>
    /// <param name="queryParams">Optional query parameters</param>
    /// <param name="jsonBody">Optional JSON body for POST requests</param>
    /// <param name="proxy">Optional proxy URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JsonDocument containing the response</returns>
    Task<JsonDocument> RequestAsync(
        HttpMethod method,
        string url,
        Dictionary<string, string>? headers = null,
        Dictionary<string, string>? queryParams = null,
        object? jsonBody = null,
        string? proxy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a GET request and returns the response as a JSON document.
    /// </summary>
    Task<JsonDocument> GetAsync(
        string url,
        Dictionary<string, string>? headers = null,
        Dictionary<string, string>? queryParams = null,
        string? proxy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request with JSON body and returns the response as a JSON document.
    /// </summary>
    Task<JsonDocument> PostAsync(
        string url,
        object jsonBody,
        Dictionary<string, string>? headers = null,
        string? proxy = null,
        CancellationToken cancellationToken = default);
}
