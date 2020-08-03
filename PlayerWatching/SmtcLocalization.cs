using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PlayerWatching
{
    /// <summary>
    /// Localization values for SMTC window.
    /// </summary>
    public class SmtcLocalization
    {
        public string TitlePrecedingText { get; set; }
        public string ArtistPrecedingText { get; set; }
        public string PlayButtonPlayingText { get; set; }

        public SmtcLocalization(string titlePrecedingText, string artistPrecedingText, string playButtonPlayingText)
        {
            TitlePrecedingText = titlePrecedingText;
            ArtistPrecedingText = artistPrecedingText;
            PlayButtonPlayingText = playButtonPlayingText;
        }

        private static readonly Dictionary<string, SmtcLocalization> _localization = new Dictionary<string, SmtcLocalization>
            {
                { "en", new SmtcLocalization("Track name ", "Track details ", "Pause track") },
                { "ru", new SmtcLocalization("Название дорожки ", "Сведения о дорожке ", "Приостановить") }
            };

        public static SmtcLocalization GetLocalization()
        {
            var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            return _localization.ContainsKey(language) ? _localization[language] : _localization.Values.First();
        }
    }
}
