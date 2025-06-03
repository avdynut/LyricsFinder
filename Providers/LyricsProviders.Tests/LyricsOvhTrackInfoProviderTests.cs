using LyricsProviders.LyricsOvh;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace LyricsProviders.Tests;

[TestClass]
public class LyricsOvhTrackInfoProviderTests
{
    [TestMethod]
    public async Task LyricsOvhTrackInfoProviderFindLyricsTest()
    {
        var provider = new LyricsOvhTrackInfoProvider();
        var testTrack = TestTrack.SkilletHeroTrack;
        var track = await provider.FindTrackAsync(testTrack.TrackInfo);

        Assert.IsNotNull(track.Lyrics);
        Assert.IsFalse(string.IsNullOrEmpty(track.Lyrics.Text?.Trim()));
        Assert.IsTrue(track.Lyrics.Text.StartsWith("I'm just a step away"));
    }
}
