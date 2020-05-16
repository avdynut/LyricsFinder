using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace PlayerWatching.Tests
{
    [TestClass]
    public class YandexMusicWatcherTests
    {
        [TestMethod]
        public void YandexMusicWatcherTrackChangedTest()
        {
            var interval = TimeSpan.FromMilliseconds(200);
            var watcher = new YandexMusicWatcher(interval);
            var resetEvent = new ManualResetEventSlim();

            watcher.TrackChanged += (s, track) => resetEvent.Set();

            watcher.StartMonitoring();

            var result = resetEvent.Wait(TimeSpan.FromSeconds(1));
            Assert.IsTrue(result, "TrackChanged event is not occured");
            Assert.IsNotNull(watcher.Track);
            Assert.IsFalse(watcher.Track.IsTrackEmpty);

            Console.WriteLine(watcher.Track);
            watcher.StopMonitoring();
        }
    }
}
