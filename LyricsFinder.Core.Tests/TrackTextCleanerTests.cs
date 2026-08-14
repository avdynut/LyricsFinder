using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LyricsFinder.Core.Tests
{
    [TestClass]
    public class TrackTextCleanerTests
    {
        [DataTestMethod]
        [DataRow(null, null)]
        [DataRow("", "")]
        [DataRow("   ", "   ")]
        public void Clean_ReturnsOriginal_WhenNullOrWhitespace(string input, string expected)
        {
            Assert.AreEqual(expected, TrackTextCleaner.Clean(input));
        }

        [DataTestMethod]
        [DataRow("Song Title | Channel Name", "Song Title")]
        [DataRow("Song Title｜Channel Name", "Song Title")]
        [DataRow("Song Title | Official Audio", "Song Title")]
        public void Clean_StripsTextAfterPipe(string input, string expected)
        {
            Assert.AreEqual(expected, TrackTextCleaner.Clean(input));
        }

        [DataTestMethod]
        [DataRow("Song Title (Official Music Video)", "Song Title")]
        [DataRow("Song Title (Official Video)", "Song Title")]
        [DataRow("Song Title [Official Audio]", "Song Title")]
        [DataRow("Song Title (Lyric Video)", "Song Title")]
        [DataRow("Song Title (Lyrics)", "Song Title")]
        [DataRow("Song Title (Audio)", "Song Title")]
        [DataRow("Song Title (Visualizer)", "Song Title")]
        [DataRow("Song Title (Remastered)", "Song Title")]
        public void Clean_RemovesKnownYouTubeNoiseLabels(string input, string expected)
        {
            Assert.AreEqual(expected, TrackTextCleaner.Clean(input));
        }

        [DataTestMethod]
        [DataRow("(Don't Fear) The Reaper", "(Don't Fear) The Reaper")]
        [DataRow("Snow (Hey Oh)", "Snow (Hey Oh)")]
        [DataRow("Song (Live)", "Song (Live)")]
        [DataRow("Song (feat. Artist)", "Song (feat. Artist)")]
        [DataRow("Song (Live (2019))", "Song (Live (2019))")]
        public void Clean_PreservesMeaningfulParentheses(string input, string expected)
        {
            Assert.AreEqual(expected, TrackTextCleaner.Clean(input));
        }

        [DataTestMethod]
        [DataRow("Artist Name - Topic", "Artist Name")]
        [DataRow("ArtistNameVEVO", "ArtistName")]
        [DataRow("Artist Name VEVO", "Artist Name")]
        public void Clean_StripsYouTubeArtistSuffixes(string input, string expected)
        {
            Assert.AreEqual(expected, TrackTextCleaner.Clean(input));
        }

        [TestMethod]
        public void Clean_ReturnsOriginal_WhenCleaningWouldEmptyText()
        {
            Assert.AreEqual("(Official Video)", TrackTextCleaner.Clean("(Official Video)"));
        }

        [TestMethod]
        public void Clean_CombinesPipeAndNoiseLabel()
        {
            Assert.AreEqual(
                "Bohemian Rhapsody",
                TrackTextCleaner.Clean("Bohemian Rhapsody (Official Video) | Queen"));
        }
    }
}
