using System.Text.Json.Serialization;

namespace ShazamIO.Models;

/// <summary>
/// Represents artist information.
/// </summary>
public class ArtistInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("verified")]
    public bool? Verified { get; set; }
    
    [JsonPropertyName("genres")]
    public List<string> Genres { get; set; } = new();
    
    [JsonPropertyName("alias")]
    public string? Alias { get; set; }
    
    [JsonPropertyName("genresPrimary")]
    public string? GenresPrimary { get; set; }
    
    [JsonPropertyName("avatar")]
    public object? Avatar { get; set; }
    
    [JsonPropertyName("adamid")]
    public string? AdamId { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    public string? GetAvatarUrl()
    {
        if (Avatar == null) return null;
        
        if (Avatar is string avatarStr)
            return avatarStr;
            
        if (Avatar is System.Text.Json.JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
                return jsonElement.GetString();
                
            if (jsonElement.TryGetProperty("default", out var defaultProp))
                return defaultProp.GetString();
        }
        
        return null;
    }
}

/// <summary>
/// Represents artist query options.
/// </summary>
public class ArtistQuery
{
    public List<string> Views { get; set; } = new();
    public List<string> Extend { get; set; } = new();
}

/// <summary>
/// Represents artist avatar dimensions.
/// </summary>
public class ArtistAvatar
{
    [JsonPropertyName("width")]
    public int Width { get; set; }
    
    [JsonPropertyName("height")]
    public int Height { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    public string GetUrlWithSize(int width, int height)
    {
        return Url.Replace("{w}", width.ToString()).Replace("{h}", height.ToString());
    }
}

/// <summary>
/// Represents artist attributes.
/// </summary>
public class ArtistAttribute
{
    [JsonPropertyName("genreNames")]
    public List<string> GenreNames { get; set; } = new();
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonPropertyName("artistBio")]
    public string? ArtistBio { get; set; }
}

/// <summary>
/// Base model for href references.
/// </summary>
public class BaseHref
{
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;
}

/// <summary>
/// Base model with id, type, and href.
/// </summary>
public class BaseIdTypeHref : BaseHref
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Represents album relationship.
/// </summary>
public class AlbumRelationship
{
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;
    
    [JsonPropertyName("next")]
    public string? Next { get; set; }
    
    [JsonPropertyName("data")]
    public List<BaseIdTypeHref> Data { get; set; } = new();
}

/// <summary>
/// Represents artist relationships.
/// </summary>
public class ArtistRelationships
{
    [JsonPropertyName("albums")]
    public AlbumRelationship? Albums { get; set; }
}

/// <summary>
/// Represents artwork information.
/// </summary>
public class Artwork
{
    [JsonPropertyName("width")]
    public int Width { get; set; }
    
    [JsonPropertyName("height")]
    public int Height { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonPropertyName("bgColor")]
    public string? BgColor { get; set; }
    
    [JsonPropertyName("textColor1")]
    public string? TextColor1 { get; set; }
    
    [JsonPropertyName("textColor2")]
    public string? TextColor2 { get; set; }
    
    [JsonPropertyName("textColor3")]
    public string? TextColor3 { get; set; }
    
    [JsonPropertyName("textColor4")]
    public string? TextColor4 { get; set; }
}

/// <summary>
/// Represents play parameters.
/// </summary>
public class PlayParams
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
}

/// <summary>
/// Represents album attributes.
/// </summary>
public class AlbumAttributes
{
    [JsonPropertyName("artwork")]
    public Artwork? Artwork { get; set; }
    
    [JsonPropertyName("artistName")]
    public string ArtistName { get; set; } = string.Empty;
    
    [JsonPropertyName("isSingle")]
    public bool IsSingle { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonPropertyName("isComplete")]
    public bool IsComplete { get; set; }
    
    [JsonPropertyName("genreNames")]
    public List<string> GenreNames { get; set; } = new();
    
    [JsonPropertyName("trackCount")]
    public int TrackCount { get; set; }
    
    [JsonPropertyName("isMasteredForItunes")]
    public bool IsMasteredForItunes { get; set; }
    
    [JsonPropertyName("releaseDate")]
    public string? ReleaseDate { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("recordLabel")]
    public string? RecordLabel { get; set; }
    
    [JsonPropertyName("copyright")]
    public string? Copyright { get; set; }
    
    [JsonPropertyName("playParams")]
    public PlayParams? PlayParams { get; set; }
    
    [JsonPropertyName("isCompilation")]
    public bool IsCompilation { get; set; }
}

/// <summary>
/// Represents an album model.
/// </summary>
public class AlbumModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;
    
    [JsonPropertyName("attributes")]
    public AlbumAttributes? Attributes { get; set; }
}

/// <summary>
/// View data for top songs.
/// </summary>
public class TopSongView
{
    [JsonPropertyName("href")]
    public string? Href { get; set; }
    
    [JsonPropertyName("next")]
    public string? Next { get; set; }
    
    [JsonPropertyName("data")]
    public List<object>? Data { get; set; }
}

/// <summary>
/// View data for top music videos.
/// </summary>
public class TopMusicVideosView
{
    [JsonPropertyName("href")]
    public string? Href { get; set; }
    
    [JsonPropertyName("next")]
    public string? Next { get; set; }
    
    [JsonPropertyName("data")]
    public List<object>? Data { get; set; }
}

/// <summary>
/// View data for similar artists.
/// </summary>
public class SimilarArtistsView
{
    [JsonPropertyName("href")]
    public string? Href { get; set; }
    
    [JsonPropertyName("next")]
    public string? Next { get; set; }
    
    [JsonPropertyName("data")]
    public List<object>? Data { get; set; }
}

/// <summary>
/// View data for latest release.
/// </summary>
public class LatestReleaseView
{
    [JsonPropertyName("href")]
    public string? Href { get; set; }
    
    [JsonPropertyName("data")]
    public List<AlbumModel>? Data { get; set; }
}

/// <summary>
/// View data for full albums.
/// </summary>
public class FullAlbumsView
{
    [JsonPropertyName("href")]
    public string? Href { get; set; }
    
    [JsonPropertyName("next")]
    public string? Next { get; set; }
    
    [JsonPropertyName("data")]
    public List<AlbumModel>? Data { get; set; }
}

/// <summary>
/// Represents artist views.
/// </summary>
public class ArtistViews
{
    [JsonPropertyName("top-music-videos")]
    public TopMusicVideosView? TopMusicVideos { get; set; }
    
    [JsonPropertyName("similar-artists")]
    public SimilarArtistsView? SimilarArtists { get; set; }
    
    [JsonPropertyName("latest-release")]
    public LatestReleaseView? LatestRelease { get; set; }
    
    [JsonPropertyName("full-albums")]
    public FullAlbumsView? FullAlbums { get; set; }
    
    [JsonPropertyName("top-songs")]
    public TopSongView? TopSongs { get; set; }
}

/// <summary>
/// Represents artist data (V3 API).
/// </summary>
public class ArtistV3
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("attributes")]
    public ArtistAttribute? Attributes { get; set; }
    
    [JsonPropertyName("relationships")]
    public ArtistRelationships? Relationships { get; set; }
    
    [JsonPropertyName("views")]
    public ArtistViews? Views { get; set; }
}

/// <summary>
/// Represents an error model.
/// </summary>
public class ErrorModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

/// <summary>
/// Represents an artist API response.
/// </summary>
public class ArtistResponse
{
    [JsonPropertyName("errors")]
    public List<ErrorModel> Errors { get; set; } = new();
    
    [JsonPropertyName("data")]
    public List<ArtistV3> Data { get; set; } = new();
}
