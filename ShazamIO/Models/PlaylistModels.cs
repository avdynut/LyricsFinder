using System.Text.Json.Serialization;

namespace ShazamIO.Models;

/// <summary>
/// Represents playlist attributes.
/// </summary>
public class PlaylistAttributes
{
    [JsonPropertyName("curatorName")]
    public string? CuratorName { get; set; }
    
    [JsonPropertyName("lastModifiedDate")]
    public string? LastModifiedDate { get; set; }
    
    [JsonPropertyName("isChart")]
    public bool IsChart { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("playlistType")]
    public string? PlaylistType { get; set; }
    
    [JsonPropertyName("description")]
    public PlaylistDescription? Description { get; set; }
    
    [JsonPropertyName("artwork")]
    public Artwork? Artwork { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Represents playlist description.
/// </summary>
public class PlaylistDescription
{
    [JsonPropertyName("standard")]
    public string? Standard { get; set; }
    
    [JsonPropertyName("short")]
    public string? Short { get; set; }
}

/// <summary>
/// Represents song attributes.
/// </summary>
public class SongAttributes
{
    [JsonPropertyName("albumName")]
    public string? AlbumName { get; set; }
    
    [JsonPropertyName("genreNames")]
    public List<string> GenreNames { get; set; } = new();
    
    [JsonPropertyName("trackNumber")]
    public int TrackNumber { get; set; }
    
    [JsonPropertyName("releaseDate")]
    public string? ReleaseDate { get; set; }
    
    [JsonPropertyName("durationInMillis")]
    public int DurationInMillis { get; set; }
    
    [JsonPropertyName("isrc")]
    public string? Isrc { get; set; }
    
    [JsonPropertyName("artwork")]
    public Artwork? Artwork { get; set; }
    
    [JsonPropertyName("composerName")]
    public string? ComposerName { get; set; }
    
    [JsonPropertyName("playParams")]
    public PlayParams? PlayParams { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonPropertyName("discNumber")]
    public int DiscNumber { get; set; }
    
    [JsonPropertyName("hasLyrics")]
    public bool HasLyrics { get; set; }
    
    [JsonPropertyName("isAppleDigitalMaster")]
    public bool IsAppleDigitalMaster { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("previews")]
    public List<SongPreview> Previews { get; set; } = new();
    
    [JsonPropertyName("artistName")]
    public string ArtistName { get; set; } = string.Empty;
}

/// <summary>
/// Represents a song preview.
/// </summary>
public class SongPreview
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Represents song relationships.
/// </summary>
public class SongRelationships
{
    [JsonPropertyName("artists")]
    public SongArtistRelationship? Artists { get; set; }
    
    [JsonPropertyName("music-videos")]
    public MusicVideoRelationship? MusicVideos { get; set; }
}

/// <summary>
/// Represents song artist relationship.
/// </summary>
public class SongArtistRelationship
{
    [JsonPropertyName("href")]
    public string? Href { get; set; }
    
    [JsonPropertyName("data")]
    public List<SongArtist>? Data { get; set; }
}

/// <summary>
/// Represents a song artist.
/// </summary>
public class SongArtist
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;
}

/// <summary>
/// Represents music video relationship.
/// </summary>
public class MusicVideoRelationship
{
    [JsonPropertyName("href")]
    public string? Href { get; set; }
    
    [JsonPropertyName("data")]
    public List<object>? Data { get; set; }
}

/// <summary>
/// Represents a song in a playlist.
/// </summary>
public class PlaylistSong
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;
    
    [JsonPropertyName("attributes")]
    public SongAttributes? Attributes { get; set; }
    
    [JsonPropertyName("relationships")]
    public SongRelationships? Relationships { get; set; }
}

/// <summary>
/// Represents a playlist.
/// </summary>
public class Playlist
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;
    
    [JsonPropertyName("attributes")]
    public PlaylistAttributes? Attributes { get; set; }
}

/// <summary>
/// Represents playlist tracks response.
/// </summary>
public class PlaylistTracksResponse
{
    [JsonPropertyName("data")]
    public List<PlaylistSong> Data { get; set; } = new();
    
    [JsonPropertyName("next")]
    public string? Next { get; set; }
}
