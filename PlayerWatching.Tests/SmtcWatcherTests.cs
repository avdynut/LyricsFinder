using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PlayerWatching.Tests
{
    [TestClass]
    public class SmtcWatcherTests
    {
        [TestMethod]
        public void SmtcWatcherUpdateTest()
        {
            var watcher = new SmtcWatcher();

            Assert.IsNull(watcher.Track);
            Assert.AreEqual(PlayerState.Unknown, watcher.PlayerState);

            var result = watcher.UpdateMediaInfo();

            Assert.IsNotNull(watcher.Track);
            Console.WriteLine(watcher.Track);
            Console.WriteLine($"PlayerState: {watcher.PlayerState}");

            if (result)
            {
                Assert.IsFalse(watcher.Track.IsTrackEmpty);
                Assert.AreNotEqual(PlayerState.Unknown, watcher.PlayerState);
            }
            else
            {
                Assert.IsTrue(watcher.Track.IsTrackEmpty);
                Assert.AreEqual(PlayerState.Unknown, watcher.PlayerState);
            }
        }
    }
}
