using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PlayerWatching.Tests
{
    [TestClass]
    public class YandexMusicWatcherTests
    {
        [TestMethod]
        public async Task YandexMusicWatcherStartMonitoringTest()
        {
            var interval = TimeSpan.FromMilliseconds(500);
            var watcher = new YandexMusicWatcher(interval);
            watcher.StartMonitoring();
            await Task.Delay(3000);
        }
    }
}
