using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using NLog;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LyricsProviders;

public class LyricsOvhTrackInfoProvider : ITrackInfoProvider
{
    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    public const string Name = "lyrics.ovh";
    public string DisplayName => Name;

    public async Task<Track> FindTrackAsync(TrackInfo trackInfo)
    {
        var track = new Track(trackInfo);
        try
        {
            var httpClient = new HttpClient();
            var uri = new Uri($"https://api.lyrics.ovh/v1/{trackInfo.Artist}/{trackInfo.Title}");
            var response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            var jsonContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonContent);
            var lyrics = doc.RootElement.GetProperty("lyrics").GetString();
            track.Lyrics = new UnsyncedLyric(lyrics.Replace("\n\n", "\n")) { Source = uri };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching lyrics from lyrics.ovh");
            track.Lyrics = new NoneLyric(ex.Message);
        }

        return track;
    }
}
