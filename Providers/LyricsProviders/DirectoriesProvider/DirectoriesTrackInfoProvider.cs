using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LyricsProviders.DirectoriesProvider
{
    public class DirectoriesTrackInfoProvider : ITrackInfoProvider
    {
        public const string ArtistMask = "%Artist";
        public const string TitleMask = "%Title";

        public static string DefaultFileNameMask { get; } = $"{ArtistMask} - {TitleMask}";

        public const string Name = "Directories";
        public string DisplayName => Name;

        public DirectoriesProviderSettings Settings { get; }
        public List<DirectoryInfo> LyricsFolders { get; } = new List<DirectoryInfo>();

        public DirectoriesTrackInfoProvider(DirectoriesProviderSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));

            foreach (var folder in settings.LyricsDirectories)
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

                var pattern = GetFileName(Settings.LyricsFileNamePattern, trackInfo);
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

        public static string GetFileName(string pattern, TrackInfo trackInfo)
        {
            return pattern.Replace(ArtistMask, trackInfo.Artist)
                          .Replace(TitleMask, trackInfo.Title)
                          .Replace("/", " ")
                          .Replace("\\", " ");
        }
    }
}
