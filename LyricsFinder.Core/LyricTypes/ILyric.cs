using System;
using System.Collections.Generic;

namespace LyricsFinder.Core.LyricTypes
{
    public interface ILyric
    {
        string Text { get; set; }
        IEnumerable<Uri> Links { get; set; }
        Uri Source { get; set; }
    }
}
