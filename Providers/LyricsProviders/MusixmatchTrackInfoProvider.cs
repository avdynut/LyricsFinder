using LyricsFinder.Core;
using System;
using System.Threading.Tasks;

namespace LyricsProviders
{
    public class MusixmatchTrackInfoProvider : ITrackInfoProvider
    {
        public string Name => "MusixMatch";

        public Task<Track> FindTrackAsync(TrackInfo trackInfo)
        {
            throw new NotImplementedException();
        }
    }
}
