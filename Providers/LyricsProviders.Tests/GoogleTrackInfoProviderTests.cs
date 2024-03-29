using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using LyricsProviders.GoogleProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LyricsProviders.Tests
{
    [TestClass]
    public class GoogleTrackInfoProviderTests
    {
        [TestMethod]
        public async Task GoogleLyricsProviderNotFoundLyricsTest()
        {
            var provider = new GoogleTrackInfoProvider(new GoogleProviderSettings());
            var trackInfo = new TrackInfo { Artist = "not song", Title = "query" };
            var track = await provider.FindTrackAsync(trackInfo);

            Assert.IsNotNull(track.Lyrics);
            Assert.IsNull(track.Lyrics.Text);
            Assert.IsInstanceOfType(track.Lyrics, typeof(NoneLyric));
        }

        [DataTestMethod]
        [DynamicData(nameof(GetTestLyrics), DynamicDataSourceType.Method)]
        public async Task GoogleTrackInfoProviderFindLyricsTest(TestTrack testTrack)
        {
            var provider = new GoogleTrackInfoProvider(new GoogleProviderSettings());
            var track = await provider.FindTrackAsync(testTrack.TrackInfo);

            Assert.IsNotNull(track.Lyrics);
            Assert.IsFalse(string.IsNullOrEmpty(track.Lyrics.Text?.Trim()));
            Assert.AreEqual(testTrack.Lyrics.Text, track.Lyrics.Text);
        }

        private static IEnumerable<object[]> GetTestLyrics() => new[]
        {
            new object[] { TestTrack.SkilletHeroTrack },
            new object[] { TestTrack.AriaImFreeTrack }
        };
    }
}
