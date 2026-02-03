using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ShazamHttpClient>? _logger;
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

    public ShazamHttpClient(RetryOptions? retryOptions = null, ILogger<ShazamHttpClient>? logger = null)
    {
        _logger = logger;
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
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    _logger?.LogDebug(
                        "Retry {RetryAttempt} after {Delay}ms for {Url}. Status: {StatusCode}",
                        retryAttempt,
                        timespan.TotalMilliseconds,
                        context["url"],
                        outcome.Result?.StatusCode);
                });
    }

    public async Task<JsonDocument> RequestAsync(
        HttpMethod method,
        string url,
        Dictionary<string, string>? headers = null,
        Dictionary<string, string>? queryParams = null,
        object? jsonBody = null,
        string? proxy = null,
        CancellationToken cancellationToken = default)
    {
        if (queryParams != null && queryParams.Count > 0)
        {
            var queryString = string.Join("&", 
                queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            url = url.Contains('?') ? $"{url}&{queryString}" : $"{url}?{queryString}";
        }

        var context = new Context { ["url"] = url };

        var response = await _retryPolicy.ExecuteAsync(async ctx =>
        {
            using var request = new HttpRequestMessage(method, url);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (jsonBody != null && method == HttpMethod.Post)
            {
                var json = JsonSerializer.Serialize(jsonBody);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            _logger?.LogDebug("Sending {Method} request to {Url}", method, url);

            return await _httpClient.SendAsync(request, cancellationToken);
        }, context);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        // Check if response is successful
        if (!response.IsSuccessStatusCode)
        {
            _logger?.LogWarning("HTTP request failed with status {StatusCode}: {Content}", 
                response.StatusCode, content.Length > 500 ? content[..500] : content);
        }

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

    public Task<JsonDocument> GetAsync(
        string url,
        Dictionary<string, string>? headers = null,
        Dictionary<string, string>? queryParams = null,
        string? proxy = null,
        CancellationToken cancellationToken = default)
    {
        return RequestAsync(HttpMethod.Get, url, headers, queryParams, null, proxy, cancellationToken);
    }

    public Task<JsonDocument> PostAsync(
        string url,
        object jsonBody,
        Dictionary<string, string>? headers = null,
        string? proxy = null,
        CancellationToken cancellationToken = default)
    {
        return RequestAsync(HttpMethod.Post, url, headers, null, jsonBody, proxy, cancellationToken);
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
