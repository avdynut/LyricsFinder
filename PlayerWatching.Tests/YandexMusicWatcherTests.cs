using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PlayerWatching.Tests
{
    [TestClass]
    public class YandexMusicWatcherTests
    {
        [TestMethod]
        public void YandexMusicWatcherUpdateTest()
        {
            var watcher = new YandexMusicWatcher();
            watcher.Initialize();

            Assert.IsNull(watcher.Track);
            Assert.AreEqual(PlayerState.Unknown, watcher.PlayerState);

            var result = watcher.UpdateMediaInfo();

            Assert.IsNotNull(watcher.Track);
            Console.WriteLine(watcher.Track);
            Console.WriteLine($"PlayerState: {watcher.PlayerState}");

            if (result)
            {
                Assert.IsFalse(watcher.Track.IsTrackEmpty);
                // Assert.AreNotEqual(PlayerState.Unknown, watcher.PlayerState);
            }
            else
            {
                Assert.IsTrue(watcher.Track.IsTrackEmpty);
                Assert.AreEqual(PlayerState.Unknown, watcher.PlayerState);
            }

            watcher.Dispose();
        }
    }
}
