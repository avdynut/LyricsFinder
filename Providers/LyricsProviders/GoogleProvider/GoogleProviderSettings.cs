using nucs.JsonSettings;

namespace LyricsProviders.GoogleProvider
{
    public class GoogleProviderSettings : JsonSettings
    {
        public override string FileName { get; set; }

        public string SearchUrl { get; set; } = "https://www.google.com/search?q=";
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.75 Safari/537.36";
        public string AcceptLanguages { get; set; } = "ru,en";
        public string LyricsVersesClassName { get; set; } = "ujudUb";
    }
}
