using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using NLog;
using System;
using System.Net;
using System.Text;
using System.Text.Json;
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
            // Use track.search to get track metadata and check if synced lyrics available
            using var docSearch = await MusixmatchAPI.SearchTracksAsync($"{trackInfo.Artist} {trackInfo.Title}");
            var trackProperty = docSearch.RootElement
                .GetProperty("message")
                .GetProperty("body")
                .GetProperty("track_list")[0]
                .GetProperty("track");

            var trackId = trackProperty.GetProperty("commontrack_id").GetInt64().ToString();

            // Check if synced lyrics are available before making the API call
            var hasRichsync = trackProperty.TryGetProperty("has_richsync", out var hasRichsyncProp) &&
                              hasRichsyncProp.GetInt32() == 1;

            if (hasRichsync)
            {
                var syncedLyrics = await TryGetSyncedLyrics(trackId);
                if (syncedLyrics != null)
                {
                    if (trackProperty.TryGetProperty("track_share_url", out var urlProperty) && urlProperty.GetString() is string url)
                    {
                        syncedLyrics.Source = new Uri(url);
                    }
                    track.Lyrics = syncedLyrics;
                    return track;
                }
            }

            // Fall back to unsynced lyrics
            using var docLyrics = await MusixmatchAPI.GetTrackLyricsAsync(trackId);
            var lyricsText = docLyrics.RootElement
                .GetProperty("message")
                .GetProperty("body")
                .GetProperty("lyrics")
                .GetProperty("lyrics_body").ToString();

            var lyrics = new UnsyncedLyric(lyricsText);
            if (trackProperty.TryGetProperty("track_share_url", out var urlProperty2) && urlProperty2.GetString() is string url2)
            {
                lyrics.Source = new Uri(url2);
            }
            track.Lyrics = lyrics;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching lyrics from Musixmatch");
            track.Lyrics = new NoneLyric(ex.Message);
        }

        return track;
    }

    private async Task<SyncedLyric> TryGetSyncedLyrics(string commontrackId)
    {
        try
        {
            using var richsyncDoc = await MusixmatchAPI.GetTrackRichsyncAsync(commontrackId, null, null);
            var message = richsyncDoc.RootElement.GetProperty("message");

            if (message.GetProperty("header").GetProperty("status_code").GetInt32() != (int)HttpStatusCode.OK)
            {
                return null;
            }

            var body = message.GetProperty("body");
            if (!body.TryGetProperty("richsync", out var richsync) ||
                !richsync.TryGetProperty("richsync_body", out var richsyncBody) ||
                richsyncBody.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            var richsyncJson = richsyncBody.GetString();
            if (string.IsNullOrEmpty(richsyncJson))
            {
                return null;
            }

            // Convert Musixmatch richsync format to LRC format
            var lrcText = ConvertRichsyncToLrc(richsyncJson);
            if (!string.IsNullOrEmpty(lrcText))
            {
                return new SyncedLyric(lrcText, SyncedLyricType.Lrc);
            }
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Could not fetch synced lyrics, will fall back to unsynced");
        }

        return null;
    }

    /// <summary>
    /// Converts Musixmatch richsync JSON format to standard LRC format
    /// </summary>
    private string ConvertRichsyncToLrc(string richsyncJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(richsyncJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var lrcBuilder = new StringBuilder();
            foreach (var line in doc.RootElement.EnumerateArray())
            {
                if (!line.TryGetProperty("ts", out var tsProperty))
                    continue;

                var timeSpan = TimeSpan.FromSeconds(tsProperty.GetDouble());
                var minutes = (int)timeSpan.TotalMinutes;
                var seconds = timeSpan.Seconds;
                var centiseconds = timeSpan.Milliseconds / 10;

                // Extract text from either "l" property or "x" array
                string text = null;
                if (line.TryGetProperty("l", out var lProperty) && lProperty.ValueKind == JsonValueKind.Array)
                {
                    // Text is in array format, concatenate
                    var textBuilder = new StringBuilder();
                    foreach (var item in lProperty.EnumerateArray())
                    {
                        if (item.TryGetProperty("c", out var charProp))
                        {
                            textBuilder.Append(charProp.GetString());
                        }
                    }
                    text = textBuilder.ToString();
                }
                else if (lProperty.ValueKind == JsonValueKind.String)
                {
                    text = lProperty.GetString();
                }

                if (!string.IsNullOrEmpty(text))
                {
                    lrcBuilder.AppendLine($"[{minutes:D2}:{seconds:D2}.{centiseconds:D2}]{text}");
                }
            }

            return lrcBuilder.ToString();
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Error converting richsync to LRC format");
            return null;
        }
    }
}
