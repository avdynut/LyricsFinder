using LyricsFinder.Core;
using LyricsProviders;
using LyricsProviders.DirectoriesProvider;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using NLog;
using SmtcWatcher;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.System;

namespace Lyrixound.ViewModels
{
    public class MainWindowViewModel : ObservableObject, IDisposable
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly DirectoriesProviderSettings _directoriesSettings;
        private readonly MusicWatcher _musicWatcher;
        private readonly MultiTrackInfoProvider _trackInfoProvider;

        public TrackViewModel Track { get; } = new TrackViewModel(new Track());

        private bool _searchInProgress;
        public bool SearchInProgress
        {
            get => _searchInProgress;
            set
            {
                if (SetProperty(ref _searchInProgress, value))
                {
                    Application.Current.Dispatcher.Invoke(FindLyricsCommand.NotifyCanExecuteChanged);
                }
            }
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

        public IRelayCommand FindLyricsCommand { get; }
        public IRelayCommand OpenLyricsCommand { get; }
        public IRelayCommand OpenWebsiteCommand { get; }

        public LyricsSettingsViewModel LyricsSettings { get; }

        public MainWindowViewModel(
            MusicWatcher musicWatcher,
            MultiTrackInfoProvider trackInfoProvider,
            DirectoriesProviderSettings directoriesSettings,
            LyricsSettingsViewModel lyricsSettings)
        {
            _musicWatcher = musicWatcher;
            _trackInfoProvider = trackInfoProvider;
            _directoriesSettings = directoriesSettings;
            LyricsSettings = lyricsSettings;

            _musicWatcher.TrackChanged += OnWatcherTrackChanged;
            Track.PropertyChanged += Track_PropertyChanged;

            FindLyricsCommand = new AsyncRelayCommand(FindLyricsAsync, CanFindLyrics);
            OpenLyricsCommand = new AsyncRelayCommand(OpenLyricsAsync, () => Track.Lyrics?.Source != null);
            OpenWebsiteCommand = new AsyncRelayCommand(OpenWebsiteAsync);
        }

        private bool CanFindLyrics() => !string.IsNullOrEmpty(Track.Title) && !SearchInProgress;

        private async Task UpdateTrackInfoAsync(Track track)
        {
            _logger.Info("Update track info {track}", track);

            Track.Artist = track.Artist;
            Track.Title = track.Title;
            Track.Lyrics = track.Lyrics;

            await FindLyricsAsync(track.ToTrackInfo());
        }

        private async Task FindLyricsAsync(TrackInfo trackInfo)
        {
            SearchInProgress = true;
            ProviderName = null;
            var foundTrack = await _trackInfoProvider.FindTrackAsync(trackInfo);
            SearchInProgress = false;

            Track.Lyrics = foundTrack.Lyrics;
            if (Track.Lyrics.Text?.Length > 0)
            {
                _logger.Debug($"Found lyrics for {foundTrack}");

                var fileName = DirectoriesTrackInfoProvider.GetFileName(_directoriesSettings.LyricsFileNamePattern, foundTrack);
                var lyricsDirectory = _directoriesSettings.LyricsDirectories.First();
                var file = Path.Combine(lyricsDirectory, fileName + ".txt");

                if (!File.Exists(file))
                {
                    Directory.CreateDirectory(lyricsDirectory);
                    File.WriteAllText(file, Track.Lyrics.Text);
                    _logger.Info($"Saved {file}");
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
            _logger.Debug($"Track changed {_musicWatcher.PlayerId} - {_musicWatcher.PlayerState}");

            PlayerName = _musicWatcher.PlayerId;
            await UpdateTrackInfoAsync(track);
        }

        private async Task FindLyricsAsync()
        {
            var trackInfo = new TrackInfo { Artist = Track.Artist, Title = Track.Title };
            await FindLyricsAsync(trackInfo);
        }

        private async Task OpenLyricsAsync()
        {
            try
            {
                await Launcher.LaunchUriAsync(Track.Lyrics.Source);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Cannot open lyrics {Track.Lyrics.Source}");
            }
        }

        private async Task OpenWebsiteAsync()
        {
            try
            {
                await Launcher.LaunchUriAsync(new Uri(App.HelpUrl));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Cannot open website");
            }
        }

        private void Track_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Track.Lyrics):
                    Application.Current.Dispatcher.Invoke(OpenLyricsCommand.NotifyCanExecuteChanged);
                    break;
                case nameof(Track.Title):
                    Application.Current.Dispatcher.Invoke(FindLyricsCommand.NotifyCanExecuteChanged);
                    break;
            }
        }

        public void Dispose()
        {
            if (_musicWatcher != null)
            {
                _musicWatcher.TrackChanged -= OnWatcherTrackChanged;
                _musicWatcher.Dispose();
            }

            Track.PropertyChanged -= Track_PropertyChanged;
        }
    }
}
