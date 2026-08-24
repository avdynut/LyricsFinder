using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using LyricsProviders.LrcLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace LyricsProviders.Tests;

[TestClass]
public class LrcLibPickBestLyricsTests
{
    [TestMethod]
    public void PrefersMatchingSongOverEarlierDifferentSyncedHit()
    {
        using var doc = JsonDocument.Parse("""
            [
              {
                "trackName": "Hello",
                "artistName": "Omar Apollo",
                "syncedLyrics": "[00:01.00]wrong",
                "plainLyrics": "wrong",
                "instrumental": false
              },
              {
                "trackName": "Hello",
                "artistName": "Adele",
                "syncedLyrics": "[00:01.00]right",
                "plainLyrics": "right",
                "instrumental": false
              }
            ]
            """);

        var lyrics = LrcLibTrackInfoProvider.PickBestLyrics(
            doc.RootElement,
            new TrackInfo { Artist = "Adele", Title = "Hello" });

        Assert.IsInstanceOfType(lyrics, typeof(SyncedLyric));
        Assert.AreEqual("[00:01.00]right", lyrics.Text);
    }

    [TestMethod]
    public void PrefersSyncedWhenArtistAndTitleMatchEqually()
    {
        using var doc = JsonDocument.Parse("""
            [
              {
                "trackName": "Hello",
                "artistName": "Adele",
                "syncedLyrics": null,
                "plainLyrics": "plain",
                "instrumental": false
              },
              {
                "trackName": "Hello",
                "artistName": "Adele",
                "syncedLyrics": "[00:01.00]synced",
                "plainLyrics": "plain",
                "instrumental": false
              }
            ]
            """);

        var lyrics = LrcLibTrackInfoProvider.PickBestLyrics(
            doc.RootElement,
            new TrackInfo { Artist = "Adele", Title = "Hello" });

        Assert.IsInstanceOfType(lyrics, typeof(SyncedLyric));
        Assert.AreEqual("[00:01.00]synced", lyrics.Text);
    }

    [TestMethod]
    public void KeepsCorrectUnsyncedInsteadOfCoverSynced()
    {
        using var doc = JsonDocument.Parse("""
            [
              {
                "trackName": "Yesterday",
                "artistName": "The Beatles",
                "syncedLyrics": null,
                "plainLyrics": "correct",
                "instrumental": false
              },
              {
                "trackName": "Yesterday",
                "artistName": "Cover Band",
                "syncedLyrics": "[00:01.00]wrong",
                "plainLyrics": "wrong",
                "instrumental": false
              }
            ]
            """);

        var lyrics = LrcLibTrackInfoProvider.PickBestLyrics(
            doc.RootElement,
            new TrackInfo { Artist = "The Beatles", Title = "Yesterday" });

        Assert.IsInstanceOfType(lyrics, typeof(UnsyncedLyric));
        Assert.AreEqual("correct", lyrics.Text);
    }

    [TestMethod]
    public void ReturnsNullWhenNoResultMatchesTheQuery()
    {
        using var doc = JsonDocument.Parse("""
            [
              {
                "trackName": "Hello",
                "artistName": "Omar Apollo",
                "syncedLyrics": "[00:01.00]wrong",
                "plainLyrics": "wrong",
                "instrumental": false
              }
            ]
            """);

        var lyrics = LrcLibTrackInfoProvider.PickBestLyrics(
            doc.RootElement,
            new TrackInfo { Artist = "Adele", Title = "Hello" });

        Assert.IsNull(lyrics);
    }
}
