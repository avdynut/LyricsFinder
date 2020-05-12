using LyricsFinder.Core;
using System;

namespace PlayerWatching.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var watcher = new YandexMusicWatcher();
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
