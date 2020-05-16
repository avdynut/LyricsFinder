using LyricsFinder.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PlayerWatching
{
    public class MultiPlayerWatcher : IPlayer
    {
        private readonly Timer _timer;

        public TimeSpan Interval { get; }
        public IEnumerable<IPlayerWatcher> PlayerWatchers { get; }

        private Track track;
        public Track Track
        {
            get => track;
            protected set
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
            protected set
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

            _timer = new Timer(_ => UpdateMediaInfo(), null, TimeSpan.Zero, Interval);
        }

        private void UpdateMediaInfo()
        {
            foreach (var watcher in PlayerWatchers)
            {
                var result = watcher.UpdateMediaInfo();

                if (result)
                {
                    Track = watcher.Track;
                    PlayerState = watcher.PlayerState;
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();

            foreach (var watcher in PlayerWatchers)
            {
                watcher.Dispose();
            }
        }
    }
}
