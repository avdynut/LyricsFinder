using LyricsFinder.Core;
using LyricsProviders;
using PlayerWatching;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Lyrixator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ITrackInfoProvider _trackInfoProvider;

        public MainWindow()
        {
            InitializeComponent();

            var interval = TimeSpan.FromSeconds(1);
            var playerWatchers = new List<IPlayerWatcher> { new YandexMusicWatcher() };
            var watcher = new MultiPlayerWatcher(playerWatchers, interval);
            watcher.TrackChanged += OnWatcherTrackChanged;

            var providers = new List<ITrackInfoProvider> { new GoogleTrackInfoProvider() };
            _trackInfoProvider = new MultiTrackInfoProvider(providers);
        }

        private async void OnWatcherTrackChanged(object sender, Track track)
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
    }
}
