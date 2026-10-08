using LyricsFinder.Core;
using LyricsProviders.LrcLib;
using System.Threading.Tasks;

namespace LyricsProviders.Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var provider = new LrcLibTrackInfoProvider();
            var trackInfo = new TrackInfo { Artist = "Elevation Worship", Title = "Available (Live)" };

            var track = await provider.FindTrackAsync(trackInfo);
        }
    }
}
