using LyricsFinder.Core;
using LyricsProviders;
using Lyrixator.Configuration;
using NLog;
using nucs.JsonSettings;
using nucs.JsonSettings.Autosave;
using PlayerWatching;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lyrixator.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly MultiPlayerWatcher _watcher;
        private readonly ITrackInfoProvider _trackInfoProvider;
        private readonly HotKey _hotKey;

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

        public ICommand FindLyricsCommand { get; }

        public LyricsSettings LyricsSettings { get; } = JsonSettings.Load<LyricsSettings>().EnableAutosave();

        public MainWindowViewModel(IEnumerable<IPlayerWatcher> playerWatchers, IEnumerable<ITrackInfoProvider> providers)
        {
            var interval = TimeSpan.FromSeconds(1);
            _watcher = new MultiPlayerWatcher(playerWatchers, interval);
            _watcher.TrackChanged += OnWatcherTrackChanged;

            _trackInfoProvider = new MultiTrackInfoProvider(providers);

            _hotKey = new HotKey(Application.Current.MainWindow, KeyModifiers.Ctrl | KeyModifiers.Alt, Key.L);
            _hotKey.OnHotKeyPressed += OnHotKeyPressed;

            FindLyricsCommand = new DelegateCommand(async () => await FindLyricsAsync(), CanFindLyrics)
                .ObservesProperty(() => Artist).ObservesProperty(() => Title);
        }

        private bool CanFindLyrics() => !string.IsNullOrEmpty(Artist) && !string.IsNullOrEmpty(Title);

        private async void OnHotKeyPressed(object sender, EventArgs e)
        {
            using var watcher = new SmtcWatcher();

            if (watcher.UpdateMediaInfo())
            {
                await UpdateTrackInfoAsync(watcher.Track);
            }
        }

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
            var foundTrack = await _trackInfoProvider.FindTrackAsync(trackInfo);
            Lyrics = foundTrack.Lyrics.Text;

            _logger.Debug($"Find lyrics length {Lyrics?.Length}");
        }

        private async void OnWatcherTrackChanged(object sender, Track track)
        {
            _logger.Debug($"Track changed {_watcher.ActualWatcher?.Name} - {_watcher.PlayerState}");
            await UpdateTrackInfoAsync(track);
        }

        private async Task FindLyricsAsync()
        {
            var trackInfo = new TrackInfo { Artist = Artist, Title = Title };
            await FindLyricsAsync(trackInfo);
        }

        public void Dispose()
        {
            if (_watcher != null)
            {
                _watcher.TrackChanged -= OnWatcherTrackChanged;
                _watcher.Dispose();
            }

            if (_hotKey != null)
            {
                _hotKey.OnHotKeyPressed -= OnHotKeyPressed;
                _hotKey.Dispose();
            }
        }
    }
}
