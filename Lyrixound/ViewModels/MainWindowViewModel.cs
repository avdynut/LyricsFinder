using LyricsFinder.Core;
using LyricsProviders;
using LyricsProviders.DirectoriesProvider;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Win10Watcher;
using Windows.System;

namespace Lyrixound.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly DirectoriesProviderSettings _directoriesSettings;
        private readonly NpsmWatcher _musicWatcher;
        private readonly MultiTrackInfoProvider _trackInfoProvider;

        public TrackViewModel Track { get; } = new TrackViewModel(new Track());

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
        public ICommand OpenLyricsCommand { get; }
        public ICommand OpenWebsiteCommand { get; }

        public LyricsSettingsViewModel LyricsSettings { get; }

        public MainWindowViewModel(
            NpsmWatcher musicWatcher,
            MultiTrackInfoProvider trackInfoProvider,
            DirectoriesProviderSettings directoriesSettings,
            LyricsSettingsViewModel lyricsSettings)
        {
            _musicWatcher = musicWatcher;
            _trackInfoProvider = trackInfoProvider;
            _directoriesSettings = directoriesSettings;
            LyricsSettings = lyricsSettings;

            _musicWatcher.TrackChanged += OnWatcherTrackChanged;

            FindLyricsCommand = new DelegateCommand(async () => await FindLyricsAsync(), CanFindLyrics)
                .ObservesProperty(() => Track.Artist).ObservesProperty(() => Track.Title).ObservesProperty(() => SearchInProgress);

            OpenLyricsCommand = new DelegateCommand(async () => await OpenLyricsAsync(), () => Track.Lyrics?.Source != null)
                .ObservesProperty(() => Track.Lyrics);

            OpenWebsiteCommand = new DelegateCommand(async () => await OpenWebsiteAsync());
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
            _logger.Debug($"Track changed {_musicWatcher.PlayerInfo.SourceAppId} - {_musicWatcher.PlayerState}");

            PlayerName = _musicWatcher.PlayerInfo.SourceAppId;
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

        public void Dispose()
        {
            if (_musicWatcher != null)
            {
                _musicWatcher.TrackChanged -= OnWatcherTrackChanged;
                _musicWatcher.Dispose();
            }
        }
    }
}
