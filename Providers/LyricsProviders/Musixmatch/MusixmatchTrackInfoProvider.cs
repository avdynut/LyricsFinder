using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using NLog;
using System;
using System.Net;
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
            var messageProperty = doc.RootElement.GetProperty("message");

            if (messageProperty
                .GetProperty("header")
                .GetProperty("status_code")
                .GetInt32() == (int)HttpStatusCode.OK &&
                messageProperty.TryGetProperty("body", out var bodyProperty) &&
                bodyProperty.TryGetProperty("lyrics", out var lyricsProperty))
            {
                var lyrics = lyricsProperty.GetProperty("lyrics_body").GetString();
                track.Lyrics = new UnsyncedLyric(lyrics);
            }
            else
            {
                using var docSearch = await MusixmatchAPI.SearchTracksAsync($"{trackInfo.Artist} {trackInfo.Title}");
                var trackProperty = docSearch.RootElement
                    .GetProperty("message")
                    .GetProperty("body")
                    .GetProperty("track_list")[0]
                    .GetProperty("track");
                var trackId = trackProperty.GetProperty("commontrack_id").GetInt64();

                using var docLyrics = await MusixmatchAPI.GetTrackLyricsAsync(trackId.ToString());
                var lyricsText = docLyrics.RootElement
                    .GetProperty("message")
                    .GetProperty("body")
                    .GetProperty("lyrics")
                    .GetProperty("lyrics_body").ToString();

                var lyrics = new UnsyncedLyric(lyricsText);
                if (trackProperty.TryGetProperty("track_share_url", out var urlProperty) && urlProperty.GetString() is string url)
                {
                    lyrics.Source = new Uri(url);
                }
                track.Lyrics = lyrics;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching lyrics from Musixmatch");
            track.Lyrics = new NoneLyric(ex.Message);
        }

        return track;
    }
}
