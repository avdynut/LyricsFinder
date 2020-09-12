using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LyricsProviders
{
    public class MultiTrackInfoProvider : ITrackInfoProvider
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public string DisplayName => "Multi";
        public IEnumerable<ITrackInfoProvider> LyricsProviders { get; }

        public ITrackInfoProvider CurrentProvider { get; private set; }

        public async Task<Track> FindTrackAsync(TrackInfo trackInfo)
        {
            _logger.Trace("Start searching lyrics");

            foreach (var provider in LyricsProviders)
            {
                var track = await provider.FindTrackAsync(trackInfo);
                bool lyricsFound = !(track.Lyrics is null || track.Lyrics is NoneLyric);

                if (lyricsFound)
                {
                    _logger.Info($"Lyrics found by {provider.DisplayName} provider");
                    CurrentProvider = provider;
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
