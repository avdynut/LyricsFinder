using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace LyricsProviders.LyricsOvh;

public static class LyricsOvhApi
{
    private static readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("https://api.lyrics.ovh/"),
        Timeout = TimeSpan.FromSeconds(5),
    };

    public static async Task<JsonDocument> SuggestSongs(string query)
    {
        var url = $"suggest/{HttpUtility.UrlEncode(query)}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> GetLyrics(string artist, string title)
    {
        var url = $"v1/{HttpUtility.UrlEncode(artist)}/{HttpUtility.UrlEncode(title)}";
        return await MakeRequestAsync(url);
    }

    public static async Task<JsonDocument> MatchLyrics(string artist, string title)
    {
        var suggested = await SuggestSongs($"{artist} {title}");
        var count = suggested.RootElement.GetProperty("total").GetInt32();

        if (count > 0)
        {
            var track = suggested.RootElement.GetProperty("data")[0];
            artist = track.GetProperty("artist").GetProperty("name").GetString();
            title = track.GetProperty("title").GetString();
            return await GetLyrics(artist, title);
        }
        else
        {
            return await GetLyrics(artist, title);
        }
    }

    private static async Task<JsonDocument> MakeRequestAsync(string url)
    {
        var response = await _client.GetStringAsync(url);
        return JsonDocument.Parse(response);
    }
}
