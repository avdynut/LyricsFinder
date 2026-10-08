using System.Net;
using System.Text;
using System.Text.Json;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using ShazamIO.Exceptions;
using ShazamIO.Interfaces;

namespace ShazamIO.Client;

/// <summary>
/// HTTP client implementation with retry logic for Shazam API requests.
/// </summary>
public class ShazamHttpClient : IHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private bool _disposed;

    /// <summary>
    /// Retry options for HTTP requests.
    /// </summary>
    public class RetryOptions
    {
        public int MaxRetries { get; set; } = 20;
        public TimeSpan MaxTimeout { get; set; } = TimeSpan.FromSeconds(60);
        public HashSet<HttpStatusCode> RetryOnStatusCodes { get; set; } = new()
        {
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout,
            (HttpStatusCode)429 // TooManyRequests
        };
    }

    public ShazamHttpClient(RetryOptions? retryOptions = null)
    {
        retryOptions ??= new RetryOptions();

        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = retryOptions.MaxTimeout
        };

        _retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => retryOptions.RetryOnStatusCodes.Contains(response.StatusCode))
            .WaitAndRetryAsync(
                retryOptions.MaxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    public async Task<JsonDocument> PostAsync(
        string url,
        object jsonBody,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            var json = JsonSerializer.Serialize(jsonBody);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return await _httpClient.SendAsync(request, cancellationToken);
        });

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            return JsonDocument.Parse(content);
        }
        catch (JsonException ex)
        {
            var preview = content.Length > 200 ? content[..200] + "..." : content;
            throw new FailedDecodeJsonException($"Failed to decode JSON response. Content preview: {preview}", ex);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
