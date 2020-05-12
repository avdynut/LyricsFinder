using LyricsFinder.Core;
using System;
using System.Threading.Tasks;

namespace LyricsProviders
{
    public class LocalFileTrackInfoProvider : ITrackInfoProvider
    {
        public string LyricsFolderPath { get; }

        public LocalFileTrackInfoProvider(string lyricsFolderPath)
        {
            LyricsFolderPath = lyricsFolderPath ?? throw new ArgumentNullException(nameof(lyricsFolderPath));
        }

        public Task<Track> FindTrackAsync(TrackInfo trackInfo)
        {
            throw new NotImplementedException();
        }
    }
}
