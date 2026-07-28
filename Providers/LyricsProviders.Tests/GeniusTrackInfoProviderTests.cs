using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using LyricsProviders.Genius;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace LyricsProviders.Tests;

[TestClass]
public class GeniusTrackInfoProviderTests
{
    [TestMethod]
    public async Task GeniusTrackInfoProviderFindLyricsTest()
    {
        var provider = new GeniusTrackInfoProvider();
        var track = await provider.FindTrackAsync(TestTrack.SkilletHeroTrack.TrackInfo);

        Assert.IsNotNull(track.Lyrics);
        Assert.IsFalse(string.IsNullOrEmpty(track.Lyrics.Text?.Trim()));
        Assert.IsTrue(track.Lyrics.Text.Contains("I need a hero"));
        Assert.IsNotNull(track.Lyrics.Source);
        Assert.IsTrue(track.Lyrics.Source.Host.Contains("genius.com"));
    }

    [TestMethod]
    public async Task GeniusTrackInfoProviderNotFoundTest()
    {
        var provider = new GeniusTrackInfoProvider();
        var trackInfo = new TrackInfo
        {
            Artist = "NonExistentArtist123456",
            Title = "NonExistentSong123456"
        };
        var track = await provider.FindTrackAsync(trackInfo);

        Assert.IsNotNull(track.Lyrics);
        Assert.IsInstanceOfType(track.Lyrics, typeof(NoneLyric));
    }

    [TestMethod]
    public async Task GeniusTrackInfoProviderHandlesEmptyTrackInfoTest()
    {
        var provider = new GeniusTrackInfoProvider();
        var track = await provider.FindTrackAsync(new TrackInfo { Artist = "", Title = "" });

        Assert.IsNotNull(track.Lyrics);
        Assert.IsInstanceOfType(track.Lyrics, typeof(NoneLyric));
    }
}
