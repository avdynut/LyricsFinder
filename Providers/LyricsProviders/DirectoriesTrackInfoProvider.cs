using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LyricsProviders
{
    public class DirectoriesTrackInfoProvider : ITrackInfoProvider
    {
        private const string ArtistMask = "%Artist";
        private const string TitleMask = "%Title";

        public string Name => "Directories";

        public string SearchPattern { get; }
        public List<DirectoryInfo> LyricsFolders { get; } = new List<DirectoryInfo>();

        public DirectoriesTrackInfoProvider(IEnumerable<string> lyricsFolders, string searchPattern = "%Artist - %Title")
        {
            SearchPattern = searchPattern ?? throw new ArgumentNullException(nameof(searchPattern));

            foreach (var folder in lyricsFolders)
            {
                var directory = new DirectoryInfo(folder);
                if (directory.Exists)
                {
                    LyricsFolders.Add(directory);
                }
            }
        }

        public async Task<Track> FindTrackAsync(TrackInfo trackInfo)
        {
            var track = new Track(trackInfo);

            foreach (var directory in LyricsFolders)
            {
                var enumerationOptions = new EnumerationOptions
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    RecurseSubdirectories = true
                };

                var pattern = SearchPattern.Replace(ArtistMask, trackInfo.Artist).Replace(TitleMask, trackInfo.Title);
                foreach (var file in directory.EnumerateFiles(pattern + "*", enumerationOptions))
                {
                    var lyrics = await File.ReadAllTextAsync(file.FullName);
                    if (lyrics.Length > 0)
                    {
                        track.Lyrics = new UnsyncedLyric(lyrics);
                        return track;
                    }
                }
            }

            track.Lyrics = new NoneLyric("Not Found");
            return track;
        }
    }
}
