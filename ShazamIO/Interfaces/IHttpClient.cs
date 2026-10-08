using System.Text.Json;

namespace ShazamIO.Interfaces;

/// <summary>
/// Sends the recognition request to Shazam.
/// </summary>
public interface IHttpClient : IDisposable
{
    Task<JsonDocument> PostAsync(
        string url,
        object jsonBody,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);
}
