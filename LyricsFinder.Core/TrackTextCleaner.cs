using System.Text.RegularExpressions;

namespace LyricsFinder.Core
{
    /// <summary>
    /// Strips common YouTube / browser SMTC noise from track titles and artists
    /// (channel suffixes after "|", "Official Video" labels, " - Topic", etc.)
    /// without removing parentheses that are part of a real song name.
    /// </summary>
    public static class TrackTextCleaner
    {
        private static readonly char[] PipeSeparators = { '|', '｜' };

        // Known promo/format labels in () or [], not arbitrary parenthetical titles.
        private static readonly Regex NoiseLabelRegex = new Regex(
            @"\s*[\(\[](?:" +
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
            @")[^\)\]]*[\)\]]",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex TopicSuffixRegex = new Regex(
            @"\s*-\s*Topic\s*$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex VevoSuffixRegex = new Regex(
            @"\s*VEVO\s*$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex MultiSpaceRegex = new Regex(
            @"\s{2,}",
            RegexOptions.Compiled);

        /// <summary>
        /// Cleans track text for lyrics lookup. Returns the original value when cleaning
        /// would leave only whitespace.
        /// </summary>
        public static string Clean(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var original = text;

            var pipeIndex = text.IndexOfAny(PipeSeparators);
            if (pipeIndex > 0)
                text = text.Substring(0, pipeIndex);

            text = NoiseLabelRegex.Replace(text, string.Empty);
            text = TopicSuffixRegex.Replace(text, string.Empty);
            text = VevoSuffixRegex.Replace(text, string.Empty);
            text = MultiSpaceRegex.Replace(text, " ").Trim();

            return string.IsNullOrWhiteSpace(text) ? original.Trim() : text;
        }
    }
}
