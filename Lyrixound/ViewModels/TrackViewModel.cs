using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Lyrixound.ViewModels
{
    public class TrackViewModel : ObservableObject
    {
        private readonly Track _track;

        public string Artist
        {
            get => _track.Artist;
            set => SetProperty(Artist, value, _track, (t, v) => t.Artist = v);
        }

        public string Title
        {
            get => _track.Title;
            set => SetProperty(Title, value, _track, (t, v) => t.Title = v);
        }

        public ILyric Lyrics
        {
            get => _track.Lyrics;
            set => SetProperty(Lyrics, value, _track, (t, v) => t.Lyrics = v);
        }

        public TrackViewModel(Track track)
        {
            _track = track;
        }
    }
}
