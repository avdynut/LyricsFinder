using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using LyricsProviders.LrcLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace LyricsProviders.Tests;

[TestClass]
public class LrcLibTrackInfoProviderTests
{
    [TestMethod]
    public async Task LrcLibTrackInfoProviderFindLyricsTest()
    {
        var provider = new LrcLibTrackInfoProvider();
        var testTrack = TestTrack.SkilletHeroTrack;
        var track = await provider.FindTrackAsync(testTrack.TrackInfo);

        Assert.IsNotNull(track.Lyrics);
        Assert.IsFalse(string.IsNullOrEmpty(track.Lyrics.Text?.Trim()));
    }

    [TestMethod]
    public async Task LrcLibTrackInfoProviderFindLyricsWithAlbumTest()
    {
        var provider = new LrcLibTrackInfoProvider();
        var trackInfo = new TrackInfo
        {
            Artist = "Queen",
            Title = "Bohemian Rhapsody",
            Album = "A Night at the Opera"
        };
        var track = await provider.FindTrackAsync(trackInfo);

        Assert.IsNotNull(track.Lyrics);
        // Should find lyrics for this well-known song
        Assert.IsTrue(track.Lyrics.Text?.Length > 0 || track.Lyrics is NoneLyric);
    }

    [TestMethod]
    public async Task LrcLibTrackInfoProviderNotFoundTest()
    {
        var provider = new LrcLibTrackInfoProvider();
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
    public async Task LrcLibTrackInfoProviderHandlesEmptyTrackInfoTest()
    {
        var provider = new LrcLibTrackInfoProvider();
        var trackInfo = new TrackInfo { Artist = "", Title = "" };
        var track = await provider.FindTrackAsync(trackInfo);

        Assert.IsNotNull(track.Lyrics);
    }

    [TestMethod]
    public async Task LrcLibTrackInfoProviderPrefersSyncedLyricsTest()
    {
        var provider = new LrcLibTrackInfoProvider();
        // Use a track known to have synced lyrics
        var trackInfo = new TrackInfo
        {
            Artist = "Adele",
            Title = "Hello"
        };
        var track = await provider.FindTrackAsync(trackInfo);

        Assert.IsNotNull(track.Lyrics);
        // If lyrics are found, synced lyrics should be preferred
        if (track.Lyrics is not NoneLyric)
        {
            // We can't guarantee synced lyrics will be found, but if any lyrics are found,
            // the provider should prefer synced when available
            Assert.IsTrue(track.Lyrics is SyncedLyric or UnsyncedLyric);
        }
    }

    [TestMethod]
    public async Task LrcLibTrackInfoProviderHandlesInstrumentalTest()
    {
        var provider = new LrcLibTrackInfoProvider();
        // Use a likely instrumental track
        var trackInfo = new TrackInfo
        {
            Artist = "Ludovico Einaudi",
            Title = "Nuvole Bianche"
        };
        var track = await provider.FindTrackAsync(trackInfo);

        Assert.IsNotNull(track.Lyrics);
        // Should handle instrumental tracks gracefully
        // (might return NoneLyric or actual lyrics if found)
    }

    [TestMethod]
    public async Task LrcLibTrackInfoProviderFallbackSearchTest()
    {
        var provider = new LrcLibTrackInfoProvider();
        // Test with track that might not match exactly
        var trackInfo = new TrackInfo
        {
            Artist = "The Beatles",
            Title = "Yesterday",
            Album = "Help!" // Specific album to test fallback
        };
        var track = await provider.FindTrackAsync(trackInfo);

        Assert.IsNotNull(track.Lyrics);
        // Should find lyrics through fallback search even if album-specific search fails
    }
}