using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsProviders.MusixMatch;

// https://github.com/Strvm/musicxmatch-api/blob/main/src/musicxmatch_api/main.py 83144cda7a81d3f072014058b18d019f343b8de6
// https://github.com/Strvm/musicxmatch-api/issues/16#issuecomment-3855763176
// https://github.com/spicetify/cli/pull/3790
// https://docs.musixmatch.com/lyrics-api/
public static class MusixmatchAPI
{
    private static readonly HttpClient _client;
    private static readonly SemaphoreSlim _tokenLock = new(1, 1);
    private static string _userToken;

    private const string BaseUrl = "https://apic-appmobile.musixmatch.com/ws/1.1/";
    private const string AppId = "mac-ios-v2.0";
    private const string UserAgent = "Musixmatch/2025120901 CFNetwork/3860.300.31 Darwin/25.2.0";

    static MusixmatchAPI()
    {
        var handler = new HttpClientHandler { UseCookies = false };
        _client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
        _client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);
        _client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        _client.DefaultRequestHeaders.TryAddWithoutValidation("x-mxm-app-version", "10.1.1");
        _client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
        _client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", $"x-mxm-token-guid={Guid.NewGuid()}");
    }

    private static async Task<string> EnsureUserTokenAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && !string.IsNullOrEmpty(_userToken))
            return _userToken;

        await _tokenLock.WaitAsync();
        try
        {
            if (!forceRefresh && !string.IsNullOrEmpty(_userToken))
                return _userToken;

            using var doc = await FetchJsonAsync($"token.get?app_id={AppId}", appendUserToken: false);
            var header = doc.RootElement.GetProperty("message").GetProperty("header");
            var statusCode = header.GetProperty("status_code").GetInt32();
            if (statusCode != (int)HttpStatusCode.OK)
            {
                var hint = header.TryGetProperty("hint", out var hintProp) ? hintProp.GetString() : null;
                throw new Exception($"Musixmatch token.get failed with status {statusCode}" +
                                    (hint != null ? $" ({hint})" : string.Empty));
            }

            var token = doc.RootElement
                .GetProperty("message")
                .GetProperty("body")
                .GetProperty("user_token")
                .GetString();

            if (string.IsNullOrEmpty(token))
                throw new Exception("Musixmatch token.get returned an empty user_token.");

            _userToken = token;
            return _userToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private static async Task<JsonDocument> FetchJsonAsync(string endpointQuery, bool appendUserToken)
    {
        var url = BaseUrl + endpointQuery;
        if (appendUserToken)
        {
            var token = await EnsureUserTokenAsync();
            url += $"&usertoken={WebUtility.UrlEncode(token)}";
        }

        var response = await _client.GetStringAsync(url);
        return JsonDocument.Parse(response);
    }

    private static bool IsTokenRenewRequired(JsonDocument doc)
    {
        if (!doc.RootElement.TryGetProperty("message", out var message) ||
            !message.TryGetProperty("header", out var header) ||
            !header.TryGetProperty("status_code", out var statusProp) ||
            statusProp.GetInt32() != (int)HttpStatusCode.Unauthorized)
        {
            return false;
        }

        if (!header.TryGetProperty("hint", out var hintProp))
            return true;

        var hint = hintProp.GetString();
        return string.Equals(hint, "renew", StringComparison.OrdinalIgnoreCase)
               || string.Equals(hint, "captcha", StringComparison.OrdinalIgnoreCase)
               || string.IsNullOrEmpty(hint);
    }

    public static async Task<JsonDocument> MakeRequestAsync(string endpointQuery)
    {
        var doc = await FetchJsonAsync(endpointQuery, appendUserToken: true);
        if (!IsTokenRenewRequired(doc))
            return doc;

        doc.Dispose();
        _userToken = null;
        await EnsureUserTokenAsync(forceRefresh: true);
        return await FetchJsonAsync(endpointQuery, appendUserToken: true);
    }

    public static async Task<JsonDocument> GetMacroSubtitlesAsync(string artist, string title)
    {
        string url =
            $"macro.subtitles.get?format=json&namespace=lyrics_richsynched&subtitle_format=lrc&app_id={AppId}" +
            $"&q_artist={WebUtility.UrlEncode(artist)}&q_track={WebUtility.UrlEncode(title)}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> SearchTracksAsync(string query, int page = 1)
    {
        string url = $"track.search?app_id={AppId}&format=json&q={WebUtility.UrlEncode(query)}&f_has_lyrics=true&page_size=5&page={page}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetTrackAsync(string trackId = null, string trackIsrc = null)
    {
        if (trackId == null && trackIsrc == null)
            throw new ArgumentException("Either trackId or trackIsrc must be provided.");

        string param = trackId != null ? $"track_id={trackId}" : $"track_isrc={trackIsrc}";
        string url = $"track.get?app_id={AppId}&format=json&{param}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetTrackLyricsAsync(
        string commontrackId = null,
        string trackIsrc = null,
        string trackId = null)
    {
        if (commontrackId == null && trackIsrc == null && trackId == null)
            throw new ArgumentException("Either commontrackId, trackId, or trackIsrc must be provided.");

        string param = commontrackId != null ? $"commontrack_id={commontrackId}"
            : trackId != null ? $"track_id={trackId}"
            : $"track_isrc={trackIsrc}";
        string url = $"track.lyrics.get?app_id={AppId}&format=json&{param}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetTrackSubtitleAsync(
        string commontrackId = null,
        string trackId = null,
        string trackIsrc = null)
    {
        if (commontrackId == null && trackId == null && trackIsrc == null)
            throw new ArgumentException("Either commontrackId, trackId, or trackIsrc must be provided.");

        var url = new StringBuilder($"track.subtitle.get?app_id={AppId}&format=json&subtitle_format=lrc");
        if (!string.IsNullOrEmpty(commontrackId))
            url.Append($"&commontrack_id={commontrackId}");
        if (!string.IsNullOrEmpty(trackId))
            url.Append($"&track_id={trackId}");
        if (!string.IsNullOrEmpty(trackIsrc))
            url.Append($"&track_isrc={trackIsrc}");

        return await MakeRequestAsync(url.ToString());
    }

    public static async Task<JsonDocument> SearchArtistAsync(string query, int page = 1)
    {
        string url = $"artist.search?app_id={AppId}&format=json&q_artist={WebUtility.UrlEncode(query)}&page_size=5&page={page}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetArtistAsync(string artistId)
    {
        string url = $"artist.get?app_id={AppId}&format=json&artist_id={artistId}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetArtistChartAsync(string country = "US", int page = 1)
    {
        string url = $"chart.artists.get?app_id={AppId}&format=json&page_size=5&country={country}&page={page}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetTrackChartAsync(string country = "US", int page = 1)
    {
        string url = $"chart.tracks.get?app_id={AppId}&format=json&page_size=5&country={country}&page={page}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetArtistAlbumsAsync(string artistId, int page = 1)
    {
        string url = $"artist.albums.get?app_id={AppId}&format=json&artist_id={artistId}&page_size=5&page={page}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetAlbumAsync(string albumId)
    {
        string url = $"album.get?app_id={AppId}&format=json&album_id={albumId}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetAlbumTracksAsync(string albumId, int page = 1)
    {
        string url = $"album.tracks.get?app_id={AppId}&format=json&album_id={albumId}&page_size=5&page={page}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetTrackLyricsTranslationAsync(string trackId, string selectedLanguage)
    {
        string url = $"crowd.track.translations.get?app_id={AppId}&format=json&track_id={trackId}&selected_language={selectedLanguage}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetTrackRichsyncAsync(
        string commontrackId = null,
        string trackId = null,
        string trackIsrc = null,
        string fRichsyncLength = null,
        string fRichsyncLengthMaxDeviation = null)
    {
        var url = new StringBuilder($"track.richsync.get?app_id={AppId}&format=json");
        if (!string.IsNullOrEmpty(commontrackId))
            url.Append($"&commontrack_id={commontrackId}");
        if (!string.IsNullOrEmpty(trackId))
            url.Append($"&track_id={trackId}");
        if (!string.IsNullOrEmpty(trackIsrc))
            url.Append($"&track_isrc={trackIsrc}");
        if (!string.IsNullOrEmpty(fRichsyncLength))
            url.Append($"&f_richsync_length={fRichsyncLength}");
        if (!string.IsNullOrEmpty(fRichsyncLengthMaxDeviation))
            url.Append($"&f_richsync_length_max_deviation={fRichsyncLengthMaxDeviation}");

        return await MakeRequestAsync(url.ToString());
    }

    public static async Task<JsonDocument> MatchLyrics(string artist, string title)
    {
        string url = $"matcher.lyrics.get?app_id={AppId}&format=json&q_track={WebUtility.UrlEncode(title)}&q_artist={WebUtility.UrlEncode(artist)}";
        return await MakeRequestAsync(url);
    }
}
