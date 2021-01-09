using LyricsFinder.Core;
using NLog;
using NPSMLib;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Win10Watcher
{
    /// <summary>
    /// Fetches media info from system media transport controls via <see cref="NowPlayingSessionManager"/>.
    /// </summary>
    public class NpsmWatcher : MusicWatcher, IDisposable
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private MediaPlaybackDataSource _playbackData;

        public TimeSpan Interval { get; }

        public NpsmWatcher(TimeSpan interval)
        {
            Interval = interval;

            Task.Run(Process, _cancellationTokenSource.Token);
        }

        private void Process()
        {
            _logger.Debug($"Initialize music watcher with timer {Interval}");

            while (true)
            {
                var sessionManager = new NowPlayingSessionManager();
                sessionManager.SessionListChanged += OnSessionListChanged;

                var currentSession = sessionManager.CurrentSession;

                if (currentSession != null)
                {
                    PlayerInfo = new PlayerInfo(currentSession.Hwnd, currentSession.PID, currentSession.SourceAppId);

                    try
                    {
                        _playbackData = currentSession.ActivateMediaPlaybackDataSource();
                        _playbackData.MediaPlaybackDataChanged += OnMediaPlaybackDataChanged;

                        GetMediaProperties();
                        GetPlaybackInfo();
                        GetTimeline();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Cannot get playback data");
                    }
                }

                Thread.Sleep(Interval);

                if (sessionManager != null)
                {
                    sessionManager.SessionListChanged -= OnSessionListChanged;
                }
                if (_playbackData != null)
                {
                    _playbackData.MediaPlaybackDataChanged -= OnMediaPlaybackDataChanged;
                }
            }
        }

        private void GetMediaProperties()
        {
            var mediaProperties = _playbackData.GetMediaObjectInfo();

            Track = new Track
            {
                Artist = mediaProperties.Artist,
                Title = mediaProperties.Title,
                Album = mediaProperties.AlbumTitle,
                Genres = mediaProperties.Genres,
                Thumbnail = _playbackData.GetThumbnailStream()
            };
        }

        private void GetPlaybackInfo()
        {
            var playback = _playbackData.GetMediaPlaybackInfo();

            PlayerState = GetPlayerState(playback.PlaybackState);
        }

        private void GetTimeline()
        {
            var timeline = _playbackData.GetMediaTimelineProperties();
            TrackProgress = timeline.Position;
        }

        private static PlayerState GetPlayerState(MediaPlaybackState playbackState) =>
            playbackState switch
            {
                MediaPlaybackState.Stopped => PlayerState.Stopped,
                MediaPlaybackState.Playing => PlayerState.Playing,
                MediaPlaybackState.Paused => PlayerState.Paused,
                _ => PlayerState.Unknown
            };

        private void OnSessionListChanged(object sender, NowPlayingSessionManagerEventArgs e)
        {
            _logger.Info($"Session list changed: {e.NotificationType}, {e.SessionTypeString}");
        }

        private void OnMediaPlaybackDataChanged(object sender, MediaPlaybackDataChangedArgs e)
        {
            _logger.Info($"Playback data changed: {e.DataChangedEvent}");
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
