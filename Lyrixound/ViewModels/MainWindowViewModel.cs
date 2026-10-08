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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        private int _thumbnailGeneration;
        private int _thumbnailHash;
        private DateTime _thumbnailRefreshUntilUtc;
        private ImageSource _playingThumbnail;
        private bool _showPlayingThumbnail = true;
        private string _playingArtist;
        private string _playingTitle;
        private int _trackSession;

        public TrackViewModel Track { get; }

        private string _lyricsTitle;
        public string LyricsTitle
        {
            get => _lyricsTitle;
            private set
            {
                if (SetProperty(ref _lyricsTitle, value))
                    RaisePropertyChanged(nameof(HasLyricsIdentity));
            }
        }

        private string _lyricsArtist;
        public string LyricsArtist
        {
            get => _lyricsArtist;
            private set
            {
                if (SetProperty(ref _lyricsArtist, value))
                    RaisePropertyChanged(nameof(HasLyricsIdentity));
            }
        }

        public bool HasLyricsIdentity =>
            !string.IsNullOrWhiteSpace(LyricsTitle) || !string.IsNullOrWhiteSpace(LyricsArtist);

        public int TrackSession
        {
            get => _trackSession;
            private set => SetProperty(ref _trackSession, value);
        }

        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get => _thumbnail;
            private set => SetProperty(ref _thumbnail, value);
        }

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
            _musicWatcher.ThumbnailChanged += OnThumbnailChanged;
            _musicWatcher.TrackProgressChanged += OnTrackProgressChanged;
            _musicWatcher.PlayerStateChanged += OnPlayerStateChanged;

            // Timer to interpolate position between SMTC updates
            _progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _progressTimer.Tick += OnProgressTimerTick;

            FindLyricsCommand = new DelegateCommand(async () => await FindLyricsAsync(), CanFindLyrics)
                .ObservesProperty(() => SearchInProgress).ObservesProperty(() => RecognizeInProgress);

            OpenLyricsCommand = new DelegateCommand(async () => await OpenLyricsAsync(), () => Track.Lyrics?.Source != null)
                .ObservesProperty(() => Track.Lyrics);

            OpenWebsiteCommand = new DelegateCommand(async () => await OpenWebsiteAsync());

            RecognizeSongCommand = new DelegateCommand(async () => await RecognizeSongAsync(), () => !RecognizeInProgress && !SearchInProgress)
                .ObservesProperty(() => RecognizeInProgress).ObservesProperty(() => SearchInProgress);
        }

        private bool CanFindLyrics() => !SearchInProgress && !RecognizeInProgress;

        public async Task DetectCurrentTrackAsync()
        {
            try
            {
                if (SearchInProgress || RecognizeInProgress)
                    return;

                var track = _musicWatcher.Track;
                if (string.IsNullOrWhiteSpace(track?.Title))
                {
                    _logger.Debug("No current track in media controls");
                    return;
                }

                _logger.Debug($"Detected current track {track.Artist} - {track.Title}");
                PlayerName = _musicWatcher.PlayerId;
                ApplyCleanedTrackInfo(track.Artist, track.Title);
                RememberPlayingTrack();
                SetShowPlayingThumbnail(true);
                LoadThumbnail(track.Thumbnail);
                await FindLyricsAsync(track.ToTrackInfo());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error detecting current track");
            }
        }

        private async Task FindLyricsAsync(TrackInfo trackInfo)
        {
            try
            {
                SearchInProgress = true;
                ProviderName = null;
                ApplyCleanedTrackInfo(trackInfo.Artist, trackInfo.Title);
                trackInfo = new TrackInfo { Artist = Track.Artist, Title = Track.Title };
                var foundTrack = await _trackInfoProvider.FindTrackAsync(trackInfo);

                Track.Lyrics = foundTrack?.Lyrics;
                SetLyricsIdentity(foundTrack);
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
                var trackInfo = await _audioRecognitionService.RecognizeSongFromSystemAudioAsync(4);

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

                TrackSession++;
                PlayerName = _musicWatcher.PlayerId;
                ApplyCleanedTrackInfo(track.Artist, track.Title);
                RememberPlayingTrack();
                SetShowPlayingThumbnail(true);
                Track.Lyrics = track.Lyrics;
                SetLyricsIdentity(null);
                LoadThumbnail(track.Thumbnail);

                var searchTask = FindLyricsAsync(track.ToTrackInfo());
                var recognizeTask = RecognizeFromAudioAsync();

                await Task.WhenAll(searchTask, recognizeTask);

                if (Track.Lyrics?.Text?.Length > 0)
                    return;

                var recognized = recognizeTask.Result;
                if (recognized != null)
                {
                    _logger.Info($"Metadata search found no lyrics, trying recognized: {recognized.Artist} - {recognized.Title}");
                    ApplyCleanedTrackInfo(recognized.Artist, recognized.Title);
                    RememberPlayingTrack();
                    await FindLyricsAsync(new TrackInfo { Artist = Track.Artist, Title = Track.Title });
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
            if (string.IsNullOrWhiteSpace(Track.Title))
            {
                await DetectCurrentTrackAsync();
                return;
            }

            ApplyCleanedTrackInfo(Track.Artist, Track.Title);
            var trackInfo = new TrackInfo { Artist = Track.Artist, Title = Track.Title };
            await FindLyricsAsync(trackInfo);
            SetShowPlayingThumbnail(IsPlayingTrack(trackInfo) || !(Track.Lyrics?.Text?.Length > 0));
        }

        private void RememberPlayingTrack()
        {
            _playingArtist = Track.Artist;
            _playingTitle = Track.Title;
        }

        private bool IsPlayingTrack(TrackInfo trackInfo)
        {
            static bool Overlaps(string a, string b) =>
                a.Contains(b, StringComparison.OrdinalIgnoreCase) || b.Contains(a, StringComparison.OrdinalIgnoreCase);

            var title = trackInfo.Title?.Trim();
            var playingTitle = _playingTitle?.Trim();
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(playingTitle) || !Overlaps(title, playingTitle))
                return false;

            var artist = trackInfo.Artist?.Trim();
            var playingArtist = _playingArtist?.Trim();
            return string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(playingArtist) || Overlaps(artist, playingArtist);
        }

        private void SetShowPlayingThumbnail(bool show)
        {
            void Apply()
            {
                _showPlayingThumbnail = show;
                Thumbnail = show ? _playingThumbnail : null;
            }

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(Apply);
                return;
            }

            Apply();
        }

        private void OnThumbnailChanged(object thumbnail)
        {
            if (thumbnail == null || DateTime.UtcNow > _thumbnailRefreshUntilUtc)
                return;

            _ = LoadThumbnailAsync(thumbnail, _thumbnailGeneration, keepCurrent: true);
        }

        private void LoadThumbnail(object thumbnail)
        {
            _thumbnailRefreshUntilUtc = DateTime.UtcNow.AddSeconds(10);
            _thumbnailHash = 0;
            var generation = ++_thumbnailGeneration;
            SetThumbnail(null);
            if (thumbnail != null)
            {
                _ = LoadThumbnailAsync(thumbnail, generation, keepCurrent: false);
            }
        }

        private async Task LoadThumbnailAsync(object thumbnail, int generation, bool keepCurrent)
        {
            byte[] bytes = null;
            try
            {
                bytes = await ThumbnailImageLoader.LoadBytesAsync(thumbnail);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Could not load track thumbnail");
            }

            if (generation != _thumbnailGeneration)
                return;

            var hash = HashBytes(bytes);
            if (keepCurrent && hash == _thumbnailHash)
                return;

            var appIcon = bytes != null && LooksLikeAppIcon(bytes);
            _thumbnailHash = hash;
            if (keepCurrent && !appIcon)
                _thumbnailRefreshUntilUtc = DateTime.MinValue;

            SetThumbnail(appIcon ? null : bytes);
        }

        private static bool LooksLikeAppIcon(byte[] bytes)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
                return dispatcher.Invoke(() => ThumbnailImageLoader.LooksLikeAppIcon(bytes));

            return ThumbnailImageLoader.LooksLikeAppIcon(bytes);
        }

        private static int HashBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return 0;

            var hash = bytes.Length;
            var step = Math.Max(1, bytes.Length / 64);
            for (var i = 0; i < bytes.Length; i += step)
                hash = unchecked(hash * 31 + bytes[i]);

            return hash;
        }

        private void SetThumbnail(byte[] bytes)
        {
            void Apply()
            {
                _playingThumbnail = CreateThumbnail(bytes);
                if (_showPlayingThumbnail)
                    Thumbnail = _playingThumbnail;
            }

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(Apply);
                return;
            }

            Apply();
        }

        private static ImageSource CreateThumbnail(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return null;

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = new MemoryStream(bytes);
            image.EndInit();
            image.Freeze();
            return image;
        }

        private void SetLyricsIdentity(Track foundTrack)
        {
            if (foundTrack?.Lyrics?.Text?.Length > 0)
            {
                LyricsTitle = foundTrack.Title;
                LyricsArtist = foundTrack.Artist;
                return;
            }

            LyricsTitle = string.IsNullOrWhiteSpace(_playingTitle) ? null : _playingTitle;
            LyricsArtist = string.IsNullOrWhiteSpace(_playingArtist) ? null : _playingArtist;
        }

        private void ApplyCleanedTrackInfo(string artist, string title)
        {
            Track.Artist = TrackTextCleaner.Clean(artist);
            Track.Title = TrackTextCleaner.CleanTitle(title, Track.Artist);
        }

        private async Task OpenLyricsAsync()
        {
            try
            {
                var source = Track.Lyrics?.Source;
                if (source == null)
                    return;

                if (source.IsFile)
                {
                    FileExplorer.SelectFile(source.LocalPath);
                    return;
                }

                await Launcher.LaunchUriAsync(source);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Cannot open lyrics {Track.Lyrics?.Source}");
            }
        }

        private async Task OpenWebsiteAsync()
        {
            try
            {
                await Launcher.LaunchUriAsync(new Uri(App.WebsiteUrlFor("tray")));
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
                ApplyCleanedTrackInfo(recognized.Artist, recognized.Title);
                await FindLyricsAsync();
            }
        }

        public void Dispose()
        {
            _progressTimer?.Stop();

            if (_musicWatcher != null)
            {
                _musicWatcher.TrackChanged -= OnWatcherTrackChanged;
                _musicWatcher.ThumbnailChanged -= OnThumbnailChanged;
                _musicWatcher.TrackProgressChanged -= OnTrackProgressChanged;
                _musicWatcher.PlayerStateChanged -= OnPlayerStateChanged;
                _musicWatcher.Dispose();
            }
        }
    }
}
