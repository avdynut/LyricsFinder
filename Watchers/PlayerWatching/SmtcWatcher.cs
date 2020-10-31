using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using LyricsFinder.Core;
using NLog;
using PlayerWatching.Localization;
using System;

namespace PlayerWatching
{
    /// <summary>
    /// Watcher for the System Media Transport Controls window.
    /// </summary>
    public class SmtcWatcher : IPlayerWatcher
    {
        private const string SmtcWindowClassName = "NativeHWNDHost";
        private const string PlayButtonAutomationId = "idPlayPause";
        private const string TitleTextAutomationId = "idStreamName";
        private const string ArtistTextAutomationId = "idArtistName";

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly SmtcLocalization _localization = SmtcLocalization.Localizations.GetCurrentLocalization();

        private UIA3Automation _automation;
        private AutomationElement _desktop;

        public const string Name = "SystemMediaTransportControls";
        public string DisplayName => Name;
        public Track Track { get; private set; }
        public PlayerState PlayerState { get; private set; }

        public void Initialize()
        {
            _automation = new UIA3Automation();
            _desktop = _automation.GetDesktop();
        }

        public bool UpdateMediaInfo()
        {
            var track = new Track();
            var playerState = PlayerState.Unknown;
            var result = false;

            var window = _desktop.FindFirstChild(
                x => x.ByClassName(SmtcWindowClassName)
                      .And(new PropertyCondition(_automation.PropertyLibrary.Element.ControlType, ControlType.Pane)));

            if (window != null)
            {
                try
                {
                    var titleText = window.FindFirstChild(TitleTextAutomationId)?.Name;
                    var artistText = window.FindFirstChild(ArtistTextAutomationId)?.Name;
                    track.Title = titleText?.Replace(_localization.TitlePrecedingText, string.Empty);
                    track.Artist = artistText?.Replace(_localization.ArtistPrecedingText, string.Empty);

                    var playButtonText = window.FindFirstChild(PlayButtonAutomationId).Name;
                    playerState = playButtonText.Contains(_localization.PlayButtonPlayingText) ? PlayerState.Playing : PlayerState.Paused;
                    result = track != Track && !track.IsTrackEmpty;
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Cannot read track info from SMTC");
                }
            }

            Track = track;
            PlayerState = playerState;
            return result;
        }

        public void Dispose()
        {
            _automation.Dispose();
        }
    }
}
