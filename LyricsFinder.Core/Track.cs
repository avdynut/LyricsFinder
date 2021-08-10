using LyricsFinder.Core.LyricTypes;
using System;
using System.Collections.Generic;

namespace LyricsFinder.Core
{
    public class Track : TrackInfo, IEquatable<Track>
    {
        public ILyric Lyrics { get; set; }
        public IEnumerable<string> Genres { get; set; }
        public object Thumbnail { get; set; }

        public bool IsTrackEmpty => this == new Track();

        public Track()
        {
        }

        public Track(TrackInfo trackInfo)
        {
            Artist = trackInfo.Artist;
            Title = trackInfo.Title;
            Album = trackInfo.Album;
        }

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

        public TrackInfo ToTrackInfo()
        {
            return new TrackInfo { Title = Title, Artist = Artist, Album = Album };
        }
    }
}
