using LyricsFinder.Core;
using System;
using System.Text.RegularExpressions;

namespace LyricsProviders;

/// <summary>
/// Scores artist/title search hits so providers can reject a different song
/// even when it happens to have synced lyrics.
/// </summary>
internal static partial class TrackMatch
{
    [GeneratedRegex(@"\s*[\(\[].*?[\)\]]", RegexOptions.CultureInvariant)]
    private static partial Regex ParentheticalRegex();

    [GeneratedRegex(@"\s+(?:feat\.?|ft\.?|featuring)\s+.+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex FeatRegex();

    [GeneratedRegex(@"[^\p{L}\p{N}\s]+", RegexOptions.CultureInvariant)]
    private static partial Regex PunctuationRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex MultiSpaceRegex();

    [GeneratedRegex(@"^the\s+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LeadingTheRegex();

    public static int Score(string resultTitle, string resultArtist, TrackInfo query)
    {
        if (query == null)
            return 0;

        var titleScore = ScorePart(resultTitle, query.Title);
        if (titleScore == 0)
            return 0;

        if (string.IsNullOrWhiteSpace(query.Artist))
            return titleScore;

        var artistScore = ScorePart(resultArtist, query.Artist);
        if (artistScore == 0)
            return 0;

        return titleScore + artistScore;
    }

    public static bool IsAcceptable(int score) => score > 0;

    internal static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var text = value.Trim();
        text = ParentheticalRegex().Replace(text, string.Empty);
        text = FeatRegex().Replace(text, string.Empty);
        text = PunctuationRegex().Replace(text, " ");
        text = LeadingTheRegex().Replace(text, string.Empty);
        text = MultiSpaceRegex().Replace(text, " ").Trim().ToLowerInvariant();
        return text;
    }

    private static int ScorePart(string result, string query)
    {
        var r = Normalize(result);
        var q = Normalize(query);
        if (string.IsNullOrEmpty(q) || string.IsNullOrEmpty(r))
            return 0;

        if (r == q)
            return 3;

        // Prefix match only — unbounded Contains("Love") would hit a different song.
        if (r.StartsWith(q + " ", StringComparison.Ordinal) ||
            q.StartsWith(r + " ", StringComparison.Ordinal))
        {
            return 1;
        }

        return 0;
    }
}
