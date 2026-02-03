using System.Text.Json;
using ShazamIO.Algorithm;
using ShazamIO.Client;
using ShazamIO.Constants;
using ShazamIO.Enums;
using ShazamIO.Interfaces;
using ShazamIO.Models;
using ShazamIO.Services;

namespace ShazamIO;

/// <summary>
/// Main Shazam API client for music recognition and chart data.
/// </summary>
public class Shazam : IDisposable
{
    private const string DefaultTimeZone = "Europe/Moscow";
    
    private readonly IHttpClient _httpClient;
    private readonly GeoService _geoService;
    private readonly string _language;
    private readonly string _endpointCountry;
    private readonly int _segmentDurationSeconds;
    private bool _disposed;

    /// <summary>
    /// Creates a new Shazam client.
    /// </summary>
    /// <param name="language">Language code (default: "en-US")</param>
    /// <param name="endpointCountry">Endpoint country code (default: "GB")</param>
    /// <param name="httpClient">Custom HTTP client (optional)</param>
    /// <param name="segmentDurationSeconds">Segment duration for recognition (default: 10)</param>
    public Shazam(
        string language = "en-US",
        string endpointCountry = "GB",
        IHttpClient? httpClient = null,
        int segmentDurationSeconds = 10)
    {
        _language = language;
        _endpointCountry = endpointCountry;
        _segmentDurationSeconds = segmentDurationSeconds;
        _httpClient = httpClient ?? new ShazamHttpClient();
        _geoService = new GeoService(_httpClient);
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

    #region Recognition Methods

    /// <summary>
    /// Recognizes a song from a file path.
    /// </summary>
    /// <param name="filePath">Path to the audio file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recognition response</returns>
    public async Task<JsonDocument> RecognizeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var audioData = await AudioConverter.ReadAudioFileAsync(filePath);
        return await RecognizeFromBytesAsync(audioData, cancellationToken);
    }

    /// <summary>
    /// Recognizes a song from audio bytes.
    /// </summary>
    /// <param name="audioBytes">Audio data bytes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recognition response</returns>
    public async Task<JsonDocument> RecognizeAsync(byte[] audioBytes, CancellationToken cancellationToken = default)
    {
        var normalizedAudio = AudioConverter.NormalizeAudioBytes(audioBytes);
        return await RecognizeFromBytesAsync(normalizedAudio, cancellationToken);
    }

    private async Task<JsonDocument> RecognizeFromBytesAsync(byte[] normalizedAudio, CancellationToken cancellationToken)
    {
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

        return await _httpClient.PostAsync(url, data, GetHeaders(), cancellationToken: cancellationToken);
    }

    #endregion

    #region Top Tracks Methods

    /// <summary>
    /// Gets the top world tracks.
    /// </summary>
    public async Task<JsonDocument> TopWorldTracksAsync(
        int limit = 200,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var playlistId = await _geoService.GetTopAsync(cancellationToken);
        var url = string.Format(ShazamUrls.TopTracksPlaylist, _endpointCountry, playlistId, limit, offset, _language);
        return await _httpClient.GetAsync(url, GetHeaders(), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets the top tracks for a specific country.
    /// </summary>
    public async Task<JsonDocument> TopCountryTracksAsync(
        string countryCode,
        int limit = 200,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var playlistId = await _geoService.GetCountryPlaylistAsync(countryCode, cancellationToken);
        var url = string.Format(ShazamUrls.TopTracksPlaylist, _endpointCountry, playlistId, limit, offset, _language);
        return await _httpClient.GetAsync(url, GetHeaders(), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets the top tracks for a specific city.
    /// </summary>
    public async Task<JsonDocument> TopCityTracksAsync(
        string countryCode,
        string cityName,
        int limit = 200,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var playlistId = await _geoService.GetCityPlaylistAsync(countryCode, cityName, cancellationToken);
        var url = string.Format(ShazamUrls.TopTracksPlaylist, _endpointCountry, playlistId, limit, offset, _language);
        return await _httpClient.GetAsync(url, GetHeaders(), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets the top world tracks by genre.
    /// </summary>
    public async Task<JsonDocument> TopWorldGenreTracksAsync(
        GenreMusic genre,
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var playlistId = await _geoService.GetGenreAsync(genre, cancellationToken);
        var url = string.Format(ShazamUrls.TopTracksPlaylist, _endpointCountry, playlistId, limit, offset, _language);
        return await _httpClient.GetAsync(url, GetHeaders(), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets the top tracks by genre for a specific country.
    /// </summary>
    public async Task<JsonDocument> TopCountryGenreTracksAsync(
        string countryCode,
        GenreMusic genre,
        int limit = 200,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var playlistId = await _geoService.GetGenreFromCountryAsync(countryCode, genre, cancellationToken);
        var url = string.Format(ShazamUrls.TopTracksPlaylist, _endpointCountry, playlistId, limit, offset, _language);
        return await _httpClient.GetAsync(url, GetHeaders(), cancellationToken: cancellationToken);
    }

    #endregion

    #region Track and Artist Methods

    /// <summary>
    /// Gets information about a specific track.
    /// </summary>
    public async Task<JsonDocument> TrackAboutAsync(
        long trackId,
        CancellationToken cancellationToken = default)
    {
        var url = string.Format(ShazamUrls.AboutTrack, _language, _endpointCountry, trackId);
        return await _httpClient.GetAsync(url, GetHeaders(), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets information about a specific artist.
    /// </summary>
    public async Task<JsonDocument> ArtistAboutAsync(
        long artistId,
        ArtistQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        var url = string.Format(ShazamUrls.SearchArtistV2, _endpointCountry, artistId);
        
        Dictionary<string, string>? queryParams = null;
        if (query != null)
        {
            queryParams = new Dictionary<string, string>();
            if (query.Extend.Count > 0)
                queryParams["extend"] = string.Join(",", query.Extend);
            if (query.Views.Count > 0)
                queryParams["views"] = string.Join(",", query.Views);
        }

        return await _httpClient.GetAsync(url, GetHeaders(), queryParams, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets related tracks for a specific track.
    /// </summary>
    public async Task<JsonDocument> RelatedTracksAsync(
        long trackId,
        int limit = 20,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var url = string.Format(ShazamUrls.RelatedSongs, _language, _endpointCountry, trackId, offset, limit);
        return await _httpClient.GetAsync(url, GetHeaders(), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Searches for artists by name.
    /// </summary>
    public async Task<JsonDocument> SearchArtistAsync(
        string query,
        int limit = 10,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var url = string.Format(ShazamUrls.SearchArtist, _language, _endpointCountry, 
            Uri.EscapeDataString(query), limit, offset);
        return await _httpClient.GetAsync(url, GetHeaders(), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Searches for tracks by name.
    /// </summary>
    public async Task<JsonDocument> SearchTrackAsync(
        string query,
        int limit = 10,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var url = string.Format(ShazamUrls.SearchMusic, _language, _endpointCountry,
            Uri.EscapeDataString(query), limit, offset);
        return await _httpClient.GetAsync(url, GetHeaders(), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets the listening counter for a track.
    /// </summary>
    public async Task<JsonDocument> ListeningCounterAsync(
        long trackId,
        CancellationToken cancellationToken = default)
    {
        var url = string.Format(ShazamUrls.ListeningCounter, trackId);
        return await _httpClient.GetAsync(url, GetHeaders(), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets albums for a specific artist.
    /// </summary>
    public async Task<JsonDocument> ArtistAlbumsAsync(
        long artistId,
        int limit = 10,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var url = string.Format(ShazamUrls.ArtistAlbums, _endpointCountry, artistId, limit, offset);
        return await _httpClient.GetAsync(url, GetHeaders(), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets information about a specific album.
    /// </summary>
    public async Task<JsonDocument> SearchAlbumAsync(
        long albumId,
        CancellationToken cancellationToken = default)
    {
        var url = string.Format(ShazamUrls.ArtistAlbumInfo, _endpointCountry, albumId);
        return await _httpClient.GetAsync(url, GetHeaders(), cancellationToken: cancellationToken);
    }

    #endregion

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
