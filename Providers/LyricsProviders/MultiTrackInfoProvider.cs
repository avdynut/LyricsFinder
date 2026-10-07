using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LyricsProviders
{
    public class MultiTrackInfoProvider : ITrackInfoProvider
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly Func<IEnumerable<ITrackInfoProvider>> _lyricsProviders;

        public string DisplayName => "Multi";
        public IEnumerable<ITrackInfoProvider> LyricsProviders => _lyricsProviders();

        public ITrackInfoProvider CurrentProvider { get; private set; }

        public async Task<Track> FindTrackAsync(TrackInfo trackInfo)
        {
            _logger.Trace("Start searching lyrics");

            Track unsyncedFallback = null;
            ITrackInfoProvider unsyncedProvider = null;

            foreach (var provider in _lyricsProviders().ToList())
            {
                var track = await provider.FindTrackAsync(trackInfo);

                if (track.Lyrics is SyncedLyric)
                {
                    _logger.Info($"Synced lyrics found by {provider.DisplayName} provider");
                    CurrentProvider = provider;
                    return track;
                }

                if (unsyncedFallback == null && track.Lyrics is not null && track.Lyrics is not NoneLyric)
                {
                    _logger.Debug($"Unsynced lyrics found by {provider.DisplayName} provider, continuing search for synced");
                    unsyncedFallback = track;
                    unsyncedProvider = provider;
                }
            }

            if (unsyncedFallback != null)
            {
                _logger.Info($"Lyrics found by {unsyncedProvider.DisplayName} provider");
                CurrentProvider = unsyncedProvider;
                return unsyncedFallback;
            }

            return new Track { Lyrics = new NoneLyric("Lyrics not found") };
        }

        public MultiTrackInfoProvider(IEnumerable<ITrackInfoProvider> lyricsProviders)
            : this(lyricsProviders == null ? null : () => lyricsProviders)
        {
        }

        public MultiTrackInfoProvider(Func<IEnumerable<ITrackInfoProvider>> lyricsProviders)
        {
            _lyricsProviders = lyricsProviders ?? throw new ArgumentNullException(nameof(lyricsProviders));
        }
    }
}
