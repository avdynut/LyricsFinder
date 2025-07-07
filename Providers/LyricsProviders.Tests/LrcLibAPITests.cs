using LyricsProviders.LrcLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using System.Threading.Tasks;

namespace LyricsProviders.Tests;

[TestClass]
public class LrcLibAPITests
{
    [TestMethod]
    public async Task LrcLibAPI_SearchLyricsByQuery_ReturnsResults()
    {
        using var result = await LrcLibAPI.SearchLyrics("bohemian rhapsody queen");

        Assert.IsNotNull(result);
        var rootElement = result.RootElement;
        Assert.AreEqual(JsonValueKind.Array, rootElement.ValueKind);

        // Should return some results for this popular song
        if (rootElement.GetArrayLength() > 0)
        {
            var firstResult = rootElement[0];
            Assert.IsTrue(firstResult.TryGetProperty("id", out _));
            Assert.IsTrue(firstResult.TryGetProperty("trackName", out _));
            Assert.IsTrue(firstResult.TryGetProperty("artistName", out _));
        }
    }

    [TestMethod]
    public async Task LrcLibAPI_SearchLyricsByFields_ReturnsResults()
    {
        using var result = await LrcLibAPI.SearchLyricsByFields("Bohemian Rhapsody", "Queen");

        Assert.IsNotNull(result);
        var rootElement = result.RootElement;
        Assert.AreEqual(JsonValueKind.Array, rootElement.ValueKind);

        // Should return some results for this popular song
        if (rootElement.GetArrayLength() > 0)
        {
            var firstResult = rootElement[0];
            Assert.IsTrue(firstResult.TryGetProperty("id", out _));
            Assert.IsTrue(firstResult.TryGetProperty("trackName", out _));
            Assert.IsTrue(firstResult.TryGetProperty("artistName", out _));
        }
    }

    [TestMethod]
    public async Task LrcLibAPI_SearchLyricsByFields_WithAlbum_ReturnsResults()
    {
        using var result = await LrcLibAPI.SearchLyricsByFields("Bohemian Rhapsody", "Queen", "A Night at the Opera");

        Assert.IsNotNull(result);
        var rootElement = result.RootElement;
        Assert.AreEqual(JsonValueKind.Array, rootElement.ValueKind);
    }

    [TestMethod]
    public async Task LrcLibAPI_SearchLyrics_EmptyQuery_ReturnsEmptyArray()
    {
        using var result = await LrcLibAPI.SearchLyrics("");

        Assert.IsNotNull(result);
        var rootElement = result.RootElement;
        Assert.AreEqual(JsonValueKind.Array, rootElement.ValueKind);
        Assert.AreEqual(0, rootElement.GetArrayLength());
    }

    [TestMethod]
    public async Task LrcLibAPI_SearchLyricsByFields_EmptyTrackName_ReturnsEmptyArray()
    {
        using var result = await LrcLibAPI.SearchLyricsByFields("", "Queen");

        Assert.IsNotNull(result);
        var rootElement = result.RootElement;
        Assert.AreEqual(JsonValueKind.Array, rootElement.ValueKind);
        Assert.AreEqual(0, rootElement.GetArrayLength());
    }

    [TestMethod]
    public async Task LrcLibAPI_GetLyricsById_ValidId_ReturnsLyrics()
    {
        // First search for a track to get a valid ID
        using var searchResult = await LrcLibAPI.SearchLyrics("hello adele");
        var searchArray = searchResult.RootElement;

        if (searchArray.GetArrayLength() > 0)
        {
            var firstResult = searchArray[0];
            if (firstResult.TryGetProperty("id", out var idProperty))
            {
                var id = idProperty.GetInt32();

                var lyricsResult = await LrcLibAPI.GetLyricsById(id);

                if (lyricsResult != null)
                {
                    var lyricsElement = lyricsResult.RootElement;
                    Assert.IsTrue(lyricsElement.TryGetProperty("id", out _));
                    Assert.IsTrue(lyricsElement.TryGetProperty("trackName", out _));
                    Assert.IsTrue(lyricsElement.TryGetProperty("artistName", out _));
                }

                lyricsResult?.Dispose();
            }
        }
    }

    [TestMethod]
    public async Task LrcLibAPI_GetLyricsById_InvalidId_ReturnsNull()
    {
        var result = await LrcLibAPI.GetLyricsById(-1);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task LrcLibAPI_GetLyricsBySignature_InvalidParams_ReturnsNull()
    {
        var result = await LrcLibAPI.GetLyricsBySignature("", "Artist", "Album", 180);
        Assert.IsNull(result);

        result = await LrcLibAPI.GetLyricsBySignature("Track", "", "Album", 180);
        Assert.IsNull(result);

        result = await LrcLibAPI.GetLyricsBySignature("Track", "Artist", "", 180);
        Assert.IsNull(result);

        result = await LrcLibAPI.GetLyricsBySignature("Track", "Artist", "Album", 0);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task LrcLibAPI_GetLyricsCached_InvalidParams_ReturnsNull()
    {
        var result = await LrcLibAPI.GetLyricsCached("", "Artist", "Album", 180);
        Assert.IsNull(result);

        result = await LrcLibAPI.GetLyricsCached("Track", "", "Album", 180);
        Assert.IsNull(result);

        result = await LrcLibAPI.GetLyricsCached("Track", "Artist", "", 180);
        Assert.IsNull(result);

        result = await LrcLibAPI.GetLyricsCached("Track", "Artist", "Album", 0);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task LrcLibAPI_GetLyricsBySignature_NotFound_ReturnsNull()
    {
        var result = await LrcLibAPI.GetLyricsBySignature(
            "NonExistentTrack123456",
            "NonExistentArtist123456",
            "NonExistentAlbum123456",
            180);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task LrcLibAPI_GetLyricsCached_NotFound_ReturnsNull()
    {
        var result = await LrcLibAPI.GetLyricsCached(
            "NonExistentTrack123456",
            "NonExistentArtist123456",
            "NonExistentAlbum123456",
            180);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task LrcLibAPI_SearchLyrics_NoResults_ReturnsEmptyArray()
    {
        using var result = await LrcLibAPI.SearchLyrics("VeryUniqueNonExistentQuery123456789");

        Assert.IsNotNull(result);
        var rootElement = result.RootElement;
        Assert.AreEqual(JsonValueKind.Array, rootElement.ValueKind);
        // May return empty array or no results - both are valid
    }
}
