using LyricsFinder.Core;
using LyricsProviders;
using PlayerWatching;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lyrixator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MultiPlayerWatcher _watcher;
        private readonly ITrackInfoProvider _trackInfoProvider;
        private readonly HotKey _hotKey;

        public MainWindow()
        {
            InitializeComponent();

            var interval = TimeSpan.FromSeconds(1);
            var playerWatchers = new List<IPlayerWatcher> { new YandexMusicWatcher() };
            _watcher = new MultiPlayerWatcher(playerWatchers, interval);
            _watcher.TrackChanged += OnWatcherTrackChanged;

            var providers = new List<ITrackInfoProvider> { new GoogleTrackInfoProvider() };
            _trackInfoProvider = new MultiTrackInfoProvider(providers);

            _hotKey = new HotKey(this, KeyModifiers.Ctrl | KeyModifiers.Alt, Key.L);
            _hotKey.OnHotKeyPressed += OnHotKeyPressed;
        }

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
            Dispatcher.Invoke(() =>
            {
                Title.Text = track.Title;
                Artist.Text = track.Artist;
                Lyrics.Text = track.Lyrics?.Text;
            });

            var trackInfo = track.ToTrackInfo();
            var foundTrack = await _trackInfoProvider.FindTrackAsync(trackInfo);

            Dispatcher.Invoke(() => Lyrics.Text = foundTrack.Lyrics.Text);
        }

        private async void OnWatcherTrackChanged(object sender, Track track)
        {
            await UpdateTrackInfoAsync(track);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _watcher.TrackChanged -= OnWatcherTrackChanged;
            _watcher.Dispose();

            _hotKey.OnHotKeyPressed -= OnHotKeyPressed;
            _hotKey.Dispose();

            base.OnClosing(e);
        }
    }
}
