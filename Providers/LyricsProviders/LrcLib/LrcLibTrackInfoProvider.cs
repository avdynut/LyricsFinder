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
            using var searchDoc = await LrcLibAPI.SearchLyricsByFields(trackInfo.Title, trackInfo.Artist);
            var lyrics = PickBestLyrics(searchDoc.RootElement, trackInfo, out var matchedTitle, out var matchedArtist);
            if (lyrics != null)
            {
                ApplyMatchedIdentity(track, matchedTitle, matchedArtist);
                track.Lyrics = lyrics;
                return track;
            }

            var combinedQuery = $"{trackInfo.Artist} {trackInfo.Title}";
            using var generalDoc = await LrcLibAPI.SearchLyrics(combinedQuery);
            lyrics = PickBestLyrics(generalDoc.RootElement, trackInfo, out matchedTitle, out matchedArtist);
            if (lyrics != null)
            {
                ApplyMatchedIdentity(track, matchedTitle, matchedArtist);
                track.Lyrics = lyrics;
                return track;
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

    internal static ILyric PickBestLyrics(JsonElement searchResults, TrackInfo trackInfo)
    {
        return PickBestLyrics(searchResults, trackInfo, out _, out _);
    }

    internal static ILyric PickBestLyrics(
        JsonElement searchResults,
        TrackInfo trackInfo,
        out string matchedTitle,
        out string matchedArtist)
    {
        matchedTitle = null;
        matchedArtist = null;

        if (searchResults.ValueKind != JsonValueKind.Array)
            return null;

        ILyric best = null;
        var bestScore = 0;
        var bestIsSynced = false;

        foreach (var result in searchResults.EnumerateArray())
        {
            var resultTitle = result.TryGetProperty("trackName", out var titleProp) ? titleProp.GetString() : null;
            var resultArtist = result.TryGetProperty("artistName", out var artistProp) ? artistProp.GetString() : null;
            var score = TrackMatch.Score(resultTitle, resultArtist, trackInfo);
            if (!TrackMatch.IsAcceptable(score))
                continue;

            var lyrics = ExtractLyricsFromResult(result);
            if (lyrics is null or NoneLyric)
                continue;

            var isSynced = lyrics is SyncedLyric;
            if (score > bestScore || (score == bestScore && isSynced && !bestIsSynced))
            {
                best = lyrics;
                bestScore = score;
                bestIsSynced = isSynced;
                matchedTitle = resultTitle;
                matchedArtist = resultArtist;
            }
        }

        return best;
    }

    private static void ApplyMatchedIdentity(Track track, string title, string artist)
    {
        if (!string.IsNullOrWhiteSpace(title))
            track.Title = title;
        if (!string.IsNullOrWhiteSpace(artist))
            track.Artist = artist;
    }

    private static ILyric ExtractLyricsFromResult(JsonElement result)
    {
        if (result.TryGetProperty("instrumental", out var instrumentalProperty) &&
            instrumentalProperty.GetBoolean())
        {
            return new NoneLyric("Track is instrumental");
        }

        if (result.TryGetProperty("syncedLyrics", out var syncedLyricsProperty) &&
            syncedLyricsProperty.ValueKind != JsonValueKind.Null &&
            !string.IsNullOrEmpty(syncedLyricsProperty.GetString()))
        {
            var syncedLyrics = syncedLyricsProperty.GetString();
            return new SyncedLyric(syncedLyrics, SyncedLyricType.Lrc);
        }

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
