using LyricsFinder.Core;
using System;
using System.Collections.Generic;

namespace LyricsFinder.Core.LyricTypes
{
    public class NoneLyric : ILyric
    {
        public string Text { get; set; }
        public IEnumerable<Uri> Links { get; set; }
        public Uri Source { get; set; }

        public string Error { get; }

        public NoneLyric(string error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }
    }
}
