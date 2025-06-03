using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using NLog;
using System;
using System.Threading.Tasks;

namespace LyricsProviders.MusixMatch;

public class MusixmatchTrackInfoProvider : ITrackInfoProvider
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public const string Name = "Musixmatch";
    public string DisplayName => Name;

    public async Task<Track> FindTrackAsync(TrackInfo trackInfo)
    {
        var track = new Track(trackInfo);
        try
        {
            using var doc = await MusixmatchAPI.MatchLyrics(trackInfo.Artist, trackInfo.Title);
            var lyrics = doc.RootElement
                .GetProperty("message")
                .GetProperty("body")
                .GetProperty("lyrics")
                .GetProperty("lyrics_body")
                .GetString();
            track.Lyrics = new UnsyncedLyric(lyrics);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching lyrics from Musixmatch");
            track.Lyrics = new NoneLyric(ex.Message);
        }

        return track;
    }
}
