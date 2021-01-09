using LyricsFinder.Core;
using System;

namespace Win10Watcher
{
    /// <summary>
    /// Observes current playing song.
    /// </summary>
    public abstract class MusicWatcher
    {
        private PlayerInfo _playerInfo;
        private PlayerState _playerState;
        private Track _track;
        private TimeSpan _trackProgress;

        /// <summary>
        /// Current player ID.
        /// </summary>
        public PlayerInfo PlayerInfo
        {
            get => _playerInfo;
            protected set
            {
                if (value != _playerInfo)
                {
                    _playerInfo = value;
                    PlayerChanged?.Invoke(this, _playerInfo);
                }
            }
        }

        /// <summary>
        /// Current player state.
        /// </summary>
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

        /// <summary>
        /// Current track.
        /// </summary>
        public Track Track
        {
            get => _track;
            protected set
            {
                if (value != _track)
                {
                    _track = value;
                    TrackChanged?.Invoke(this, _track);
                }
            }
        }

        /// <summary>
        /// Time progress of the current track.
        /// </summary>
        public TimeSpan TrackProgress
        {
            get => _trackProgress;
            protected set
            {
                if (value != _trackProgress)
                {
                    _trackProgress = value;
                    TrackProgressChanged?.Invoke(this, _trackProgress);
                }
            }
        }

        /// <summary>
        /// Occurs when current player changed.
        /// </summary>
        public event EventHandler<PlayerInfo> PlayerChanged;

        /// <summary>
        /// Occurs when player state changed.
        /// </summary>
        public event EventHandler<PlayerState> PlayerStateChanged;

        /// <summary>
        /// Occurs when current track changed.
        /// </summary>
        public event EventHandler<Track> TrackChanged;

        /// <summary>
        /// Occurs when time progress changed.
        /// </summary>
        public event EventHandler<TimeSpan> TrackProgressChanged;
    }
}
