using LyricsFinder.Core;
using LyricsProviders.DirectoriesProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LyricsProviders.Tests
{
    [TestClass]
    public class DirectoriesTrackInfoProviderTests
    {
        [TestMethod]
        public void CreateNotExistingFoldersTest()
        {
            var settings = new DirectoriesProviderSettings
            {
                LyricsDirectories = new List<string> { "1", "2", "3" }
            };
            var provider = new DirectoriesTrackInfoProvider(settings);

            Assert.IsNotNull(provider.LyricsFolders);
            Assert.AreEqual(0, provider.LyricsFolders.Count, "Folders are not exist");
        }

        [TestMethod]
        public async Task FindLyricsTest()
        {
            var folders = new List<string> { "a", "b", "c" };
            var settings = new DirectoriesProviderSettings { LyricsDirectories = folders };

            foreach (var folder in folders)
            {
                Directory.CreateDirectory(folder);
                var guid = Guid.NewGuid().ToString();
                await File.WriteAllTextAsync(Path.Combine(folder, guid), guid);
            }

            const string artist = "Ben";
            const string title = "Song";

            var lyricsFile = Path.Combine(folders[1], $"{artist.ToUpper()} - {title.ToLower()}.txt");
            var lyricsText = "blablabla";
            await File.WriteAllTextAsync(lyricsFile, lyricsText);

            var trackInfo = new TrackInfo { Artist = artist, Title = title };

            var provider = new DirectoriesTrackInfoProvider(settings);

            Assert.IsNotNull(provider.LyricsFolders);
            Assert.AreEqual(folders.Count, provider.LyricsFolders.Count);

            var track = await provider.FindTrackAsync(trackInfo);

            Assert.IsNotNull(track.Lyrics);
            Assert.AreEqual(lyricsText, track.Lyrics.Text);
            Assert.AreEqual(artist, track.Artist);
            Assert.AreEqual(title, track.Title);

            foreach (var folder in folders)
            {
                Directory.Delete(folder, true);
            }
        }
    }
}
