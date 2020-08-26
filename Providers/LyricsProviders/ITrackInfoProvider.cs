using LyricsFinder.Core;
using System.Threading.Tasks;

namespace LyricsProviders
{
    public interface ITrackInfoProvider
    {
        string DisplayName { get; }
        Task<Track> FindTrackAsync(TrackInfo trackInfo);
    }
}
