using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace SmtcWatcher
{
    /// <summary>
    /// Fetches media info from system media transport controls.
    /// </summary>
    public class SystemMediaWatcher : IDisposable
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private GlobalSystemMediaTransportControlsSessionManager _sessionManager;
        private GlobalSystemMediaTransportControlsSession _currentSession;

        public async Task InitializeAsync()
        {
            _logger.Debug("Initialize session manager");

            _sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            GetSessions();
            await GetCurrentSessionAsync();

            _sessionManager.SessionsChanged += OnSessionsChanged;
            _sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
        }

        private void GetSessions()
        {
            var sessions = _sessionManager.GetSessions();
            _logger.Debug($"Sessions: {string.Join(", ", sessions.Select(x => x.SourceAppUserModelId))}");
        }

        private async Task GetCurrentSessionAsync()
        {
            _currentSession = _sessionManager.GetCurrentSession();
            _logger.Debug($"Current session: {_currentSession?.SourceAppUserModelId}");

            if (_currentSession != null)
            {
                await GetMediaPropertiesAsync();
                GetPlaybackInfo();
                GetTimeline();

                _currentSession.MediaPropertiesChanged += OnCurrentSessionMediaPropertiesChanged;
                _currentSession.PlaybackInfoChanged += OnCurrentSessionPlaybackInfoChanged;
                _currentSession.TimelinePropertiesChanged += OnCurrentSessionTimelinePropertiesChanged;
            }
        }

        private async Task GetMediaPropertiesAsync()
        {
            if (await _currentSession?.TryGetMediaPropertiesAsync() is GlobalSystemMediaTransportControlsSessionMediaProperties mp)
            {
                _logger.Debug($"Artist: {mp.Artist}, Title: {mp.Title}, Album: {mp.AlbumTitle}, Genres: {string.Join(";", mp.Genres)}, Type: {mp.PlaybackType}, Thumbnail: {mp.Thumbnail}");
            }
        }

        private void GetPlaybackInfo()
        {
            if (_currentSession?.GetPlaybackInfo() is GlobalSystemMediaTransportControlsSessionPlaybackInfo playback)
            {
                _logger.Debug($"PlaybackType: {playback.PlaybackType}, PlaybackStatus: {playback.PlaybackStatus}, Rate: {playback.PlaybackRate}");
            }
        }

        private void GetTimeline()
        {
            if (_currentSession?.GetTimelineProperties() is GlobalSystemMediaTransportControlsSessionTimelineProperties timeline)
            {
                _logger.Debug($"Position: {timeline.Position}, StartTime: {timeline.StartTime}, EndTime: {timeline.EndTime}, LastUpdatedTime: {timeline.LastUpdatedTime}");
            }
        }

        private async void OnCurrentSessionMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            await GetMediaPropertiesAsync();
        }

        private void OnCurrentSessionPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            GetPlaybackInfo();
        }

        private void OnCurrentSessionTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
        {
            GetTimeline();
        }

        private void OnSessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            GetSessions();
        }

        private async void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        {
            await GetCurrentSessionAsync();
        }

        public void Dispose()
        {
            _sessionManager.SessionsChanged -= OnSessionsChanged;
            _sessionManager.CurrentSessionChanged -= OnCurrentSessionChanged;

            _logger.Debug("Disposed session manager");
        }
    }
}
