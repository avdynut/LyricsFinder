using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using MSHTML;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LyricsProviders.Genius;

public class GeniusTrackInfoProvider : ITrackInfoProvider
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public const string Name = "Genius";
    public string DisplayName => Name;

    public async Task<Track> FindTrackAsync(TrackInfo trackInfo)
    {
        var track = new Track(trackInfo);
        try
        {
            if (string.IsNullOrWhiteSpace(trackInfo.Artist) && string.IsNullOrWhiteSpace(trackInfo.Title))
            {
                track.Lyrics = new NoneLyric("Empty track info");
                return track;
            }

            var query = $"{trackInfo.Artist} {trackInfo.Title}".Trim();
            using var searchDoc = await GeniusAPI.SearchSongsAsync(query);
            if (!TryPickBestSong(searchDoc.RootElement, trackInfo, out var songUrl, out var matchedTitle, out var matchedArtist))
            {
                track.Lyrics = new NoneLyric("Genius search did not match the requested track.");
                return track;
            }

            var html = await GeniusAPI.GetSongPageHtmlAsync(songUrl);
            var lyricsText = ParseLyricsHtml(html);
            if (string.IsNullOrWhiteSpace(lyricsText))
            {
                track.Lyrics = new NoneLyric("No lyrics found");
                return track;
            }

            _logger.Debug("Matched Genius song '{0}' by '{1}'", matchedTitle, matchedArtist);
            track.Lyrics = new UnsyncedLyric(lyricsText) { Source = new Uri(songUrl) };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching lyrics from Genius");
            track.Lyrics = new NoneLyric(ex.Message);
        }

        return track;
    }

    internal static bool TryPickBestSong(
        JsonElement root,
        TrackInfo trackInfo,
        out string songUrl,
        out string matchedTitle,
        out string matchedArtist)
    {
        songUrl = null;
        matchedTitle = null;
        matchedArtist = null;

        if (!root.TryGetProperty("response", out var response) ||
            !response.TryGetProperty("hits", out var hits) ||
            hits.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        JsonElement? bestResult = null;
        var bestScore = 0;

        foreach (var hit in hits.EnumerateArray())
        {
            if (!hit.TryGetProperty("type", out var typeProp) ||
                !string.Equals(typeProp.GetString(), "song", StringComparison.OrdinalIgnoreCase) ||
                !hit.TryGetProperty("result", out var result))
            {
                continue;
            }

            var title = result.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null;
            var artist = TryReadPrimaryArtist(result);
            var score = TrackMatch.Score(title, artist, trackInfo);
            if (!TrackMatch.IsAcceptable(score) || score <= bestScore)
                continue;

            bestScore = score;
            bestResult = result;
        }

        if (bestResult is not JsonElement song)
            return false;

        songUrl = song.GetProperty("url").GetString();
        matchedTitle = song.TryGetProperty("title", out var matchedTitleProp) ? matchedTitleProp.GetString() : null;
        matchedArtist = TryReadPrimaryArtist(song);

        return !string.IsNullOrEmpty(songUrl);
    }

    private static string TryReadPrimaryArtist(JsonElement result)
    {
        return result.TryGetProperty("primary_artist", out var artistProp) &&
               artistProp.TryGetProperty("name", out var artistNameProp)
            ? artistNameProp.GetString()
            : null;
    }

    private static string ParseLyricsHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return null;

        var doc = (IHTMLDocument2)new HTMLDocument();
        doc.write(html);
        var htmlDoc = (HTMLDocument)doc;

        var sb = new StringBuilder();
        foreach (IHTMLElement element in htmlDoc.getElementsByTagName("div"))
        {
            var attr = element.getAttribute("data-lyrics-container", 0);
            if (attr == null || !string.Equals(attr.ToString(), "true", StringComparison.OrdinalIgnoreCase))
                continue;

            RemoveExcludedNodes(element);
            var text = element.innerText?.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                if (sb.Length > 0)
                    sb.AppendLine();
                sb.AppendLine(text);
            }
        }

        var lyrics = sb.ToString().Trim();
        if (string.IsNullOrEmpty(lyrics))
            return null;

        // Collapse huge blank runs from Genius layout spacing.
        lyrics = Regex.Replace(lyrics, @"[ \t]+\r?\n", "\n");
        lyrics = Regex.Replace(lyrics, @"(\r?\n){3,}", "\n\n");
        return lyrics.Trim();
    }

    private static void RemoveExcludedNodes(IHTMLElement container)
    {
        var toRemove = new List<IHTMLElement>();
        if (container.all is not IHTMLElementCollection all)
            return;

        foreach (IHTMLElement child in all)
        {
            var excl = child.getAttribute("data-exclude-from-selection", 0);
            if (excl != null && string.Equals(excl.ToString(), "true", StringComparison.OrdinalIgnoreCase))
                toRemove.Add(child);
        }

        foreach (var child in toRemove)
        {
            try
            {
                child.outerHTML = string.Empty;
            }
            catch
            {
                // Ignore COM removal failures; lyrics text may include minor header noise.
            }
        }
    }
}
