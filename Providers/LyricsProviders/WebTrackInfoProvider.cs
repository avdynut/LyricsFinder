using LyricsFinder.Core;
using System.Threading.Tasks;

namespace LyricsProviders
{
    public abstract class WebTrackInfoProvider : ITrackInfoProvider
    {
        public abstract string Name { get; }

        public abstract Task<Track> FindTrackAsync(TrackInfo trackInfo);
    }
}
