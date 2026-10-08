using System;
using System.Text.RegularExpressions;

namespace LyricsFinder.Core
{
    /// <summary>
    /// Strips common YouTube / browser SMTC noise from track titles and artists
    /// (channel suffixes after "|", "Official Video" labels, " - Topic", etc.)
    /// without removing parentheses that are part of a real song name.
    /// </summary>
    public static partial class TrackTextCleaner
    {
        private static readonly char[] PipeSeparators = { '|', '｜' };

        [GeneratedRegex(@"\s")]
        private static partial Regex UnicodeSpaceRegex();

        // Known promo/format labels in () [] {} or fullwidth brackets.
        [GeneratedRegex(
            @"\s*[\(\[\{（【]\s*(?:" +
            @"official\s*(?:music\s*)?(?:lyric\s*)?(?:video|audio)|" +
            @"(?:music\s*)?video|" +
            @"lyric\s*video|" +
            @"official\s*audio|" +
            @"audio(?:\s*only)?|" +
            @"with\s*lyrics?|" +
            @"lyrics?|" +
            @"visualizer|" +
            @"mv|" +
            @"hd|" +
            @"4k|" +
            @"remaster(?:ed)?" +
            @")[^\)\]\}）】]*[\)\]\}）】]",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex NoiseLabelRegex();

        [GeneratedRegex(
            @"\s+(?:official\s*(?:music\s*)?(?:lyric\s*)?(?:video|audio)|lyric\s*video|visualizer|audio\s*only)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex TrailingNoiseRegex();

        [GeneratedRegex(@"\s*-\s*Topic\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex TopicSuffixRegex();

        [GeneratedRegex(@"\s*VEVO\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex VevoSuffixRegex();

        // "PhilWickham" / "Maroon5" after a VEVO suffix — not "BTS".
        [GeneratedRegex(@"(?<=[\p{Ll}\p{N}])(?=\p{Lu})|(?<=\p{L})(?=\p{N})")]
        private static partial Regex CamelCaseSplitRegex();

        [GeneratedRegex(@"\s{2,}")]
        private static partial Regex MultiSpaceRegex();

        [GeneratedRegex(@"[\u200B-\u200D\uFEFF\u2060]")]
        private static partial Regex InvisibleCharsRegex();

        [GeneratedRegex(@"[^\p{L}\p{N}\s]+", RegexOptions.CultureInvariant)]
        private static partial Regex PunctuationRegex();

        // First "Artist - Title" split. Spaces around the dash are required so "X-Ray" stays intact.
        [GeneratedRegex(@"^(.*?)\s*(?:[\-\u2010-\u2015\u2212\uFF0D]|--)+\s+(.*)$")]
        private static partial Regex ArtistTitleSplitRegex();

        /// <summary>
        /// Cleans track text for lyrics lookup. Returns the original value when cleaning
        /// would leave only whitespace.
        /// </summary>
        public static string Clean(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var original = text;
            text = InvisibleCharsRegex().Replace(text, string.Empty);
            text = UnicodeSpaceRegex().Replace(text, " ");

            var pipeIndex = text.IndexOfAny(PipeSeparators);
            if (pipeIndex > 0)
                text = text.Substring(0, pipeIndex);

            text = NoiseLabelRegex().Replace(text, string.Empty);
            text = TrailingNoiseRegex().Replace(text, string.Empty);
            text = TopicSuffixRegex().Replace(text, string.Empty);
            var hadVevo = VevoSuffixRegex().IsMatch(text);
            text = VevoSuffixRegex().Replace(text, string.Empty);
            text = MultiSpaceRegex().Replace(text, " ").Trim();
            if (hadVevo && text.IndexOf(' ') < 0)
                text = CamelCaseSplitRegex().Replace(text, " ");

            return string.IsNullOrWhiteSpace(text) ? original.Trim() : text;
        }

        /// <summary>
        /// Cleans a track title, then drops a duplicated <c>Artist - </c> prefix when the
        /// title already starts with the same artist (common in YouTube / SMTC metadata).
        /// </summary>
        public static string CleanTitle(string title, string artist)
        {
            title = Clean(title);
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(artist))
                return title;

            artist = Clean(artist);
            if (string.IsNullOrWhiteSpace(artist))
                return title;

            return StripRedundantArtistPrefix(title, artist);
        }

        private static string StripRedundantArtistPrefix(string title, string artist)
        {
            var match = ArtistTitleSplitRegex().Match(title);
            if (!match.Success || !NamesEqual(match.Groups[1].Value, artist))
                return title;

            var song = match.Groups[2].Value.Trim();
            return string.IsNullOrWhiteSpace(song) ? title : song;
        }

        private static bool NamesEqual(string left, string right)
        {
            return string.Equals(NormalizeForCompare(left), NormalizeForCompare(right), StringComparison.Ordinal);
        }

        private static string NormalizeForCompare(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var text = value.Trim().ToLowerInvariant();
            text = PunctuationRegex().Replace(text, string.Empty);
            return MultiSpaceRegex().Replace(text, string.Empty).Trim();
        }
    }
}
