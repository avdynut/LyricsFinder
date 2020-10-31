using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PlayerWatching.Tests
{
    [TestClass]
    public class MultiPlayerWatcherTests
    {
        [TestMethod]
        public void MultiPlayerWatcherMonitoringTest()
        {
            var interval = TimeSpan.FromMilliseconds(200);
            var playerWatchers = new List<IPlayerWatcher> { new SmtcWatcher(), new YandexMusicWatcher() };
            var watcher = new MultiPlayerWatcher(playerWatchers, interval);
            var resetEvent = new ManualResetEventSlim();

            watcher.TrackChanged += (s, track) => resetEvent.Set();
            watcher.Initialize();

            var result = resetEvent.Wait(TimeSpan.FromSeconds(1));
            Assert.IsTrue(result, "TrackChanged event is not occured");
            Assert.IsNotNull(watcher.Track);
            Assert.IsFalse(watcher.Track.IsTrackEmpty);

            Console.WriteLine(watcher.Track);
            watcher.Dispose();
        }
    }
}
