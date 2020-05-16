using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using LyricsFinder.Core;
using System;

namespace PlayerWatching
{
    // todo: add localized constants
    public class SmtcWatcher
    {
        private const string PlayButtonAutomationId = "idPlayPause";
        private const string PlayButtonPlayingText = "Приостановить";

        private const string TitleTextAutomationId = "idStreamName";
        private const string TitlePrecedingText = "Название дорожки ";

        private const string ArtistTextAutomationId = "idArtistName";
        private const string ArtistPrecedingText = "Сведения о дорожке ";

        public Track Track { get; private set; }
        public PlayerState PlayerState { get; private set; }

        public bool UpdateMediaInfo()
        {
            var track = new Track();
            var playerState = PlayerState.Unknown;
            var result = false;

            // change system volume to see SMTC window
            Keyboard.Type(VirtualKeyShort.VOLUME_UP);
            Keyboard.Type(VirtualKeyShort.VOLUME_DOWN);

            using var automation = new UIA3Automation();
            var desktop = automation.GetDesktop();

            try
            {
                var titleText = desktop.FindFirstDescendant(TitleTextAutomationId).Name;
                var artistText = desktop.FindFirstDescendant(ArtistTextAutomationId).Name;
                track.Title = titleText.Replace(TitlePrecedingText, string.Empty);
                track.Artist = artistText.Replace(ArtistPrecedingText, string.Empty);

                var playButtonText = desktop.FindFirstDescendant(PlayButtonAutomationId).Name;
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
    }
}
