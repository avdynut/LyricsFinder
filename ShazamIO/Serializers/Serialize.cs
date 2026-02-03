using System.Text.Json;
using ShazamIO.Models;

namespace ShazamIO.Serializers;

/// <summary>
/// Provides serialization utilities for Shazam API responses.
/// </summary>
public static class Serialize
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Deserializes a track info from a JSON document.
    /// </summary>
    public static TrackInfo? Track(JsonDocument document)
    {
        if (document.RootElement.TryGetProperty("track", out var trackElement))
        {
            return JsonSerializer.Deserialize<TrackInfo>(trackElement.GetRawText(), JsonOptions);
        }
        return null;
    }

    /// <summary>
    /// Deserializes a full track response from a JSON document.
    /// </summary>
    public static ResponseTrack? FullTrack(JsonDocument document)
    {
        return JsonSerializer.Deserialize<ResponseTrack>(document.RootElement.GetRawText(), JsonOptions);
    }

    /// <summary>
    /// Deserializes an artist response from a JSON document.
    /// </summary>
    public static ArtistResponse? Artist(JsonDocument document)
    {
        return JsonSerializer.Deserialize<ArtistResponse>(document.RootElement.GetRawText(), JsonOptions);
    }

    /// <summary>
    /// Deserializes playlist tracks from a JSON document.
    /// </summary>
    public static PlaylistTracksResponse? PlaylistTracks(JsonDocument document)
    {
        return JsonSerializer.Deserialize<PlaylistTracksResponse>(document.RootElement.GetRawText(), JsonOptions);
    }

    /// <summary>
    /// Deserializes a list of albums from a JSON document.
    /// </summary>
    public static List<AlbumModel>? Albums(JsonDocument document)
    {
        if (document.RootElement.TryGetProperty("data", out var dataElement))
        {
            return JsonSerializer.Deserialize<List<AlbumModel>>(dataElement.GetRawText(), JsonOptions);
        }
        return null;
    }

    /// <summary>
    /// Deserializes an album from a JSON document.
    /// </summary>
    public static AlbumModel? Album(JsonDocument document)
    {
        if (document.RootElement.TryGetProperty("data", out var dataElement) && 
            dataElement.ValueKind == JsonValueKind.Array)
        {
            var albums = JsonSerializer.Deserialize<List<AlbumModel>>(dataElement.GetRawText(), JsonOptions);
            return albums?.FirstOrDefault();
        }
        return null;
    }

    /// <summary>
    /// Deserializes YouTube data from a JSON document.
    /// </summary>
    public static YoutubeData? Youtube(JsonDocument document)
    {
        return JsonSerializer.Deserialize<YoutubeData>(document.RootElement.GetRawText(), JsonOptions);
    }

    /// <summary>
    /// Deserializes full albums view from a JSON document.
    /// </summary>
    public static FullAlbumsView? ArtistAlbums(JsonDocument document)
    {
        return JsonSerializer.Deserialize<FullAlbumsView>(document.RootElement.GetRawText(), JsonOptions);
    }

    /// <summary>
    /// Converts a JsonDocument to a typed object.
    /// </summary>
    public static T? ToObject<T>(JsonDocument document) where T : class
    {
        return JsonSerializer.Deserialize<T>(document.RootElement.GetRawText(), JsonOptions);
    }

    /// <summary>
    /// Converts a JsonElement to a typed object.
    /// </summary>
    public static T? ToObject<T>(JsonElement element) where T : class
    {
        return JsonSerializer.Deserialize<T>(element.GetRawText(), JsonOptions);
    }
}
