using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using MSHTML;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
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
                track.Lyrics = new NoneLyric("No lyrics found");
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

    private static bool TryPickBestSong(
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
            hits.ValueKind != JsonValueKind.Array ||
            hits.GetArrayLength() == 0)
        {
            return false;
        }

        var songs = new List<(JsonElement Result, int Score)>();
        foreach (var hit in hits.EnumerateArray())
        {
            if (!hit.TryGetProperty("type", out var typeProp) ||
                !string.Equals(typeProp.GetString(), "song", StringComparison.OrdinalIgnoreCase) ||
                !hit.TryGetProperty("result", out var result))
            {
                continue;
            }

            songs.Add((result, ScoreSong(result, trackInfo)));
        }

        if (songs.Count == 0)
            return false;

        var best = songs.OrderByDescending(s => s.Score).First();
        if (best.Score <= 0)
            best = songs[0];

        var song = best.Result;
        songUrl = song.GetProperty("url").GetString();
        matchedTitle = song.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null;
        matchedArtist = song.TryGetProperty("primary_artist", out var artistProp) &&
                        artistProp.TryGetProperty("name", out var artistNameProp)
            ? artistNameProp.GetString()
            : null;

        return !string.IsNullOrEmpty(songUrl);
    }

    private static int ScoreSong(JsonElement result, TrackInfo trackInfo)
    {
        var title = result.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "" : "";
        var artist = result.TryGetProperty("primary_artist", out var artistProp) &&
                     artistProp.TryGetProperty("name", out var artistNameProp)
            ? artistNameProp.GetString() ?? ""
            : "";

        var score = 0;
        if (NamesMatch(title, trackInfo.Title))
            score += 3;
        else if (ContainsNormalized(title, trackInfo.Title) || ContainsNormalized(trackInfo.Title, title))
            score += 1;

        if (NamesMatch(artist, trackInfo.Artist))
            score += 3;
        else if (ContainsNormalized(artist, trackInfo.Artist) || ContainsNormalized(trackInfo.Artist, artist))
            score += 1;

        return score;
    }

    private static bool NamesMatch(string left, string right) =>
        string.Equals(NormalizeName(left), NormalizeName(right), StringComparison.Ordinal);

    private static bool ContainsNormalized(string haystack, string needle)
    {
        var h = NormalizeName(haystack);
        var n = NormalizeName(needle);
        return !string.IsNullOrEmpty(n) && h.Contains(n, StringComparison.Ordinal);
    }

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Trim().ToLowerInvariant();
        normalized = Regex.Replace(normalized, @"\s+", " ");
        return normalized;
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
