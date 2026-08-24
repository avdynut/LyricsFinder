using LyricsFinder.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LyricsProviders.Tests;

[TestClass]
public class TrackMatchTests
{
    [TestMethod]
    public void ExactArtistAndTitleIsAcceptable()
    {
        var score = TrackMatch.Score("Hello", "Adele", new TrackInfo { Artist = "Adele", Title = "Hello" });
        Assert.IsTrue(TrackMatch.IsAcceptable(score));
        Assert.AreEqual(6, score);
    }

    [TestMethod]
    public void DifferentArtistIsRejected()
    {
        var score = TrackMatch.Score("Hello", "Omar Apollo", new TrackInfo { Artist = "Adele", Title = "Hello" });
        Assert.IsFalse(TrackMatch.IsAcceptable(score));
    }

    [TestMethod]
    public void DifferentTitleIsRejected()
    {
        var score = TrackMatch.Score("Someone Like You", "Adele", new TrackInfo { Artist = "Adele", Title = "Hello" });
        Assert.IsFalse(TrackMatch.IsAcceptable(score));
    }

    [TestMethod]
    public void UnboundedSubstringDoesNotMatchADifferentSong()
    {
        var score = TrackMatch.Score(
            "Can't Help Falling in Love",
            "Elvis Presley",
            new TrackInfo { Artist = "Elvis Presley", Title = "Love" });
        Assert.IsFalse(TrackMatch.IsAcceptable(score));
    }

    [TestMethod]
    public void ParentheticalAndThePrefixStillMatch()
    {
        var score = TrackMatch.Score(
            "Yesterday (Remastered)",
            "The Beatles",
            new TrackInfo { Artist = "Beatles", Title = "Yesterday" });
        Assert.IsTrue(TrackMatch.IsAcceptable(score));
    }
}
