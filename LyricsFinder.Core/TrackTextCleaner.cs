using System.Text.RegularExpressions;

namespace LyricsFinder.Core
{
    /// <summary>
    /// Utility class for cleaning track text by removing YouTube channel names, parentheses content, and text after pipe character.
    /// </summary>
    public static class TrackTextCleaner
    {
        /// <summary>
        /// Cleans track text by removing YouTube channel names, parentheses content, and text after pipe character.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <returns>The cleaned text, or the original text if it's null or whitespace.</returns>
        public static string Clean(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Remove text after pipe character (|)
            var pipeIndex = text.IndexOf('|');
            if (pipeIndex > 0)
            {
                text = text.Substring(0, pipeIndex);
            }

            // Remove parentheses and their content, including (*) patterns
            text = Regex.Replace(text, @"\s*\([^)]*\)", "");

            return text.Trim();
        }
    }
}
