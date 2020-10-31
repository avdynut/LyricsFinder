using System.Collections.Generic;

namespace PlayerWatching.Localization
{
    /// <summary>
    /// Localization values for SMTC window.
    /// </summary>
    public class SmtcLocalization
    {
        public string TitlePrecedingText { get; }
        public string ArtistPrecedingText { get; }
        public string PlayButtonPlayingText { get; }

        public static Dictionary<string, SmtcLocalization> Localizations { get; } = new Dictionary<string, SmtcLocalization>
        {
            { "en", new SmtcLocalization("Track name ", "Track details ", "Pause track") },
            { "ru", new SmtcLocalization("Название дорожки ", "Сведения о дорожке ", "Приостановить") }
        };

        public SmtcLocalization(string titlePrecedingText, string artistPrecedingText, string playButtonPlayingText)
        {
            TitlePrecedingText = titlePrecedingText;
            ArtistPrecedingText = artistPrecedingText;
            PlayButtonPlayingText = playButtonPlayingText;
        }
    }
}
