using System.Collections.Generic;

namespace PlayerWatching.Localization
{
    public class YandexMusicLocalization
    {
        public string WindowTitle { get; }

        public static Dictionary<string, YandexMusicLocalization> Localizations { get; } = new Dictionary<string, YandexMusicLocalization>
        {
            { "en", new YandexMusicLocalization("Yandex.Music") },
            { "ru", new YandexMusicLocalization("Яндекс.Музыка") }
        };

        public YandexMusicLocalization(string windowTitle)
        {
            WindowTitle = windowTitle;
        }
    }
}
