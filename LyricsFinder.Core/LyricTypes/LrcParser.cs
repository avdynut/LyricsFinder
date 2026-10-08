using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LyricsFinder.Core.LyricTypes
{
    /// <summary>
    /// Utility class for parsing and validating LRC (Lyrics) files
    /// </summary>
    public static partial class LrcParser
    {
        // Regex pattern to match LRC timestamp format: [mm:ss.xx] or [mm:ss]
        [GeneratedRegex(@"\[(\d{2,3}):(\d{2})(?:\.(\d{2,3}))?\]")]
        private static partial Regex TimestampPattern();

        // Regex pattern to match metadata tags like [ar:Artist] or [ti:Title]
        [GeneratedRegex(@"\[([a-z]{2}):(.*?)\]", RegexOptions.IgnoreCase)]
        private static partial Regex MetadataPattern();

        /// <summary>
        /// Checks if the given text contains LRC format timestamps
        /// </summary>
        /// <param name="text">Text to check</param>
        /// <returns>True if text contains LRC timestamps, false otherwise</returns>
        public static bool IsLrcFormat(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            // Look for at least one timestamp pattern
            return TimestampPattern().IsMatch(text);
        }

        /// <summary>
        /// Parses LRC format text into structured lyric lines with timestamps
        /// </summary>
        /// <param name="lrcText">LRC format text</param>
        /// <returns>List of parsed lyric lines</returns>
        public static List<LrcLine> Parse(string lrcText)
        {
            if (string.IsNullOrWhiteSpace(lrcText))
                return new List<LrcLine>();

            var lines = lrcText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var lrcLines = new List<LrcLine>();

            foreach (var line in lines)
            {
                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Skip metadata lines (like [ar:Artist], [ti:Title], etc.)
                if (MetadataPattern().IsMatch(line) && !TimestampPattern().IsMatch(line))
                    continue;

                // Find all timestamps in the line
                var timestamps = TimestampPattern().Matches(line);
                if (timestamps.Count == 0)
                    continue;

                // Get the lyric text (everything after the last timestamp)
                var lastTimestamp = timestamps[timestamps.Count - 1];
                var lyricText = line.Substring(lastTimestamp.Index + lastTimestamp.Length).Trim();

                // Parse each timestamp and create a line for it
                foreach (Match timestamp in timestamps)
                {
                    var minutes = int.Parse(timestamp.Groups[1].Value);
                    var seconds = int.Parse(timestamp.Groups[2].Value);
                    var milliseconds = 0;

                    if (timestamp.Groups[3].Success)
                    {
                        var msString = timestamp.Groups[3].Value;
                        // Handle both 2-digit (centiseconds) and 3-digit (milliseconds) formats
                        if (msString.Length == 2)
                        {
                            milliseconds = int.Parse(msString) * 10; // centiseconds to milliseconds
                        }
                        else
                        {
                            milliseconds = int.Parse(msString);
                        }
                    }

                    var timeSpan = new TimeSpan(0, 0, minutes, seconds, milliseconds);
                    lrcLines.Add(new LrcLine(timeSpan, lyricText));
                }
            }

            // Sort by timestamp
            return lrcLines.OrderBy(l => l.Timestamp).ToList();
        }

        /// <summary>
        /// Formats a lyric line with LRC timestamp format
        /// </summary>
        /// <param name="timestamp">Timestamp of the line</param>
        /// <param name="text">Lyric text</param>
        /// <returns>Formatted LRC line</returns>
        public static string FormatLine(TimeSpan timestamp, string text)
        {
            var minutes = (int)timestamp.TotalMinutes;
            var seconds = timestamp.Seconds;
            var centiseconds = timestamp.Milliseconds / 10;

            return $"[{minutes:D2}:{seconds:D2}.{centiseconds:D2}]{text}";
        }
    }

    /// <summary>
    /// Represents a single line in an LRC file with timestamp and text
    /// </summary>
    public class LrcLine
    {
        public TimeSpan Timestamp { get; set; }
        public string Text { get; set; }

        public LrcLine(TimeSpan timestamp, string text)
        {
            Timestamp = timestamp;
            Text = text ?? string.Empty;
        }

        public override string ToString()
        {
            return LrcParser.FormatLine(Timestamp, Text);
        }
    }
}

