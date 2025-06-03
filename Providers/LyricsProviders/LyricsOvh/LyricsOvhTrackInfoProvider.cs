using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using NLog;
using System;
using System.Threading.Tasks;

namespace LyricsProviders.LyricsOvh;

public class LyricsOvhTrackInfoProvider : ITrackInfoProvider
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public const string Name = "lyrics.ovh";
    public string DisplayName => Name;

    public async Task<Track> FindTrackAsync(TrackInfo trackInfo)
    {
        var track = new Track(trackInfo);
        try
        {
            using var doc = await LyricsOvhApi.MatchLyrics(trackInfo.Artist, trackInfo.Title);
            var lyrics = doc.RootElement.GetProperty("lyrics").GetString();
            track.Lyrics = new UnsyncedLyric(lyrics.Replace("\n\n", "\n"));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching lyrics from lyrics.ovh");
            track.Lyrics = new NoneLyric(ex.Message);
        }

        return track;
    }
}
