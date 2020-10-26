﻿using LyricsFinder.Core;
using LyricsProviders;
using LyricsProviders.DirectoriesProvider;
using NLog;
using PlayerWatching;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
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

        public LyricsSettingsViewModel LyricsSettings { get; }

        public MainWindowViewModel(
            MultiPlayerWatcher playersWatcher,
            MultiTrackInfoProvider trackInfoProvider,
            DirectoriesProviderSettings directoriesSettings,
            LyricsSettingsViewModel lyricsSettings)
        {
            _playersWatcher = playersWatcher;
            _trackInfoProvider = trackInfoProvider;
            _directoriesSettings = directoriesSettings;
            LyricsSettings = lyricsSettings;

            _playersWatcher.TrackChanged += OnWatcherTrackChanged;
            _playersWatcher.Initialize();

            FindLyricsCommand = new DelegateCommand(async () => await FindLyricsAsync(), CanFindLyrics)
                .ObservesProperty(() => Track.Artist).ObservesProperty(() => Track.Title).ObservesProperty(() => SearchInProgress);

            OpenLyricsCommand = new DelegateCommand(OpenLyrics, () => Track.Lyrics?.Source != null)
                .ObservesProperty(() => Track.Lyrics);
        }

        private bool CanFindLyrics() => !string.IsNullOrEmpty(Track.Artist) && !string.IsNullOrEmpty(Track.Title) && !SearchInProgress;

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
            var trackInfo = new TrackInfo { Artist = Track.Artist, Title = Track.Title };
            await FindLyricsAsync(trackInfo);
        }

        private void OpenLyrics()
        {
            var uri = Track.Lyrics.Source;
            var path = uri.IsFile ? uri.LocalPath : uri.AbsoluteUri;
            var startInfo = new ProcessStartInfo(path) { UseShellExecute = true };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Cannot open lyrics {path}");
            }
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
