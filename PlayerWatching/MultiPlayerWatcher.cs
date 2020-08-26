using LyricsFinder.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlayerWatching
{
    public class MultiPlayerWatcher : IPlayer
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public TimeSpan Interval { get; }
        public IEnumerable<IPlayerWatcher> PlayerWatchers { get; }
        public IPlayerWatcher ActualWatcher { get; private set; }

        private Track track;
        public Track Track
        {
            get => track;
            private set
            {
                if (value != track && value?.IsTrackEmpty == false)
                {
                    track = value;
                    TrackChanged?.Invoke(this, track);
                }
            }
        }

        private PlayerState _playerState;
        public PlayerState PlayerState
        {
            get => _playerState;
            private set
            {
                if (value != _playerState)
                {
                    _playerState = value;
                    PlayerStateChanged?.Invoke(this, _playerState);
                }
            }
        }

        public event EventHandler<Track> TrackChanged;
        public event EventHandler<PlayerState> PlayerStateChanged;
        //public event EventHandler<TimeSpan> ProgressChanged;

        public MultiPlayerWatcher(IEnumerable<IPlayerWatcher> playerWatchers, TimeSpan interval)
        {
            PlayerWatchers = playerWatchers ?? throw new ArgumentNullException(nameof(playerWatchers));
            Interval = interval;
        }

        public void Initialize()
        {
            // UI automation has to be initialized in background thread to avoid freezing UI
            Task.Run(Process, _cancellationTokenSource.Token);
        }

        private void Process()
        {
            foreach (var watcher in PlayerWatchers)
            {
                watcher.Initialize();
            }

            _logger.Debug($"Initialize multi watcher with timer {Interval}");

            while (true)
            {
                UpdateMediaInfo();
                Thread.Sleep(Interval);
            }
        }

        private void UpdateMediaInfo()
        {
            foreach (var watcher in PlayerWatchers)
            {
                var result = watcher.UpdateMediaInfo();

                if (result)
                {
                    ActualWatcher = watcher;
                    Track = watcher.Track;
                    PlayerState = watcher.PlayerState;

                    if (PlayerState == PlayerState.Playing)
                        return;
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            foreach (var watcher in PlayerWatchers)
            {
                watcher.Dispose();
            }
        }
    }
}
