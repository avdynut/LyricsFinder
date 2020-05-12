using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using LyricsFinder.Core;
using System;
using System.Threading;

namespace PlayerWatching
{
    public class YandexMusicWatcher : PlayerWatcher
    {
        private const string YandexMusicProcessName = "Y.Music";
        private const string YandexMusicWindowTitleRu = "Яндекс.Музыка";

        private readonly TimeSpan _delayPeriod = TimeSpan.FromMilliseconds(5000);

        private UIA3Automation _automation;
        private AutomationElement _desktop;
        private Timer _timer;

        public override void StartMonitoring()
        {
            //var processes = Process.GetProcessesByName(YandexMusicProcessName);

            _automation = new UIA3Automation();
            _desktop = _automation.GetDesktop();

            _timer = new Timer(UpdateTrack, null, TimeSpan.Zero, _delayPeriod);
        }

        private void UpdateTrack(object state)
        {
            var track = new Track();
            var yandexMusicParentWindow = _desktop.FindFirstChild(x => x.ByName(YandexMusicWindowTitleRu));

            if (yandexMusicParentWindow != null)
            {
                var window = yandexMusicParentWindow.FindChildAt(1);
                //var window = yandexMusicParentWindow.FindFirstChild(x => x.ByProcessId(yMusicProcess.Id));

                if (window != null)
                {
                    track.Artist = window.FindFirstChild("ArtistButton").Name;
                    track.Title = window.FindFirstChild("TitleButton").Name;

                    Console.WriteLine(track);
                    Track = track;
                }
                else
                {
                    Console.WriteLine("Cannot find Yandex.Music main area");
                }
            }
            else
            {
                Console.WriteLine("Cannot find Yandex.Music window");
            }
        }

        public override void StopMonitoring()
        {
            _automation?.Dispose();
            _timer?.Dispose();
        }
    }
}
