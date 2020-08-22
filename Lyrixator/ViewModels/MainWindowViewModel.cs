using LyricsFinder.Core;
using LyricsProviders;
using LyricsProviders.DirectoriesProvider;
using Lyrixator.Configuration;
using NLog;
using nucs.JsonSettings;
using PlayerWatching;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
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
        private readonly MultiPlayerWatcher _watcher;
        private readonly ITrackInfoProvider _trackInfoProvider;

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

        public LyricsSettings LyricsSettings { get; } = JsonSettings.Load<LyricsSettings>();

        public MainWindowViewModel(DirectoriesProviderSettings directoriesSettings, IEnumerable<IPlayerWatcher> playerWatchers, IEnumerable<ITrackInfoProvider> providers)
        {
            _directoriesSettings = directoriesSettings;
            var settings = JsonSettings.Load<Settings>();

            _watcher = new MultiPlayerWatcher(playerWatchers, settings.CheckInterval);
            _watcher.TrackChanged += OnWatcherTrackChanged;

            _trackInfoProvider = new MultiTrackInfoProvider(providers);

            FindLyricsCommand = new DelegateCommand(async () => await FindLyricsAsync(), CanFindLyrics)
                .ObservesProperty(() => Artist).ObservesProperty(() => Title);
        }

        private bool CanFindLyrics() => !string.IsNullOrEmpty(Artist) && !string.IsNullOrEmpty(Title);

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
            }
            else
            {
                _logger.Debug("Lyrics not found");
            }
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
        }
    }
}
