using LyricsFinder.Core;
using System;

namespace PlayerWatching
{
    public abstract class PlayerWatcher
    {
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
        public event EventHandler<TimeSpan> ProgressChanged;

        public abstract void StartMonitoring();
        public abstract void StopMonitoring();
    }
}
