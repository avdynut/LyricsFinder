using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace LyricsProviders.LrcLib;

// https://lrclib.net/docs
public static class LrcLibAPI
{
    private static readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("https://lrclib.net/api/"),
        Timeout = TimeSpan.FromSeconds(10), // Increased timeout as per docs recommendation
    };

    static LrcLibAPI()
    {
        _client.DefaultRequestHeaders.Add("User-Agent", "Lyrixound/1.2.3 (https://github.com/avdynut/LyricsFinder)");
    }

    /// <summary>
    /// Get lyrics with a track's signature (attempts to access external sources if not in internal database)
    /// </summary>
    /// <param name="trackName">Title of the track</param>
    /// <param name="artistName">Name of the artist</param>
    /// <param name="albumName">Name of the album</param>
    /// <param name="duration">Track's duration in seconds</param>
    /// <returns>JsonDocument containing track lyrics or null if not found</returns>
    public static async Task<JsonDocument> GetLyricsBySignature(string trackName, string artistName, string albumName, int duration)
    {
        if (string.IsNullOrWhiteSpace(trackName) || string.IsNullOrWhiteSpace(artistName) ||
            string.IsNullOrWhiteSpace(albumName) || duration <= 0)
        {
            return null;
        }

        var url = $"get?track_name={HttpUtility.UrlEncode(trackName)}&artist_name={HttpUtility.UrlEncode(artistName)}&album_name={HttpUtility.UrlEncode(albumName)}&duration={duration}";

        try
        {
            return await MakeRequestAsync(url);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            return null; // Track not found
        }
    }

    /// <summary>
    /// Get lyrics with a track's signature (cached only - will NOT attempt to access external sources)
    /// </summary>
    /// <param name="trackName">Title of the track</param>
    /// <param name="artistName">Name of the artist</param>
    /// <param name="albumName">Name of the album</param>
    /// <param name="duration">Track's duration in seconds</param>
    /// <returns>JsonDocument containing track lyrics or null if not found</returns>
    public static async Task<JsonDocument> GetLyricsCached(string trackName, string artistName, string albumName, int duration)
    {
        if (string.IsNullOrWhiteSpace(trackName) || string.IsNullOrWhiteSpace(artistName) ||
            string.IsNullOrWhiteSpace(albumName) || duration <= 0)
        {
            return null;
        }

        var url = $"get-cached?track_name={HttpUtility.UrlEncode(trackName)}&artist_name={HttpUtility.UrlEncode(artistName)}&album_name={HttpUtility.UrlEncode(albumName)}&duration={duration}";

        try
        {
            return await MakeRequestAsync(url);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            return null; // Track not found
        }
    }

    /// <summary>
    /// Get lyrics by LRCLIB's ID
    /// </summary>
    /// <param name="id">ID of the lyrics record</param>
    /// <returns>JsonDocument containing track lyrics or null if not found</returns>
    public static async Task<JsonDocument> GetLyricsById(int id)
    {
        var url = $"get/{id}";

        try
        {
            return await MakeRequestAsync(url);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            return null; // Track not found
        }
    }

    /// <summary>
    /// Search for lyrics records using keywords in any field
    /// </summary>
    /// <param name="query">Search for keyword present in ANY fields (track's title, artist name or album name)</param>
    /// <returns>JsonDocument containing search results (max 20 results)</returns>
    public static async Task<JsonDocument> SearchLyrics(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return JsonDocument.Parse("[]");
        }

        var url = $"search?q={HttpUtility.UrlEncode(query)}";
        return await MakeRequestAsync(url);
    }

    /// <summary>
    /// Search for lyrics records using specific fields
    /// </summary>
    /// <param name="trackName">Search for keyword in track's title (conditional - required if query is null)</param>
    /// <param name="artistName">Search for keyword in track's artist name (optional)</param>
    /// <param name="albumName">Search for keyword in track's album name (optional)</param>
    /// <returns>JsonDocument containing search results (max 20 results)</returns>
    public static async Task<JsonDocument> SearchLyricsByFields(string trackName = null, string artistName = null, string albumName = null)
    {
        if (string.IsNullOrWhiteSpace(trackName))
        {
            return JsonDocument.Parse("[]");
        }

        var url = $"search?track_name={HttpUtility.UrlEncode(trackName)}";

        if (!string.IsNullOrWhiteSpace(artistName))
        {
            url += $"&artist_name={HttpUtility.UrlEncode(artistName)}";
        }

        if (!string.IsNullOrWhiteSpace(albumName))
        {
            url += $"&album_name={HttpUtility.UrlEncode(albumName)}";
        }

        return await MakeRequestAsync(url);
    }

    private static async Task<JsonDocument> MakeRequestAsync(string url)
    {
        var response = await _client.GetStringAsync(url);
        return JsonDocument.Parse(response);
    }
}