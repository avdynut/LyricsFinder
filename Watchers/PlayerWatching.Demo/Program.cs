using LyricsFinder.Core;
using System;
using System.Collections.Generic;

namespace PlayerWatching.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var interval = TimeSpan.FromSeconds(1);
            var playerWatchers = new List<IPlayerWatcher> { new YandexMusicWatcher() };
            var watcher = new MultiPlayerWatcher(playerWatchers, interval);
            watcher.TrackChanged += OnWatcherTrackChanged;

            Console.ReadLine();
        }

        private static void OnWatcherTrackChanged(object sender, Track track)
        {
            Console.WriteLine($"New track: {track}");
        }
    }
}
