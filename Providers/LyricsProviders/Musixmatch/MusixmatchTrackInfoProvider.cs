using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using NLog;
using LyricsProviders;
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
            // Prefer macro.subtitles.get (artist+title match + lyrics in one call).
            // Fall back to track.search for queries the matcher misses (e.g. some Cyrillic titles).
            string commontrackId;
            string mxmTrackId;
            bool hasRichsync;
            bool? hasSubtitles;
            Uri sourceUri;
            string lyricsFromMacro;
            string subtitleFromMacro;

            using (var macroDoc = await MusixmatchAPI.GetMacroSubtitlesAsync(trackInfo.Artist, trackInfo.Title))
            {
                if (TryReadMatchedTrack(macroDoc.RootElement, out commontrackId, out mxmTrackId, out hasRichsync, out hasSubtitles, out sourceUri, out lyricsFromMacro, out subtitleFromMacro))
                {
                    // Matched via macro.
                }
                else
                {
                    lyricsFromMacro = null;
                    subtitleFromMacro = null;
                    using var searchDoc = await MusixmatchAPI.SearchTracksAsync($"{trackInfo.Artist} {trackInfo.Title}");
                    if (!TryPickBestSearchTrack(searchDoc.RootElement, trackInfo, out commontrackId, out mxmTrackId, out hasRichsync, out hasSubtitles, out sourceUri))
                    {
                        throw new Exception("Musixmatch search did not match the requested track.");
                    }
                }
            }

            if (hasRichsync)
            {
                var syncedLyrics = await TryGetSyncedLyrics(commontrackId);
                if (syncedLyrics != null)
                {
                    syncedLyrics.Source = sourceUri;
                    track.Lyrics = syncedLyrics;
                    return track;
                }
            }

            var subtitleText = subtitleFromMacro;
            if (string.IsNullOrEmpty(subtitleText) && hasSubtitles != false)
            {
                subtitleText = await TryGetSubtitleLyricsAsync(commontrackId, mxmTrackId);
            }

            if (!string.IsNullOrEmpty(subtitleText))
            {
                track.Lyrics = new SyncedLyric(subtitleText, SyncedLyricType.Lrc) { Source = sourceUri };
                return track;
            }

            var lyricsText = lyricsFromMacro;
            if (string.IsNullOrEmpty(lyricsText))
            {
                lyricsText = await TryGetUnsyncedLyricsAsync(commontrackId, mxmTrackId);
            }

            if (string.IsNullOrEmpty(lyricsText))
            {
                throw new Exception("Musixmatch returned no lyrics for the matched track.");
            }

            track.Lyrics = new UnsyncedLyric(lyricsText) { Source = sourceUri };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching lyrics from Musixmatch");
            track.Lyrics = new NoneLyric(ex.Message);
        }

        return track;
    }

    private async Task<string> TryGetUnsyncedLyricsAsync(string commontrackId, string trackId)
    {
        if (!string.IsNullOrEmpty(commontrackId))
        {
            using var doc = await MusixmatchAPI.GetTrackLyricsAsync(commontrackId: commontrackId);
            var text = TryReadLyricsBody(doc.RootElement);
            if (!string.IsNullOrEmpty(text))
                return text;
        }

        if (!string.IsNullOrEmpty(trackId))
        {
            using var doc = await MusixmatchAPI.GetTrackLyricsAsync(trackId: trackId);
            var text = TryReadLyricsBody(doc.RootElement);
            if (!string.IsNullOrEmpty(text))
                return text;
        }

        return null;
    }

    private async Task<string> TryGetSubtitleLyricsAsync(string commontrackId, string trackId)
    {
        try
        {
            if (!string.IsNullOrEmpty(commontrackId))
            {
                using var doc = await MusixmatchAPI.GetTrackSubtitleAsync(commontrackId: commontrackId);
                var text = TryReadSubtitleBody(doc.RootElement);
                if (!string.IsNullOrEmpty(text))
                    return text;
            }

            if (!string.IsNullOrEmpty(trackId))
            {
                using var doc = await MusixmatchAPI.GetTrackSubtitleAsync(trackId: trackId);
                var text = TryReadSubtitleBody(doc.RootElement);
                if (!string.IsNullOrEmpty(text))
                    return text;
            }
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Could not fetch line-synced subtitles, will fall back to unsynced");
        }

        return null;
    }

    /// <summary>
    /// Error responses use <c>"body": []</c>; success uses an object with <c>lyrics</c>.
    /// </summary>
    private static string TryReadLyricsBody(JsonElement root)
    {
        if (!root.TryGetProperty("message", out var message) ||
            !message.TryGetProperty("header", out var header) ||
            !header.TryGetProperty("status_code", out var statusProp) ||
            statusProp.GetInt32() != (int)HttpStatusCode.OK)
        {
            return null;
        }

        if (!message.TryGetProperty("body", out var body) || body.ValueKind != JsonValueKind.Object)
            return null;

        if (!body.TryGetProperty("lyrics", out var lyrics) ||
            !lyrics.TryGetProperty("lyrics_body", out var lyricsBody))
        {
            return null;
        }

        return lyricsBody.GetString();
    }

    /// <summary>
    /// Reads line-synced LRC from <c>track.subtitles.get</c> (macro) or <c>track.subtitle.get</c>.
    /// </summary>
    private static string TryReadSubtitleBody(JsonElement root)
    {
        if (!root.TryGetProperty("message", out var message) ||
            !message.TryGetProperty("header", out var header) ||
            !header.TryGetProperty("status_code", out var statusProp) ||
            statusProp.GetInt32() != (int)HttpStatusCode.OK)
        {
            return null;
        }

        if (!message.TryGetProperty("body", out var body) || body.ValueKind != JsonValueKind.Object)
            return null;

        if (body.TryGetProperty("subtitle_list", out var subtitleList) &&
            subtitleList.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in subtitleList.EnumerateArray())
            {
                if (item.TryGetProperty("subtitle", out var listSubtitle) &&
                    TryReadSubtitleBodyText(listSubtitle, out var listText))
                {
                    return listText;
                }
            }
        }

        if (body.TryGetProperty("subtitle", out var subtitle) &&
            TryReadSubtitleBodyText(subtitle, out var text))
        {
            return text;
        }

        return null;
    }

    private static bool TryReadSubtitleBodyText(JsonElement subtitle, out string text)
    {
        text = null;
        if (!subtitle.TryGetProperty("subtitle_body", out var subtitleBody) ||
            subtitleBody.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        text = subtitleBody.GetString();
        if (string.IsNullOrEmpty(text) || !LrcParser.IsLrcFormat(text))
        {
            text = null;
            return false;
        }

        return true;
    }

    private static bool? TryReadHasFlag(JsonElement trackProperty, string propertyName)
    {
        if (!trackProperty.TryGetProperty(propertyName, out var flagProp))
            return null;

        if (flagProp.ValueKind == JsonValueKind.Number && flagProp.TryGetInt32(out var flag))
            return flag == 1;

        return null;
    }

    private static bool TryReadMatchedTrack(
        JsonElement root,
        out string commontrackId,
        out string trackId,
        out bool hasRichsync,
        out bool? hasSubtitles,
        out Uri sourceUri,
        out string lyricsBody,
        out string subtitleBody)
    {
        commontrackId = null;
        trackId = null;
        hasRichsync = false;
        hasSubtitles = null;
        sourceUri = null;
        lyricsBody = null;
        subtitleBody = null;

        if (!root.TryGetProperty("message", out var message) ||
            !message.TryGetProperty("body", out var body) ||
            body.ValueKind != JsonValueKind.Object ||
            !body.TryGetProperty("macro_calls", out var macroCalls) ||
            !macroCalls.TryGetProperty("matcher.track.get", out var matcherCall))
        {
            return false;
        }

        var matcherMessage = matcherCall.GetProperty("message");
        if (matcherMessage.GetProperty("header").GetProperty("status_code").GetInt32() != (int)HttpStatusCode.OK)
            return false;

        if (!matcherMessage.TryGetProperty("body", out var matcherBody) ||
            matcherBody.ValueKind != JsonValueKind.Object ||
            !matcherBody.TryGetProperty("track", out var trackProperty))
        {
            return false;
        }

        commontrackId = trackProperty.GetProperty("commontrack_id").GetInt64().ToString();
        if (trackProperty.TryGetProperty("track_id", out var trackIdProp))
            trackId = trackIdProp.GetInt64().ToString();

        hasRichsync = TryReadHasFlag(trackProperty, "has_richsync") == true;
        hasSubtitles = TryReadHasFlag(trackProperty, "has_subtitles");
        if (trackProperty.TryGetProperty("track_share_url", out var urlProperty) &&
            urlProperty.GetString() is string url)
        {
            sourceUri = new Uri(url);
        }

        if (macroCalls.TryGetProperty("track.lyrics.get", out var lyricsCall))
        {
            lyricsBody = TryReadLyricsBody(lyricsCall);
        }

        if (macroCalls.TryGetProperty("track.subtitles.get", out var subtitlesCall))
        {
            subtitleBody = TryReadSubtitleBody(subtitlesCall);
        }

        if (string.IsNullOrEmpty(subtitleBody) &&
            macroCalls.TryGetProperty("track.subtitle.get", out var subtitleCall))
        {
            subtitleBody = TryReadSubtitleBody(subtitleCall);
        }

        return true;
    }

    private static bool TryPickBestSearchTrack(
        JsonElement root,
        TrackInfo trackInfo,
        out string commontrackId,
        out string trackId,
        out bool hasRichsync,
        out bool? hasSubtitles,
        out Uri sourceUri)
    {
        commontrackId = null;
        trackId = null;
        hasRichsync = false;
        hasSubtitles = null;
        sourceUri = null;

        if (!root.TryGetProperty("message", out var message) ||
            !message.TryGetProperty("body", out var body) ||
            body.ValueKind != JsonValueKind.Object ||
            !body.TryGetProperty("track_list", out var trackList) ||
            trackList.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        JsonElement? bestTrack = null;
        var bestScore = 0;

        foreach (var item in trackList.EnumerateArray())
        {
            if (!item.TryGetProperty("track", out var trackProperty))
                continue;

            var resultTitle = trackProperty.TryGetProperty("track_name", out var titleProp) ? titleProp.GetString() : null;
            var resultArtist = trackProperty.TryGetProperty("artist_name", out var artistProp) ? artistProp.GetString() : null;
            var score = TrackMatch.Score(resultTitle, resultArtist, trackInfo);
            if (!TrackMatch.IsAcceptable(score) || score <= bestScore)
                continue;

            bestScore = score;
            bestTrack = trackProperty;
        }

        if (bestTrack is not JsonElement chosen)
            return false;

        commontrackId = chosen.GetProperty("commontrack_id").GetInt64().ToString();
        trackId = chosen.TryGetProperty("track_id", out var trackIdProp)
            ? trackIdProp.GetInt64().ToString()
            : null;
        hasRichsync = TryReadHasFlag(chosen, "has_richsync") == true;
        hasSubtitles = TryReadHasFlag(chosen, "has_subtitles");
        if (chosen.TryGetProperty("track_share_url", out var urlProperty) &&
            urlProperty.GetString() is string url)
        {
            sourceUri = new Uri(url);
        }

        return true;
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
            if (body.ValueKind != JsonValueKind.Object ||
                !body.TryGetProperty("richsync", out var richsync) ||
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
