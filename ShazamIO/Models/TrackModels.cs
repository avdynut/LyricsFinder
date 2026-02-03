using System.Text.Json.Serialization;

namespace ShazamIO.Models;

/// <summary>
/// Represents a share model with various sharing options.
/// </summary>
public class ShareModel
{
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;
    
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;
    
    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;
    
    [JsonPropertyName("twitter")]
    public string Twitter { get; set; } = string.Empty;
    
    [JsonPropertyName("html")]
    public string Html { get; set; } = string.Empty;
    
    [JsonPropertyName("snapchat")]
    public string Snapchat { get; set; } = string.Empty;
}

/// <summary>
/// Represents an action model with URI and share information.
/// </summary>
public class ActionModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("share")]
    public ShareModel? Share { get; set; }
    
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }
}

/// <summary>
/// Represents song metadata pages.
/// </summary>
public class SongMetaPages
{
    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;
    
    [JsonPropertyName("caption")]
    public string Caption { get; set; } = string.Empty;
}

/// <summary>
/// Represents song metadata.
/// </summary>
public class SongMetadata
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Represents a base ID and type model.
/// </summary>
public class BaseIdTypeModel
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Represents top tracks information.
/// </summary>
public class TopTracksModel
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Represents a song section.
/// </summary>
public class SongSection
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("metapages")]
    public List<SongMetaPages> MetaPages { get; set; } = new();
    
    [JsonPropertyName("tabname")]
    public string TabName { get; set; } = string.Empty;
    
    [JsonPropertyName("metadata")]
    public List<SongMetadata> Metadata { get; set; } = new();
}

/// <summary>
/// Represents an artist section.
/// </summary>
public class ArtistSection
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("verified")]
    public bool Verified { get; set; }
    
    [JsonPropertyName("actions")]
    public List<BaseIdTypeModel> Actions { get; set; } = new();
    
    [JsonPropertyName("tabname")]
    public string TabName { get; set; } = string.Empty;
    
    [JsonPropertyName("toptracks")]
    public TopTracksModel? TopTracks { get; set; }
}

/// <summary>
/// Represents beacon data for lyrics section.
/// </summary>
public class BeaconDataLyricsSection
{
    [JsonPropertyName("lyricsid")]
    public string? LyricsId { get; set; }
    
    [JsonPropertyName("providername")]
    public string? ProviderName { get; set; }
    
    [JsonPropertyName("commontrackid")]
    public string? CommonTrackId { get; set; }
}

/// <summary>
/// Represents a lyrics section.
/// </summary>
public class LyricsSection
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("text")]
    public List<string> Text { get; set; } = new();
    
    [JsonPropertyName("footer")]
    public string Footer { get; set; } = string.Empty;
    
    [JsonPropertyName("tabname")]
    public string TabName { get; set; } = string.Empty;
    
    [JsonPropertyName("beacondata")]
    public BeaconDataLyricsSection? BeaconData { get; set; }
}

/// <summary>
/// Represents a video section.
/// </summary>
public class VideoSection
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "VIDEO";
    
    [JsonPropertyName("tabname")]
    public string TabName { get; set; } = string.Empty;
    
    [JsonPropertyName("youtubeurl")]
    public string? YoutubeUrl { get; set; }
}

/// <summary>
/// Represents a related section.
/// </summary>
public class RelatedSection
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonPropertyName("tabname")]
    public string TabName { get; set; } = string.Empty;
}

/// <summary>
/// Represents dimensions model.
/// </summary>
public class DimensionsModel
{
    [JsonPropertyName("width")]
    public int Width { get; set; }
    
    [JsonPropertyName("height")]
    public int Height { get; set; }
}

/// <summary>
/// Represents a YouTube image model.
/// </summary>
public class YoutubeImageModel
{
    [JsonPropertyName("dimensions")]
    public DimensionsModel? Dimensions { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Represents a match model from Shazam recognition.
/// </summary>
public class MatchModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("offset")]
    public double Offset { get; set; }
    
    [JsonPropertyName("timeskew")]
    public double TimeSkew { get; set; }
    
    [JsonPropertyName("frequencyskew")]
    public double FrequencySkew { get; set; }
    
    [JsonPropertyName("channel")]
    public string? Channel { get; set; }
}

/// <summary>
/// Represents location model.
/// </summary>
public class LocationModel
{
    [JsonPropertyName("accuracy")]
    public double Accuracy { get; set; }
}

/// <summary>
/// Represents YouTube data.
/// </summary>
public class YoutubeData
{
    [JsonPropertyName("caption")]
    public string Caption { get; set; } = string.Empty;
    
    [JsonPropertyName("image")]
    public YoutubeImageModel? Image { get; set; }
    
    [JsonPropertyName("actions")]
    public List<ActionModel> Actions { get; set; } = new();
    
    public string? Uri => Actions.FirstOrDefault(a => !string.IsNullOrEmpty(a.Uri))?.Uri;
}

/// <summary>
/// Represents track information.
/// </summary>
public class TrackInfo
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = string.Empty;
    
    [JsonPropertyName("artistid")]
    public string? ArtistId { get; set; }
    
    [JsonPropertyName("images")]
    public TrackImages? Images { get; set; }
    
    [JsonPropertyName("share")]
    public ShareModel? Share { get; set; }
    
    [JsonPropertyName("hub")]
    public TrackHub? Hub { get; set; }
    
    [JsonPropertyName("sections")]
    public List<object>? Sections { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    
    public string ShazamUrl => $"https://www.shazam.com/track/{Key}";
    
    public string? PhotoUrl => Images?.CoverArt;
    
    public string? AppleMusicUrl => Hub?.Options?.FirstOrDefault(o => 
        o.Actions?.Any(a => a.Type == "applemusicopen") == true)?.Actions?
        .FirstOrDefault(a => a.Type == "applemusicopen")?.Uri;
    
    public string? SpotifyUrl => Hub?.Providers?.FirstOrDefault(p => 
        p.Type?.ToLower() == "spotify")?.Actions?.FirstOrDefault()?.Uri;
}

/// <summary>
/// Represents track images.
/// </summary>
public class TrackImages
{
    [JsonPropertyName("background")]
    public string? Background { get; set; }
    
    [JsonPropertyName("coverart")]
    public string? CoverArt { get; set; }
    
    [JsonPropertyName("coverarthq")]
    public string? CoverArtHq { get; set; }
    
    [JsonPropertyName("joecolor")]
    public string? JoeColor { get; set; }
}

/// <summary>
/// Represents track hub (external links).
/// </summary>
public class TrackHub
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("image")]
    public string? Image { get; set; }
    
    [JsonPropertyName("actions")]
    public List<HubAction>? Actions { get; set; }
    
    [JsonPropertyName("options")]
    public List<HubOption>? Options { get; set; }
    
    [JsonPropertyName("providers")]
    public List<HubProvider>? Providers { get; set; }
}

/// <summary>
/// Represents a hub action.
/// </summary>
public class HubAction
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }
}

/// <summary>
/// Represents a hub option.
/// </summary>
public class HubOption
{
    [JsonPropertyName("caption")]
    public string? Caption { get; set; }
    
    [JsonPropertyName("actions")]
    public List<HubAction>? Actions { get; set; }
    
    [JsonPropertyName("beacondata")]
    public object? BeaconData { get; set; }
    
    [JsonPropertyName("image")]
    public string? Image { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("listcaption")]
    public string? ListCaption { get; set; }
    
    [JsonPropertyName("overflowimage")]
    public string? OverflowImage { get; set; }
    
    [JsonPropertyName("colouroverflowimage")]
    public bool? ColourOverflowImage { get; set; }
    
    [JsonPropertyName("providername")]
    public string? ProviderName { get; set; }
}

/// <summary>
/// Represents a hub provider.
/// </summary>
public class HubProvider
{
    [JsonPropertyName("caption")]
    public string? Caption { get; set; }
    
    [JsonPropertyName("images")]
    public ProviderImages? Images { get; set; }
    
    [JsonPropertyName("actions")]
    public List<HubAction>? Actions { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

/// <summary>
/// Represents provider images.
/// </summary>
public class ProviderImages
{
    [JsonPropertyName("overflow")]
    public string? Overflow { get; set; }
    
    [JsonPropertyName("default")]
    public string? Default { get; set; }
}

/// <summary>
/// Represents the full response from track recognition.
/// </summary>
public class ResponseTrack
{
    [JsonPropertyName("tagid")]
    public string? TagId { get; set; }
    
    [JsonPropertyName("retryms")]
    public int? RetryMs { get; set; }
    
    [JsonPropertyName("location")]
    public LocationModel? Location { get; set; }
    
    [JsonPropertyName("matches")]
    public List<MatchModel> Matches { get; set; } = new();
    
    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
    
    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }
    
    [JsonPropertyName("track")]
    public TrackInfo? Track { get; set; }
}
