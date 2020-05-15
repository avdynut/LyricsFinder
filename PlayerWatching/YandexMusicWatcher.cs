﻿using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using LyricsFinder.Core;
using System;

namespace PlayerWatching
{
    public class YandexMusicWatcher : TimerPlayerWatcher
    {
        private const string YandexMusicWindowTitleRu = "Яндекс.Музыка";

        private const string TitleButtonAutomationId = "TitleButton";
        private const string TitleTextBlockAutomationId = "Title";
        private const string TitleButtonPhoneModeAutomationId = "PhoneTrackButton";

        private const string ArtistButtonAutomationId = "ArtistButton";
        private const string ArtistTextBlockAutomationId = "Artist";

        private const int PhoneModeMaxWidth = 500;
        private const int TopmostModeMaxHeight = 384;

        private UIA3Automation _automation;
        private AutomationElement _desktop;

        public YandexMusicWatcher(TimeSpan interval) : base(interval)
        {
        }

        protected override void Initialize()
        {
            _automation = new UIA3Automation();
            _desktop = _automation.GetDesktop();
        }

        protected override void UpdateTrack()
        {
            var track = new Track();
            var yandexMusicParentWindow = _desktop.FindFirstChild(x => x.ByName(YandexMusicWindowTitleRu));

            if (yandexMusicParentWindow != null)
            {
                AutomationElement window;
                try
                {
                    window = yandexMusicParentWindow.FindChildAt(1);

                    var size = window.BoundingRectangle;

                    if (size.Height <= TopmostModeMaxHeight)    // in topmost mode
                    {
                        track.Title = window.FindFirstChild(TitleTextBlockAutomationId)?.Name;
                        track.Artist = window.FindFirstChild(ArtistTextBlockAutomationId)?.Name;
                    }
                    else if (size.Width < PhoneModeMaxWidth && size.Height >= PhoneModeMaxWidth) // in phone mode
                    {
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
                        track.Title = window.FindFirstChild(TitleButtonAutomationId)?.Name;
                        track.Artist = window.FindFirstChild(ArtistButtonAutomationId)?.Name;
                    }

                    Console.WriteLine(track);
                    Track = track;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot find song info: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Cannot find Yandex.Music window");
            }
        }

        protected override void Dispose()
        {
            _automation?.Dispose();
        }
    }
}
