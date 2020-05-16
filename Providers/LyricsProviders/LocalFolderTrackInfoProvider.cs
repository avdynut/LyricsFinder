using LyricsFinder.Core;
using System;
using System.Threading.Tasks;

namespace LyricsProviders
{
    public class LocalFolderTrackInfoProvider : ITrackInfoProvider
    {
        public string Name => "Folder";
        public string LyricsFolderPath { get; }

        public LocalFolderTrackInfoProvider(string lyricsFolderPath)
        {
            LyricsFolderPath = lyricsFolderPath ?? throw new ArgumentNullException(nameof(lyricsFolderPath));
        }

        public Task<Track> FindTrackAsync(TrackInfo trackInfo)
        {
            throw new NotImplementedException();
        }
    }
}
