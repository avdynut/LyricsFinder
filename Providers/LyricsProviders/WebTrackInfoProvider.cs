using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using System.Threading.Tasks;

namespace LyricsProviders
{
    public abstract class WebTrackInfoProvider : ITrackInfoProvider
    {
        public abstract Task<Track> FindTrackAsync(TrackInfo trackInfo);
    }
}
