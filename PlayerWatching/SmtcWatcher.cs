using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using LyricsFinder.Core;
using NLog;
using PlayerWatching.Localization;
using System;
using System.Threading;

namespace PlayerWatching
{
    /// <summary>
    /// Watcher for System Media Transport Controls window.
    /// </summary>
    public class SmtcWatcher : IPlayerWatcher
    {
        private const string PlayButtonAutomationId = "idPlayPause";
        private const string TitleTextAutomationId = "idStreamName";
        private const string ArtistTextAutomationId = "idArtistName";

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly SmtcLocalization _localization = SmtcLocalization.Localizations.GetCurrentLocalization();
        private readonly UIA3Automation _automation;
        private readonly AutomationElement _desktop;

        public string Name => "SystemMediaTransportControls";
        public Track Track { get; private set; }
        public PlayerState PlayerState { get; private set; }

        public SmtcWatcher()
        {
            _automation = new UIA3Automation();
            _desktop = _automation.GetDesktop();
        }

        public bool UpdateMediaInfo()
        {
            var track = new Track();
            var playerState = PlayerState.Unknown;
            var result = false;

            // change system volume to see SMTC window
            Keyboard.Type(VirtualKeyShort.VOLUME_UP);
            Keyboard.Type(VirtualKeyShort.VOLUME_DOWN);

            // wait some to load automation elements
            Thread.Sleep(50);

            try
            {
                var titleText = _desktop.FindFirstDescendant(TitleTextAutomationId).Name;
                var artistText = _desktop.FindFirstDescendant(ArtistTextAutomationId).Name;
                track.Title = titleText.Replace(_localization.TitlePrecedingText, string.Empty);
                track.Artist = artistText.Replace(_localization.ArtistPrecedingText, string.Empty);

                var playButtonText = _desktop.FindFirstDescendant(PlayButtonAutomationId).Name;
                playerState = playButtonText.Contains(_localization.PlayButtonPlayingText) ? PlayerState.Playing : PlayerState.Paused;
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Cannot read track info from SMTC");
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
