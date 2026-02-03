namespace ShazamIO.Enums;

/// <summary>
/// Music genre categories available on Shazam charts.
/// </summary>
public enum GenreMusic
{
    Pop,
    HipHopRap,
    Dance,
    Electronic,
    RnbSoul,
    Alternative,
    Rock,
    Latin,
    FilmTvStage,
    Country,
    AfroBeats,
    Worldwide,
    ReggaeDanceHall,
    House,
    KPop,
    FrenchPop,
    SingerSongwriter,
    RegionalMexicano
}

public static class GenreMusicExtensions
{
    public static string ToUrlName(this GenreMusic genre) => genre switch
    {
        GenreMusic.Pop => "pop",
        GenreMusic.HipHopRap => "hip-hop-rap",
        GenreMusic.Dance => "dance",
        GenreMusic.Electronic => "electronic",
        GenreMusic.RnbSoul => "randb-soul",
        GenreMusic.Alternative => "alternative",
        GenreMusic.Rock => "rock",
        GenreMusic.Latin => "latin",
        GenreMusic.FilmTvStage => "film-tv-and-stage",
        GenreMusic.Country => "country",
        GenreMusic.AfroBeats => "afrobeats",
        GenreMusic.Worldwide => "worldwide",
        GenreMusic.ReggaeDanceHall => "reggae-dancehall",
        GenreMusic.House => "house",
        GenreMusic.KPop => "k-pop",
        GenreMusic.FrenchPop => "french-pop",
        GenreMusic.SingerSongwriter => "singer-songwriter",
        GenreMusic.RegionalMexicano => "regional-mexicano",
        _ => throw new ArgumentOutOfRangeException(nameof(genre))
    };

    public static GenreMusic FromUrlName(string urlName) => urlName switch
    {
        "pop" => GenreMusic.Pop,
        "hip-hop-rap" => GenreMusic.HipHopRap,
        "dance" => GenreMusic.Dance,
        "electronic" => GenreMusic.Electronic,
        "randb-soul" => GenreMusic.RnbSoul,
        "alternative" => GenreMusic.Alternative,
        "rock" => GenreMusic.Rock,
        "latin" => GenreMusic.Latin,
        "film-tv-and-stage" => GenreMusic.FilmTvStage,
        "country" => GenreMusic.Country,
        "afrobeats" => GenreMusic.AfroBeats,
        "worldwide" => GenreMusic.Worldwide,
        "reggae-dancehall" => GenreMusic.ReggaeDanceHall,
        "house" => GenreMusic.House,
        "k-pop" => GenreMusic.KPop,
        "french-pop" => GenreMusic.FrenchPop,
        "singer-songwriter" => GenreMusic.SingerSongwriter,
        "regional-mexicano" => GenreMusic.RegionalMexicano,
        _ => throw new ArgumentException($"Unknown genre URL name: {urlName}", nameof(urlName))
    };
}
