using LyricsFinder.Core;
using LyricsProviders.MusixMatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace LyricsProviders.Tests;

[TestClass]
public class MusixmatchTrackInfoProviderTests
{
    [TestMethod]
    public async Task MusixmatchTrackInfoProviderFindLyricsTest()
    {
        var provider = new MusixmatchTrackInfoProvider();
        var testTrack = TestTrack.SkilletHeroTrack;
        testTrack.TrackInfo.Artist += " ";
        var track = await provider.FindTrackAsync(testTrack.TrackInfo);

        Assert.IsNotNull(track.Lyrics);
        Assert.IsFalse(string.IsNullOrEmpty(track.Lyrics.Text?.Trim()));
        // Synced (LRC) lyrics are prefixed with timestamps; unsynced start with the plain line.
        Assert.IsTrue(track.Lyrics.Text.Contains("I'm just a step away"));
    }

    [TestMethod]
    public async Task MusixmatchTrackInfoProviderFindLyricsRussianTest()
    {
        var provider = new MusixmatchTrackInfoProvider();
        var trackInfo = new TrackInfo { Artist = "Танисия", Title = "Я закричу" };
        var track = await provider.FindTrackAsync(trackInfo);

        Assert.IsNotNull(track.Lyrics);
        Assert.IsFalse(string.IsNullOrEmpty(track.Lyrics.Text?.Trim()));
        Assert.IsTrue(track.Lyrics.Text.Contains("А я сотру горизонт"));
    }
}
