using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LyricsProviders
{
    public class MultiTrackInfoProvider : ITrackInfoProvider
    {
        public string Name => "Multi";
        public IEnumerable<ITrackInfoProvider> LyricsProviders { get; }

        public async Task<Track> FindTrackAsync(TrackInfo trackInfo)
        {
            foreach (var provider in LyricsProviders)
            {
                var track = await provider.FindTrackAsync(trackInfo);
                bool lyricsFound = !(track.Lyrics is null || track.Lyrics is NoneLyric);

                if (lyricsFound)
                {
                    return track;
                }
            }

            return new Track { Lyrics = new NoneLyric("Lyrics not found") };
        }

        public MultiTrackInfoProvider(IEnumerable<ITrackInfoProvider> lyricsProviders)
        {
            LyricsProviders = lyricsProviders ?? throw new ArgumentNullException(nameof(lyricsProviders));
        }
    }
}
