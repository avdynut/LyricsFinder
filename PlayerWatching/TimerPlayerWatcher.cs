using System;
using System.Threading;

namespace PlayerWatching
{
    public abstract class TimerPlayerWatcher : PlayerWatcher
    {
        private Timer _timer;

        public TimeSpan Interval { get; }

        public TimerPlayerWatcher(TimeSpan interval)
        {
            Interval = interval;
        }

        public override void StartMonitoring()
        {
            Initialize();
            _timer = new Timer(_ => UpdateTrack(), null, TimeSpan.Zero, Interval);
        }

        public override void StopMonitoring()
        {
            Dispose();
            _timer?.Dispose();
        }

        protected abstract void Initialize();
        protected abstract void Dispose();
        protected abstract void UpdateTrack();
    }
}
