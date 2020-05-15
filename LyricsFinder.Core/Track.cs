using LyricsFinder.Core.LyricTypes;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace LyricsFinder.Core
{
    public class Track : IEquatable<Track>
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public ImageSource AlbumArt { get; set; }
        public int Year { get; set; }
        public TimeSpan Duration { get; set; }
        public Uri Source { get; set; }
        public IEnumerable<string> Genres { get; set; }
        public ILyric Lyrics { get; set; }

        public bool IsTrackEmpty => this == new Track();

        public override string ToString() => $"{Artist} - {Title}";

        public override bool Equals(object obj)
        {
            return Equals(obj as Track);
        }

        public bool Equals(Track other)
        {
            return other != null &&
                   Artist == other.Artist &&
                   Title == other.Title &&
                   Album == other.Album;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Artist, Title, Album);
        }

        public static bool operator ==(Track left, Track right)
        {
            return EqualityComparer<Track>.Default.Equals(left, right);
        }

        public static bool operator !=(Track left, Track right)
        {
            return !(left == right);
        }
    }
}
