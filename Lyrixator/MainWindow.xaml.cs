﻿using LyricsFinder.Core;
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
        private MultiPlayerWatcher _watcher;
        private ITrackInfoProvider _trackInfoProvider;
        private HotKey _hotKey;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTools();
        }

        private void InitializeTools()
        {
            var interval = TimeSpan.FromSeconds(1);
            var playerWatchers = new List<IPlayerWatcher> { new YandexMusicWatcher() };
            _watcher = new MultiPlayerWatcher(playerWatchers, interval);
            //_watcher.TrackChanged += OnWatcherTrackChanged;

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

            await FindLyricsAsync(track.ToTrackInfo());
        }

        private async Task FindLyricsAsync(TrackInfo trackInfo)
        {
            var foundTrack = await _trackInfoProvider.FindTrackAsync(trackInfo);
            Dispatcher.Invoke(() => Lyrics.Text = foundTrack.Lyrics.Text);
        }

        private async void OnWatcherTrackChanged(object sender, Track track)
        {
            await UpdateTrackInfoAsync(track);
        }

        private async void OnFindButtonClick(object sender, RoutedEventArgs e)
        {
            var trackInfo = new TrackInfo { Artist = Artist.Text, Title = Title.Text };
            await FindLyricsAsync(trackInfo);
        }

        protected override void OnClosing(CancelEventArgs e)
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

            base.OnClosing(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            DragMove();
        }

        private void OnTaskbarIconTrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                MinimizeWindow();
            }
        }

        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            MinimizeWindow();
        }

        private void MinimizeWindow()
        {
            ShowInTaskbar = true;
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
        }

        private void OnMaximizeButtonClick(object sender, RoutedEventArgs e)
        {
            MaximizeWindow();
        }

        private void MaximizeWindow()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            TrayIcon.Dispose();
            base.OnClosed(e);
        }

        private void OnTitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                MaximizeWindow();
            }
        }
    }
}
