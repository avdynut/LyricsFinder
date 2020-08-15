using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using LyricsFinder.Core;
using NLog;
using PlayerWatching.Localization;
using System;

namespace PlayerWatching
{
    public class YandexMusicWatcher : IPlayerWatcher
    {
        private const string PlayButtonAutomationId = "PlayButton";
        private const string TitleButtonAutomationId = "TitleButton";
        private const string TitleTextBlockAutomationId = "Title";
        private const string TitleButtonPhoneModeAutomationId = "PhoneTrackButton";

        private const string ArtistButtonAutomationId = "ArtistButton";
        private const string ArtistTextBlockAutomationId = "Artist";

        private const int PhoneModeMaxWidth = 500;
        private const int TopmostModeMaxHeight = 384;

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly YandexMusicLocalization _localization = YandexMusicLocalization.Localizations.GetCurrentLocalization();
        private readonly UIA3Automation _automation;
        private readonly AutomationElement _desktop;

        public string Name => "Yandex.Music";
        public Track Track { get; private set; }
        public PlayerState PlayerState { get; private set; }

        public YandexMusicWatcher()
        {
            _automation = new UIA3Automation();
            _desktop = _automation.GetDesktop();
        }

        public bool UpdateMediaInfo()
        {
            var result = false;
            var track = new Track();
            var yandexMusicParentWindow = _desktop.FindFirstChild(x => x.ByName(_localization.WindowTitle));

            if (yandexMusicParentWindow != null)
            {
                AutomationElement window;
                try
                {
                    window = yandexMusicParentWindow.FindChildAt(1);

                    var size = window.BoundingRectangle;

                    if (size.Height <= TopmostModeMaxHeight)    // in topmost mode
                    {
                        _logger.Trace("Getting song info from Y.Music in topmost mode");

                        track.Title = window.FindFirstChild(TitleTextBlockAutomationId)?.Name;
                        track.Artist = window.FindFirstChild(ArtistTextBlockAutomationId)?.Name;
                    }
                    else if (size.Width < PhoneModeMaxWidth && size.Height >= PhoneModeMaxWidth) // in phone mode
                    {
                        _logger.Trace("Getting song info from Y.Music in phone mode");
                        var children = window.FindAllChildren(x => x.ByControlType(ControlType.Button));

                        for (int i = 0; i < children.Length; i++)
                        {
                            if (children[i].AutomationId == TitleButtonPhoneModeAutomationId)
                            {
                                track.Title = children[i].Name;
                                track.Artist = children[i + 1].Name;
                                break;
                            }
                        }
                    }
                    else // normal mode
                    {
                        _logger.Trace("Getting song info from Y.Music");
                        track.Title = window.FindFirstChild(TitleButtonAutomationId)?.Name;
                        track.Artist = window.FindFirstChild(ArtistButtonAutomationId)?.Name;
                    }

                    Track = track;
                    result = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Cannot find song info");
                }
            }

            return result;
        }

        public void Dispose()
        {
            _automation.Dispose();
        }
    }
}
