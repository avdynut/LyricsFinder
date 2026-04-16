namespace Lyrixound.Services
{
    /// <summary>
    /// Detailed information about a recognized track from Shazam.
    /// </summary>
    public class RecognizedTrackInfo
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public string Label { get; set; }
        public string ReleaseDate { get; set; }
        public string CoverArtUrl { get; set; }
        public string ShazamUrl { get; set; }
        public string AppleMusicUrl { get; set; }
        public string SpotifyUrl { get; set; }
        public string YouTubeUrl { get; set; }
        public string Isrc { get; set; }
        public string Key { get; set; }
        public int? Bpm { get; set; }

        public bool IsRecognized => !string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Artist);
    }
}
