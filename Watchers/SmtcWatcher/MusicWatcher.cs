using LyricsFinder.Core;
using System;

namespace SmtcWatcher
{
    /// <summary>
    /// Observes current playing song.
    /// </summary>
    public abstract class MusicWatcher : IDisposable
    {
        private string _playerId;
        private PlayerState _playerState;
        private Track _track;
        private TimeSpan _trackProgress;

        /// <summary>
        /// Current player ID.
        /// </summary>
        public string PlayerId
        {
            get => _playerId;
            protected set
            {
                if (value != _playerId)
                {
                    _playerId = value;
                    PlayerChanged?.Invoke(this, _playerId);
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
        public event EventHandler<string> PlayerChanged;

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

        public abstract void Dispose();
    }
}
