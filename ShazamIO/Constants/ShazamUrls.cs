namespace ShazamIO.Constants;

/// <summary>
/// Contains all Shazam API endpoint URLs.
/// </summary>
public static class ShazamUrls
{
    public const string SearchFromFile = 
        "https://amp.shazam.com/discovery/v5/{0}/{1}/{2}/-/tag/{3}/{4}?sync=true&webv3=true&sampling=true&connected=&shazamapiversion=v3&sharehub=true&hubv5minorversion=v5.1&hidelb=true&video=v3";
    
    public const string AboutTrack = 
        "https://www.shazam.com/discovery/v5/{0}/{1}/web/-/track/{2}?shazamapiversion=v3&video=v3";
    
    public const string TopTracksPlaylist = 
        "https://www.shazam.com/services/amapi/v1/catalog/{0}/playlists/{1}/tracks?limit={2}&offset={3}&l={4}&relate[songs]=artists,music-videos";
    
    public const string Locations = 
        "https://www.shazam.com/services/charts/locations";
    
    public const string RelatedSongs = 
        "https://cdn.shazam.com/shazam/v3/{0}/{1}/web/-/tracks/track-similarities-id-{2}?startFrom={3}&pageSize={4}&connected=&channel=";
    
    public const string SearchArtist = 
        "https://www.shazam.com/services/search/v4/{0}/{1}/web/search?term={2}&limit={3}&offset={4}&types=artists";
    
    public const string SearchMusic = 
        "https://www.shazam.com/services/search/v3/{0}/{1}/web/search?query={2}&numResults={3}&offset={4}&types=songs";
    
    public const string ListeningCounter = 
        "https://www.shazam.com/services/count/v2/web/track/{0}";
    
    public const string ListeningCounterMany = 
        "https://www.shazam.com/services/count/v2/web/track";
    
    public const string SearchArtistV2 = 
        "https://www.shazam.com/services/amapi/v1/catalog/{0}/artists/{1}";
    
    public const string ArtistAlbums = 
        "https://www.shazam.com/services/amapi/v1/catalog/{0}/artists/{1}/albums?limit={2}&offset={3}";
    
    public const string ArtistAlbumInfo = 
        "https://www.shazam.com/services/amapi/v1/catalog/{0}/albums/{1}";
}
