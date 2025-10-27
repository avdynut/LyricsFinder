using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using NLog;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace LyricsProviders.LrcLib;

public class LrcLibTrackInfoProvider : ITrackInfoProvider
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public const string Name = "LrcLib";
    public string DisplayName => Name;

    public async Task<Track> FindTrackAsync(TrackInfo trackInfo)
    {
        var track = new Track(trackInfo);

        try
        {
            // Strategy 1: Try field-based search (more precise)
            using var searchDoc = await LrcLibAPI.SearchLyricsByFields(trackInfo.Title, trackInfo.Artist);
            var searchResults = searchDoc.RootElement;

            if (searchResults.ValueKind == JsonValueKind.Array && searchResults.GetArrayLength() > 0)
            {
                var lyrics = ExtractLyricsFromResult(searchResults[0]);
                if (lyrics != null)
                {
                    track.Lyrics = lyrics;
                    return track;
                }
            }

            // Strategy 2: Try general search with combined query
            var combinedQuery = $"{trackInfo.Artist} {trackInfo.Title}";
            using var generalDoc = await LrcLibAPI.SearchLyrics(combinedQuery);
            var generalResults = generalDoc.RootElement;

            if (generalResults.ValueKind == JsonValueKind.Array && generalResults.GetArrayLength() > 0)
            {
                var lyrics = ExtractLyricsFromResult(generalResults[0]);
                if (lyrics != null)
                {
                    track.Lyrics = lyrics;
                    return track;
                }
            }

            track.Lyrics = new NoneLyric("No lyrics found");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching lyrics from LrcLib");
            track.Lyrics = new NoneLyric(ex.Message);
        }

        return track;
    }

    private static ILyric ExtractLyricsFromResult(JsonElement result)
    {
        // Check if the track is marked as instrumental
        if (result.TryGetProperty("instrumental", out var instrumentalProperty) &&
            instrumentalProperty.GetBoolean())
        {
            return new NoneLyric("Track is instrumental");
        }

        // Prefer synced lyrics over plain lyrics
        if (result.TryGetProperty("syncedLyrics", out var syncedLyricsProperty) &&
            syncedLyricsProperty.ValueKind != JsonValueKind.Null &&
            !string.IsNullOrEmpty(syncedLyricsProperty.GetString()))
        {
            var syncedLyrics = syncedLyricsProperty.GetString();
            return new SyncedLyric(syncedLyrics, SyncedLyricType.Lrc);
        }

        // Fall back to plain lyrics if synced lyrics are not available
        if (result.TryGetProperty("plainLyrics", out var plainLyricsProperty) &&
            plainLyricsProperty.ValueKind != JsonValueKind.Null &&
            !string.IsNullOrEmpty(plainLyricsProperty.GetString()))
        {
            var plainLyrics = plainLyricsProperty.GetString();
            return new UnsyncedLyric(plainLyrics);
        }

        return null;
    }
}