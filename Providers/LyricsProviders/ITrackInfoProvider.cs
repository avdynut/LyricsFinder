using LyricsFinder.Core;
using System.Threading.Tasks;

namespace LyricsProviders
{
    public interface ITrackInfoProvider
    {
        Task<Track> FindTrackAsync(TrackInfo trackInfo);
    }
}
