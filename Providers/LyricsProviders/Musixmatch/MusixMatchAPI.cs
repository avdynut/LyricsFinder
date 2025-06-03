using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LyricsProviders.MusixMatch;

// https://github.com/Strvm/musicxmatch-api/blob/main/src/musicxmatch_api/main.py 83144cda7a81d3f072014058b18d019f343b8de6
public static class MusixmatchAPI
{
    private static readonly HttpClient _client = new() { Timeout = TimeSpan.FromSeconds(5) };
    private static string _secret;

    private const string BaseUrl = "https://www.musixmatch.com/ws/1.1/";
    private const string UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36";

    static MusixmatchAPI()
    {
        _client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
    }

    private static async Task<string> GetLatestAppAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://www.musixmatch.com/search");
        request.Headers.Add("Cookie", "mxm_bab=AB");

        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        var pattern = @"src=\""([^\""]*/_next/static/chunks/pages/_app-[^\""]+\.js)\""";
        var match = Regex.Matches(content, pattern);
        if (match.Count > 0)
        {
            return match[^1].Groups[1].Value;
        }
        throw new Exception("_app URL not found.");
    }

    private static async Task<string> GetSecretAsync()
    {
        var appUrl = await GetLatestAppAsync();
        var jsCode = await _client.GetStringAsync(appUrl);

        var pattern = @"from\(\s*\""(.*?)\""\s*\.split";
        var match = Regex.Match(jsCode, pattern);
        if (!match.Success) throw new Exception("Encoded string not found.");

        var encoded = match.Groups[1].Value;
        var reversed = new string(encoded.Reverse().ToArray());
        var decodedBytes = Convert.FromBase64String(reversed);
        return Encoding.UTF8.GetString(decodedBytes);
    }

    private static async Task<string> GenerateSignature(string url)
    {
        var now = DateTime.UtcNow;
        string dateString = now.ToString("yyyyMMdd");
        string message = url + dateString;
        _secret ??= await GetSecretAsync();
        byte[] keyBytes = Encoding.UTF8.GetBytes(_secret);
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        using (var hmac = new HMACSHA256(keyBytes))
        {
            var hash = hmac.ComputeHash(messageBytes);
            var signature = Convert.ToBase64String(hash);
            var r = WebUtility.UrlEncode(signature).Replace("%2F", "/");
            return $"&signature={r}&signature_protocol=sha256";
        }
    }

    public static async Task<JsonDocument> MakeRequestAsync(string endpointQuery)
    {
        string fullUrl = BaseUrl + endpointQuery;
        string signedUrl = fullUrl + await GenerateSignature(fullUrl);
        var response = await _client.GetStringAsync(signedUrl);
        return JsonDocument.Parse(response);
    }

    public static async Task<JsonDocument> SearchTracksAsync(string query, int page = 1)
    {
        string url = $"track.search?app_id=web-desktop-app-v1.0&format=json&q={WebUtility.UrlEncode(query)}&f_has_lyrics=true&page_size=5&page={page}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetTrackAsync(string trackId = null, string trackIsrc = null)
    {
        if (trackId == null && trackIsrc == null)
            throw new ArgumentException("Either trackId or trackIsrc must be provided.");

        string param = trackId != null ? $"track_id={trackId}" : $"track_isrc={trackIsrc}";
        string url = $"track.get?app_id=web-desktop-app-v1.0&format=json&{param}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetTrackLyricsAsync(string trackId = null, string trackIsrc = null)
    {
        if (trackId == null && trackIsrc == null)
            throw new ArgumentException("Either trackId or trackIsrc must be provided.");
        string param = trackId != null ? $"track_id={trackId}" : $"track_isrc={trackIsrc}";
        string url = $"track.lyrics.get?app_id=web-desktop-app-v1.0&format=json&{param}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> SearchArtistAsync(string query, int page = 1)
    {
        string url = $"artist.search?app_id=web-desktop-app-v1.0&format=json&q_artist={WebUtility.UrlEncode(query)}&page_size=5&page={page}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetArtistAsync(string artistId)
    {
        string url = $"artist.get?app_id=web-desktop-app-v1.0&format=json&artist_id={artistId}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetArtistChartAsync(string country = "US", int page = 1)
    {
        string url = $"chart.artists.get?app_id=web-desktop-app-v1.0&format=json&page_size=5&country={country}&page={page}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetTrackChartAsync(string country = "US", int page = 1)
    {
        string url = $"chart.tracks.get?app_id=web-desktop-app-v1.0&format=json&page_size=5&country={country}&page={page}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetArtistAlbumsAsync(string artistId, int page = 1)
    {
        string url = $"artist.albums.get?app_id=web-desktop-app-v1.0&format=json&artist_id={artistId}&page_size=5&page={page}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetAlbumAsync(string albumId)
    {
        string url = $"album.get?app_id=web-desktop-app-v1.0&format=json&album_id={albumId}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetAlbumTracksAsync(string albumId, int page = 1)
    {
        string url = $"album.tracks.get?app_id=web-desktop-app-v1.0&format=json&album_id={albumId}&page_size=5&page={page}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetTrackLyricsTranslationAsync(string trackId, string selectedLanguage)
    {
        string url = $"crowd.track.translations.get?app_id=web-desktop-app-v1.0&format=json&track_id={trackId}&selected_language={selectedLanguage}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetTrackRichsyncAsync(
        string commontrackId = null,
        string trackId = null,
        string trackIsrc = null,
        string fRichsyncLength = null,
        string fRichsyncLengthMaxDeviation = null)
    {
        var url = new StringBuilder("track.richsync.get?app_id=web-desktop-app-v1.0&format=json");
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
        string url = $"matcher.lyrics.get?app_id=web-desktop-app-v1.0&format=json&q_track={title}&q_artist={artist}";
        return await MakeRequestAsync(url);
    }
}
