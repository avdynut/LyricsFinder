using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using Prism.Mvvm;

namespace Lyrixator.ViewModels
{
    public class TrackViewModel : BindableBase
    {
        private readonly Track _track;

        public string Artist
        {
            get => _track.Artist;
            set
            {
                _track.Artist = value;
                RaisePropertyChanged();
            }
        }

        public string Title
        {
            get => _track.Title;
            set
            {
                _track.Title = value;
                RaisePropertyChanged();
            }
        }

        public ILyric Lyrics
        {
            get => _track.Lyrics;
            set
            {
                _track.Lyrics = value;
                RaisePropertyChanged();
            }
        }

        public TrackViewModel(Track track)
        {
            _track = track;
        }
    }
}
