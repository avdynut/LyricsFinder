using LyricsFinder.Core;
using LyricsProviders;
using LyricsProviders.DirectoriesProvider;
using Lyrixator.Configuration;
using NLog;
using PlayerWatching;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lyrixator.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly DirectoriesProviderSettings _directoriesSettings;
        private readonly MultiPlayerWatcher _playersWatcher;
        private readonly MultiTrackInfoProvider _trackInfoProvider;

        private string _artist;
        public string Artist
        {
            get => _artist;
            set => SetProperty(ref _artist, value);
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _lyrics;
        public string Lyrics
        {
            get => _lyrics;
            set => SetProperty(ref _lyrics, value);
        }

        private bool _searchInProgress;
        public bool SearchInProgress
        {
            get => _searchInProgress;
            set => SetProperty(ref _searchInProgress, value);
        }

        private string _playerImage;
        public string PlayerName
        {
            get => _playerImage;
            set => SetProperty(ref _playerImage, value);
        }

        private string _providerImage;
        public string ProviderName
        {
            get => _providerImage;
            set => SetProperty(ref _providerImage, value);
        }

        public ICommand FindLyricsCommand { get; }

        public LyricsSettings LyricsSettings { get; }

        public MainWindowViewModel(
            MultiPlayerWatcher playersWatcher,
            MultiTrackInfoProvider trackInfoProvider,
            DirectoriesProviderSettings directoriesSettings,
            LyricsSettings lyricsSettings)
        {
            _playersWatcher = playersWatcher;
            _trackInfoProvider = trackInfoProvider;
            _directoriesSettings = directoriesSettings;
            LyricsSettings = lyricsSettings;

            _playersWatcher.TrackChanged += OnWatcherTrackChanged;
            _playersWatcher.Initialize();

            FindLyricsCommand = new DelegateCommand(async () => await FindLyricsAsync(), CanFindLyrics)
                .ObservesProperty(() => Artist).ObservesProperty(() => Title).ObservesProperty(() => SearchInProgress);
        }

        private bool CanFindLyrics() => !string.IsNullOrEmpty(Artist) && !string.IsNullOrEmpty(Title) && !SearchInProgress;

        private async Task UpdateTrackInfoAsync(Track track)
        {
            _logger.Info("Update track info {track}", track);

            Title = track.Title;
            Artist = track.Artist;
            Lyrics = track.Lyrics?.Text;

            await FindLyricsAsync(track.ToTrackInfo());
        }

        private async Task FindLyricsAsync(TrackInfo trackInfo)
        {
            SearchInProgress = true;
            ProviderName = null;
            var foundTrack = await _trackInfoProvider.FindTrackAsync(trackInfo);
            SearchInProgress = false;

            Lyrics = foundTrack.Lyrics.Text;

            if (Lyrics?.Length > 0)
            {
                _logger.Debug($"Found lyrics for {foundTrack}");

                var fileName = DirectoriesTrackInfoProvider.GetFileName(_directoriesSettings.LyricsFileNamePattern, foundTrack);
                var lyricsDirectory = _directoriesSettings.LyricsDirectories.First();
                var file = Path.Combine(lyricsDirectory, fileName + ".txt");

                if (!File.Exists(file))
                {
                    Directory.CreateDirectory(lyricsDirectory);
                    File.WriteAllText(file, Lyrics);
                }

                ProviderName = _trackInfoProvider.CurrentProvider?.DisplayName;
            }
            else
            {
                _logger.Debug("Lyrics not found");
                ProviderName = null;
            }
        }

        private async void OnWatcherTrackChanged(object sender, Track track)
        {
            _logger.Debug($"Track changed {_playersWatcher.ActualWatcher?.DisplayName} - {_playersWatcher.PlayerState}");

            PlayerName = _playersWatcher.ActualWatcher?.DisplayName;
            await UpdateTrackInfoAsync(track);
        }

        private async Task FindLyricsAsync()
        {
            var trackInfo = new TrackInfo { Artist = Artist, Title = Title };
            await FindLyricsAsync(trackInfo);
        }

        public void Dispose()
        {
            if (_playersWatcher != null)
            {
                _playersWatcher.TrackChanged -= OnWatcherTrackChanged;
                _playersWatcher.Dispose();
            }
        }
    }
}
