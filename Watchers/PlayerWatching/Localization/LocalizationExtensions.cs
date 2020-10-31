using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PlayerWatching.Localization
{
    public static class LocalizationExtensions
    {
        public static T GetCurrentLocalization<T>(this IDictionary<string, T> localization)
        {
            var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            return localization.ContainsKey(language) ? localization[language] : localization.Values.First();
        }
    }
}