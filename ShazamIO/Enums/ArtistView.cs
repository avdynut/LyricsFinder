namespace ShazamIO.Enums;

/// <summary>
/// Artist view options for API queries.
/// </summary>
public enum ArtistView
{
    FullAlbums,
    FeaturedAlbums,
    LatestRelease,
    TopMusicVideos,
    SimilarArtists,
    TopSongs,
    Playlists
}

public static class ArtistViewExtensions
{
    public static string ToValue(this ArtistView view) => view switch
    {
        ArtistView.FullAlbums => "full-albums",
        ArtistView.FeaturedAlbums => "featured-albums",
        ArtistView.LatestRelease => "latest-release",
        ArtistView.TopMusicVideos => "top-music-videos",
        ArtistView.SimilarArtists => "similar-artists",
        ArtistView.TopSongs => "top-songs",
        ArtistView.Playlists => "playlists",
        _ => throw new ArgumentOutOfRangeException(nameof(view))
    };
}
