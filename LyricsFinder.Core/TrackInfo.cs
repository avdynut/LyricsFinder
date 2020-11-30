namespace LyricsFinder.Core
{
    public class TrackInfo
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }

        public override string ToString() => $"{Artist} - {Title}";
    }
}
