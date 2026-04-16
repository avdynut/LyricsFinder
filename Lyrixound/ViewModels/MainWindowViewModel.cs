using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using LyricsProviders;
using LyricsProviders.DirectoriesProvider;
using Lyrixound.Services;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using SmtcWatcher;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Windows.System;

namespace Lyrixound.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly DirectoriesProviderSettings _directoriesSettings;
        private readonly CyclicalSmtcWatcher _musicWatcher;
        private readonly MultiTrackInfoProvider _trackInfoProvider;
        private readonly DispatcherTimer _progressTimer;
        private readonly AudioRecognitionService _audioRecognitionService;
        private TimeSpan _lastKnownPosition;
        private DateTimeOffset _lastPositionUpdateTime;

        public TrackViewModel Track { get; }

        private bool _searchInProgress;
        public bool SearchInProgress
        {
            get => _searchInProgress;
            set => SetProperty(ref _searchInProgress, value);
        }

        private bool _recognizeInProgress;
        public bool RecognizeInProgress
        {
            get => _recognizeInProgress;
            set => SetProperty(ref _recognizeInProgress, value);
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
        public ICommand RecognizeSongCommand { get; }

        public LyricsSettingsViewModel LyricsSettings { get; }

        public MainWindowViewModel(
            CyclicalSmtcWatcher musicWatcher,
            MultiTrackInfoProvider trackInfoProvider,
            DirectoriesProviderSettings directoriesSettings,
            LyricsSettingsViewModel lyricsSettings)
        {
            _musicWatcher = musicWatcher;
            _trackInfoProvider = trackInfoProvider;
            _directoriesSettings = directoriesSettings;
            LyricsSettings = lyricsSettings;
            Track = new TrackViewModel(new Track(), lyricsSettings);
            _audioRecognitionService = new AudioRecognitionService();

            _musicWatcher.TrackChanged += OnWatcherTrackChanged;
            _musicWatcher.TrackProgressChanged += OnTrackProgressChanged;
            _musicWatcher.PlayerStateChanged += OnPlayerStateChanged;

            // Timer to interpolate position between SMTC updates
            _progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _progressTimer.Tick += OnProgressTimerTick;

            FindLyricsCommand = new DelegateCommand(async () => await FindLyricsAsync(), CanFindLyrics)
                .ObservesProperty(() => Track.Artist).ObservesProperty(() => Track.Title).ObservesProperty(() => SearchInProgress).ObservesProperty(() => RecognizeInProgress);

            OpenLyricsCommand = new DelegateCommand(async () => await OpenLyricsAsync(), () => Track.Lyrics?.Source != null)
                .ObservesProperty(() => Track.Lyrics);

            OpenWebsiteCommand = new DelegateCommand(async () => await OpenWebsiteAsync());

            RecognizeSongCommand = new DelegateCommand(async () => await RecognizeSongAsync(), () => !RecognizeInProgress && !SearchInProgress)
                .ObservesProperty(() => RecognizeInProgress).ObservesProperty(() => SearchInProgress);
        }

        private bool CanFindLyrics() => !string.IsNullOrEmpty(Track.Title) && !SearchInProgress && !RecognizeInProgress;

        private async Task FindLyricsAsync(TrackInfo trackInfo)
        {
            try
            {
                SearchInProgress = true;
                ProviderName = null;
                var foundTrack = await _trackInfoProvider.FindTrackAsync(trackInfo);

                Track.Lyrics = foundTrack?.Lyrics;
                if (Track.Lyrics?.Text?.Length > 0)
                {
                    _logger.Debug($"Found lyrics for {foundTrack}");

                    var fileName = DirectoriesTrackInfoProvider.GetFileName(_directoriesSettings.LyricsFileNamePattern, foundTrack);
                    var lyricsDirectory = _directoriesSettings.LyricsDirectories.FirstOrDefault();

                    if (!string.IsNullOrEmpty(lyricsDirectory))
                    {
                        string fileExtension;
                        if (Track.Lyrics is SyncedLyric syncedLyric && syncedLyric.Type == SyncedLyricType.Lrc)
                        {
                            fileExtension = ".lrc";
                        }
                        else
                        {
                            fileExtension = ".txt";
                        }

                        var file = Path.Combine(lyricsDirectory, fileName + fileExtension);

                        if (!File.Exists(file))
                        {
                            Directory.CreateDirectory(lyricsDirectory);
                            File.WriteAllText(file, Track.Lyrics.Text);
                            _logger.Info($"Saved {file}");
                        }
                    }

                    ProviderName = _trackInfoProvider.CurrentProvider?.DisplayName;
                }
                else
                {
                    _logger.Debug("Lyrics not found");
                    ProviderName = null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error finding lyrics for {trackInfo}", trackInfo);
            }
            finally
            {
                SearchInProgress = false;
            }
        }

        private async Task<RecognizedTrackInfo> RecognizeFromAudioAsync()
        {
            try
            {
                RecognizeInProgress = true;
                var trackInfo = await _audioRecognitionService.RecognizeSongFromSystemAudioAsync(3);

                if (trackInfo.IsRecognized)
                {
                    _logger.Info($"Recognized: {trackInfo.Artist} - {trackInfo.Title}");
                    return trackInfo;
                }

                _logger.Warn("Could not recognize the song");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during audio recognition");
                return null;
            }
            finally
            {
                RecognizeInProgress = false;
            }
        }

        private async void OnWatcherTrackChanged(object sender, Track track)
        {
            try
            {
                _logger.Debug($"Track changed {_musicWatcher.PlayerId} - {_musicWatcher.PlayerState}");

                PlayerName = _musicWatcher.PlayerId;
                Track.Artist = track.Artist;
                Track.Title = track.Title;
                Track.Lyrics = track.Lyrics;

                var searchTask = FindLyricsAsync(track.ToTrackInfo());
                var recognizeTask = RecognizeFromAudioAsync();

                await Task.WhenAll(searchTask, recognizeTask);

                if (Track.Lyrics?.Text?.Length > 0)
                    return;

                var recognized = recognizeTask.Result;
                if (recognized != null)
                {
                    _logger.Info($"Metadata search found no lyrics, trying recognized: {recognized.Artist} - {recognized.Title}");
                    Track.Artist = recognized.Artist;
                    Track.Title = recognized.Title;
                    await FindLyricsAsync(new TrackInfo { Artist = recognized.Artist, Title = recognized.Title });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling track change");
            }
        }

        private void OnTrackProgressChanged(TimeSpan progress, DateTimeOffset lastUpdatedTime)
        {
            // Store the last known position from SMTC
            _lastKnownPosition = progress;
            _lastPositionUpdateTime = lastUpdatedTime;
            Track.CurrentPosition = progress;
            //_logger.Info($"Track progress: {progress}");
        }

        private void OnPlayerStateChanged(object sender, PlayerState state)
        {
            // Start/stop the interpolation timer based on player state
            if (state == PlayerState.Playing)
            {
                _progressTimer.Start();
            }
            else
            {
                _progressTimer.Stop();
            }
        }

        private void OnProgressTimerTick(object sender, EventArgs e)
        {
            // Interpolate position when playing
            if (_musicWatcher.PlayerState == PlayerState.Playing && Track.HasSyncedLyrics)
            {
                var elapsed = DateTime.UtcNow - _lastPositionUpdateTime;
                var interpolatedPosition = _lastKnownPosition + elapsed;
                Track.CurrentPosition = interpolatedPosition;
                //_logger.Info($"Track progress: {interpolatedPosition}");
            }
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

        private async Task RecognizeSongAsync()
        {
            var recognized = await RecognizeFromAudioAsync();
            if (recognized != null)
            {
                Track.Artist = recognized.Artist;
                Track.Title = recognized.Title;
                await FindLyricsAsync();
            }
        }

        public void Dispose()
        {
            _progressTimer?.Stop();

            if (_musicWatcher != null)
            {
                _musicWatcher.TrackChanged -= OnWatcherTrackChanged;
                _musicWatcher.TrackProgressChanged -= OnTrackProgressChanged;
                _musicWatcher.PlayerStateChanged -= OnPlayerStateChanged;
                _musicWatcher.Dispose();
            }
        }
    }
}
