using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LyricsProviders.DirectoriesProvider
{
    public class DirectoriesTrackInfoProvider : ITrackInfoProvider
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

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

            Settings.LyricsDirectories.ForEach(x => CheckFolder(x));
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

                // Prioritize .lrc files (synced lyrics) over other formats
                var allFiles = directory.EnumerateFiles(pattern + "*", enumerationOptions).ToList();
                var lrcFiles = allFiles.Where(f => f.Extension.Equals(".lrc", StringComparison.OrdinalIgnoreCase)).ToList();
                var otherFiles = allFiles.Where(f => !f.Extension.Equals(".lrc", StringComparison.OrdinalIgnoreCase)).ToList();

                // First, try to find .lrc files
                foreach (var file in lrcFiles)
                {
                    var lyrics = await File.ReadAllTextAsync(file.FullName);
                    if (lyrics.Length > 0)
                    {
                        // Check if it's actually in LRC format with timestamps
                        if (LrcParser.IsLrcFormat(lyrics))
                        {
                            track.Lyrics = new SyncedLyric(lyrics, SyncedLyricType.Lrc) { Source = new Uri(file.FullName) };
                            return track;
                        }
                        else
                        {
                            // .lrc file without timestamps, treat as unsynced
                            track.Lyrics = new UnsyncedLyric(lyrics) { Source = new Uri(file.FullName) };
                            return track;
                        }
                    }
                }

                // Then, try other files
                foreach (var file in otherFiles)
                {
                    var lyrics = await File.ReadAllTextAsync(file.FullName);
                    if (lyrics.Length > 0)
                    {
                        // Check if the content is in LRC format (even if not .lrc extension)
                        if (LrcParser.IsLrcFormat(lyrics))
                        {
                            track.Lyrics = new SyncedLyric(lyrics, SyncedLyricType.Lrc) { Source = new Uri(file.FullName) };
                            return track;
                        }
                        else
                        {
                            track.Lyrics = new UnsyncedLyric(lyrics) { Source = new Uri(file.FullName) };
                            return track;
                        }
                    }
                }
            }

            track.Lyrics = new NoneLyric("Not Found");
            return track;
        }

        public static string GetFileName(string pattern, TrackInfo trackInfo)
        {
            var filename = pattern.Replace(ArtistMask, trackInfo.Artist)
                          .Replace(TitleMask, trackInfo.Title);

            // remove illegal chars
            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }

        private void CheckFolder(string folder)
        {
            try
            {
                var directory = new DirectoryInfo(folder);
                directory.Create();

                if (directory.Exists)
                {
                    LyricsFolders.Add(directory);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, $"Incorrect folder for lyrics: {folder}");
            }
        }
    }
}
