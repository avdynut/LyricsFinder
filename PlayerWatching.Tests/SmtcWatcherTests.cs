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

            watcher.UpdateMediaInfo();

            Assert.IsNotNull(watcher.Track);
            Assert.IsFalse(watcher.Track.IsTrackEmpty);
            Assert.AreNotEqual(PlayerState.Unknown, watcher.PlayerState);

            Console.WriteLine(watcher.Track);
            Console.WriteLine($"PlayerState: {watcher.PlayerState}");
        }
    }
}
