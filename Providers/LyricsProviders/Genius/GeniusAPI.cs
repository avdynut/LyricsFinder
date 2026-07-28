using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace LyricsProviders.Genius;

public static class GeniusAPI
{
    private static readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromSeconds(15),
    };

    static GeniusAPI()
    {
        _client.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
        _client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/html");
    }

    public static async Task<JsonDocument> SearchSongsAsync(string query)
    {
        var url = $"https://genius.com/api/search?q={HttpUtility.UrlEncode(query)}";
        var response = await _client.GetStringAsync(url);
        return JsonDocument.Parse(response);
    }

    public static async Task<string> GetSongPageHtmlAsync(string songUrl)
    {
        return await _client.GetStringAsync(songUrl);
    }
}
