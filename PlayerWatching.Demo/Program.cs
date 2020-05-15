using LyricsFinder.Core;
using System;

namespace PlayerWatching.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var interval = TimeSpan.FromSeconds(1);
            var watcher = new YandexMusicWatcher(interval);
            watcher.TrackChanged += OnWatcherTrackChanged;
            watcher.StartMonitoring();

            Console.ReadLine();
        }

        private static void OnWatcherTrackChanged(object sender, Track track)
        {
            Console.WriteLine($"New track: {track}");
        }
    }
}
