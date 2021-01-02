using LyricsFinder.Core;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace SmtcWatcher
{
    /// <summary>
    /// Fetches media info from system media transport controls in cycle.
    /// The watcher will be removed when Microsoft fixes a bug with SMTC events.
    /// </summary>
    public class CyclicalSmtcWatcher : MusicWatcher
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private GlobalSystemMediaTransportControlsSession _currentSession;

        public TimeSpan Interval { get; }

        public CyclicalSmtcWatcher(TimeSpan interval)
        {
            Interval = interval;

            Task.Run(Process, _cancellationTokenSource.Token);
        }

        private async Task Process()
        {
            _logger.Debug($"Initialize music watcher with timer {Interval}");

            while (true)
            {
                var _sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                _currentSession = _sessionManager.GetCurrentSession();

                PlayerId = _currentSession?.SourceAppUserModelId;

                if (_currentSession != null)
                {
                    await GetMediaPropertiesAsync();
                    GetPlaybackInfo();
                    GetTimeline();

                    //_currentSession.MediaPropertiesChanged += OnCurrentSessionMediaPropertiesChanged;
                    //_currentSession.PlaybackInfoChanged += OnCurrentSessionPlaybackInfoChanged;
                    //_currentSession.TimelinePropertiesChanged += OnCurrentSessionTimelinePropertiesChanged;
                }
                await Task.Delay(Interval);
            }
        }

        private async Task GetMediaPropertiesAsync()
        {
            if (await _currentSession?.TryGetMediaPropertiesAsync() is GlobalSystemMediaTransportControlsSessionMediaProperties mp)
            {
                Track = new Track
                {
                    Artist = mp.Artist,
                    Title = mp.Title,
                    Album = mp.AlbumTitle,
                    Genres = mp.Genres,
                    Thumbnail = mp.Thumbnail
                };
                _logger.Debug($"Artist: {mp.Artist}, Title: {mp.Title}, Album: {mp.AlbumTitle}, Genres: {string.Join(";", mp.Genres)}, Type: {mp.PlaybackType}, Thumbnail: {mp.Thumbnail}");
            }
        }

        private void GetPlaybackInfo()
        {
            if (_currentSession?.GetPlaybackInfo() is GlobalSystemMediaTransportControlsSessionPlaybackInfo playback)
            {
                PlayerState = SystemMediaWatcher.GetPlayerState(playback.PlaybackStatus);
                _logger.Debug($"PlaybackType: {playback.PlaybackType}, PlaybackStatus: {playback.PlaybackStatus}, Rate: {playback.PlaybackRate}");
            }
        }

        private void GetTimeline()
        {
            if (_currentSession?.GetTimelineProperties() is GlobalSystemMediaTransportControlsSessionTimelineProperties timeline)
            {
                TrackProgress = timeline.Position;
                _logger.Debug($"Position: {timeline.Position}, StartTime: {timeline.StartTime}, EndTime: {timeline.EndTime}, LastUpdatedTime: {timeline.LastUpdatedTime}");
            }
        }

        public override void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
