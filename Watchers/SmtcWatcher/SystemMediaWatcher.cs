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

        public SystemMediaWatcher()
        {
            //    var sessions = smtcSessionManager.GetSessions();
            //    foreach (var session in sessions)
            //    {
            //        var info = session.GetPlaybackInfo();
            //        var time = session.GetTimelineProperties();
            //        var d = await session.TryGetMediaPropertiesAsync();
            //    }
        }

        public async Task InitializeAsync()
        {
            _logger.Debug("Initialize session manager");

            _sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            GetSessions();
            GetCurrentSession();

            _sessionManager.SessionsChanged += OnSessionsChanged;
            _sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
        }

        private void GetSessions()
        {
            var sessions = _sessionManager.GetSessions();
            _logger.Debug($"Sessions: {string.Join(", ", sessions.Select(x => x.SourceAppUserModelId))}");
        }

        private void GetCurrentSession()
        {
            _currentSession = _sessionManager.GetCurrentSession();
            _logger.Debug($"Current session: {_currentSession?.SourceAppUserModelId}");

            if (_currentSession != null)
            {
                //_currentSession.
            }
        }

        private void OnSessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            GetSessions();
        }

        private void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        {
            GetCurrentSession();
        }

        public void Dispose()
        {
            _sessionManager.SessionsChanged -= OnSessionsChanged;
            _sessionManager.CurrentSessionChanged -= OnCurrentSessionChanged;

            _logger.Debug("Disposed session manager");
        }
    }
}
