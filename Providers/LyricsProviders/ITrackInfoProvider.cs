using LyricsFinder.Core;
using System.Threading.Tasks;

namespace LyricsProviders
{
    public interface ITrackInfoProvider
    {
        string Name { get; }
        Task<Track> FindTrackAsync(TrackInfo trackInfo);
    }
}
