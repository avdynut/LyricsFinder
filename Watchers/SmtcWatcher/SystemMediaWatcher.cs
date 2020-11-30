using LyricsFinder.Core;
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
    public class SystemMediaWatcher : MusicWatcher
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
            PlayerId = _currentSession?.SourceAppUserModelId;
            _logger.Debug($"Current player: {PlayerId}");

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
                PlayerState = GetPlayerState(playback.PlaybackStatus);
                _logger.Debug($"PlaybackType: {playback.PlaybackType}, PlaybackStatus: {playback.PlaybackStatus}, Rate: {playback.PlaybackRate}");
            }
        }

        internal static PlayerState GetPlayerState(GlobalSystemMediaTransportControlsSessionPlaybackStatus playbackStatus) =>
            playbackStatus switch
            {
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped => PlayerState.Stopped,
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing => PlayerState.Playing,
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused => PlayerState.Paused,
                _ => PlayerState.Unknown
            };

        private void GetTimeline()
        {
            if (_currentSession?.GetTimelineProperties() is GlobalSystemMediaTransportControlsSessionTimelineProperties timeline)
            {
                TrackProgress = timeline.Position;
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

        public override void Dispose()
        {
            _sessionManager.SessionsChanged -= OnSessionsChanged;
            _sessionManager.CurrentSessionChanged -= OnCurrentSessionChanged;

            _logger.Debug("Disposed session manager");
        }
    }
}
