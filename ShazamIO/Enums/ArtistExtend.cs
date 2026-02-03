namespace ShazamIO.Enums;

/// <summary>
/// Artist extend options for additional data in API queries.
/// </summary>
public enum ArtistExtend
{
    ArtistBio,
    BornOrFormed,
    EditorialArtwork,
    Origin
}

public static class ArtistExtendExtensions
{
    public static string ToValue(this ArtistExtend extend) => extend switch
    {
        ArtistExtend.ArtistBio => "artistBio",
        ArtistExtend.BornOrFormed => "bornOrFormed",
        ArtistExtend.EditorialArtwork => "editorialArtwork",
        ArtistExtend.Origin => "origin",
        _ => throw new ArgumentOutOfRangeException(nameof(extend))
    };
}
