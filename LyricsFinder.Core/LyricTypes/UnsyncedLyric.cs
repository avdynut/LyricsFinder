using System;
using System.Collections.Generic;

namespace LyricsFinder.Core.LyricTypes
{
    public class UnsyncedLyric : ILyric
    {
        public string Text { get; set; }
        public IEnumerable<Uri> Links { get; set; }
        public Uri Source { get; set; }

        public UnsyncedLyric(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}
