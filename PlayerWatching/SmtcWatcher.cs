using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using LyricsFinder.Core;
using System;
using System.Threading;

namespace PlayerWatching
{
    // todo: add localized constants
    public class SmtcWatcher : IPlayerWatcher
    {
        private const string PlayButtonAutomationId = "idPlayPause";
        private const string PlayButtonPlayingText = "Приостановить";

        private const string TitleTextAutomationId = "idStreamName";
        private const string TitlePrecedingText = "Название дорожки ";

        private const string ArtistTextAutomationId = "idArtistName";
        private const string ArtistPrecedingText = "Сведения о дорожке ";

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
                track.Title = titleText.Replace(TitlePrecedingText, string.Empty);
                track.Artist = artistText.Replace(ArtistPrecedingText, string.Empty);

                var playButtonText = _desktop.FindFirstDescendant(PlayButtonAutomationId).Name;
                playerState = playButtonText.Contains(PlayButtonPlayingText) ? PlayerState.Playing : PlayerState.Paused;
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot read track info: {ex.Message}");
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
