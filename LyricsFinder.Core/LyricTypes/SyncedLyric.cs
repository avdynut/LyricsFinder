using System;
using System.Collections.Generic;

namespace LyricsFinder.Core.LyricTypes
{
    public class SyncedLyric : ILyric
    {
        public string Text { get; set; }
        public IEnumerable<Uri> Links { get; set; }
        public Uri Source { get; set; }

        public SyncedLyricType Type { get; }

        public SyncedLyric(string text, SyncedLyricType type)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Type = type;
        }
    }

    public enum SyncedLyricType
    {
        Lrc,
        Srt
    }
}
