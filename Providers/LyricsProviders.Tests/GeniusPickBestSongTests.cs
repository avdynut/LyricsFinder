using LyricsFinder.Core;
using LyricsProviders.Genius;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace LyricsProviders.Tests;

[TestClass]
public class GeniusPickBestSongTests
{
    [TestMethod]
    public void RejectsUnrelatedFirstHitForYouTubeAd()
    {
        using var doc = JsonDocument.Parse("""
            {
              "response": {
                "hits": [
                  {
                    "type": "song",
                    "result": {
                      "title": "Interview: A Response to Donald Sterling's Remarks",
                      "url": "https://genius.com/Magic-johnson-interview-a-response-to-donald-sterlings-remarks-annotated",
                      "primary_artist": { "name": "Magic Johnson" }
                    }
                  },
                  {
                    "type": "song",
                    "result": {
                      "title": "The First Phone Call from Heaven",
                      "url": "https://genius.com/mitch-albom-the-first-phone-call-from-heaven-annotated",
                      "primary_artist": { "name": "Mitch Albom" }
                    }
                  }
                ]
              }
            }
            """);

        var matched = GeniusTrackInfoProvider.TryPickBestSong(
            doc.RootElement,
            new TrackInfo
            {
                Artist = "JustCall",
                Title = "The First Phone Call Could have been Answered by AI"
            },
            out var songUrl,
            out _,
            out _);

        Assert.IsFalse(matched);
        Assert.IsNull(songUrl);
    }

    [TestMethod]
    public void PicksMatchingSongOverEarlierUnrelatedHit()
    {
        using var doc = JsonDocument.Parse("""
            {
              "response": {
                "hits": [
                  {
                    "type": "song",
                    "result": {
                      "title": "Interview: A Response to Donald Sterling's Remarks",
                      "url": "https://genius.com/Magic-johnson-interview-a-response-to-donald-sterlings-remarks-annotated",
                      "primary_artist": { "name": "Magic Johnson" }
                    }
                  },
                  {
                    "type": "song",
                    "result": {
                      "title": "Hero",
                      "url": "https://genius.com/Skillet-hero-lyrics",
                      "primary_artist": { "name": "Skillet" }
                    }
                  }
                ]
              }
            }
            """);

        var matched = GeniusTrackInfoProvider.TryPickBestSong(
            doc.RootElement,
            new TrackInfo { Artist = "Skillet", Title = "Hero" },
            out var songUrl,
            out var title,
            out var artist);

        Assert.IsTrue(matched);
        Assert.AreEqual("https://genius.com/Skillet-hero-lyrics", songUrl);
        Assert.AreEqual("Hero", title);
        Assert.AreEqual("Skillet", artist);
    }
}
