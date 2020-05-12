using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PlayerWatching.Tests
{
    [TestClass]
    public class YandexMusicWatcherTests
    {
        [TestMethod]
        public async Task YandexMusicWatcherStartMonitoringTest()
        {
            var watcher = new YandexMusicWatcher();
            watcher.StartMonitoring();
            await Task.Delay(3000);
        }
    }
}
