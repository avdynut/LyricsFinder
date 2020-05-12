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

        public event EventHandler<Track> TrackChanged;
        public event EventHandler Played;
        public event EventHandler Stopped;
        public event EventHandler Paused;
        public event EventHandler<TimeSpan> ProgressChanged;

        public abstract void StartMonitoring();
        public abstract void StopMonitoring();
    }
}
