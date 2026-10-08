using System.Text.Json;
using ShazamIO.Algorithm;
using ShazamIO.Client;
using ShazamIO.Constants;
using ShazamIO.Enums;
using ShazamIO.Interfaces;
using ShazamIO.Services;

namespace ShazamIO;

/// <summary>
/// Shazam client used to recognize a song from audio bytes.
/// </summary>
public class Shazam : IDisposable
{
    private const string DefaultTimeZone = "Europe/Moscow";

    private readonly IHttpClient _httpClient;
    private readonly string _language;
    private readonly string _endpointCountry;
    private bool _disposed;

    /// <summary>
    /// Creates a new Shazam client.
    /// </summary>
    /// <param name="language">Language code (default: "en-US")</param>
    /// <param name="endpointCountry">Endpoint country code (default: "GB")</param>
    /// <param name="httpClient">Custom HTTP client (optional)</param>
    public Shazam(
        string language = "en-US",
        string endpointCountry = "GB",
        IHttpClient? httpClient = null)
    {
        _language = language;
        _endpointCountry = endpointCountry;
        _httpClient = httpClient ?? new ShazamHttpClient();
    }

    private Dictionary<string, string> GetHeaders()
    {
        return new Dictionary<string, string>
        {
            ["X-Shazam-Platform"] = "IPHONE",
            ["X-Shazam-AppVersion"] = "14.1.0",
            ["Accept"] = "*/*",
            ["Accept-Language"] = _language,
            ["Accept-Encoding"] = "gzip, deflate",
            ["User-Agent"] = UserAgents.GetRandom()
        };
    }

    /// <summary>
    /// Recognizes a song from audio bytes.
    /// </summary>
    /// <param name="audioBytes">Audio data bytes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recognition response</returns>
    public async Task<JsonDocument> RecognizeAsync(byte[] audioBytes, CancellationToken cancellationToken = default)
    {
        var normalizedAudio = AudioConverter.NormalizeAudioData(audioBytes);
        var generator = AudioConverter.CreateSignatureGenerator(normalizedAudio);
        var signature = generator.GetNextSignature();

        if (signature == null)
        {
            return JsonDocument.Parse("{\"matches\": []}");
        }

        return await SendRecognizeRequestAsync(signature, cancellationToken);
    }

    private async Task<JsonDocument> SendRecognizeRequestAsync(DecodedMessage signature, CancellationToken cancellationToken)
    {
        var sampleMs = (int)(signature.NumberSamples / (double)signature.SampleRateHz * 1000);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var data = AudioConverter.CreateSearchData(
            DefaultTimeZone,
            signature.EncodeToUri(),
            sampleMs,
            timestamp);

        var url = string.Format(
            ShazamUrls.SearchFromFile,
            _language,
            _endpointCountry,
            DeviceExtensions.RandomDevice().ToValue(),
            Guid.NewGuid().ToString().ToUpper(),
            Guid.NewGuid().ToString().ToUpper());

        return await _httpClient.PostAsync(url, data, GetHeaders(), cancellationToken);
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
