using LyricsProviders.DirectoriesProvider;
using LyricsProviders.GoogleProvider;
using nucs.JsonSettings;
using PlayerWatching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lyrixator.Configuration
{
    public class Settings : JsonSettings
    {
        public const string ConfigurationDirectory = "settings";

        public override string FileName { get; set; } = Path.Combine(ConfigurationDirectory, "app.json");

        public virtual TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(1);

        public virtual List<Element> PlayerWatchers { get; set; } = new List<Element>
            {
                new Element(SmtcWatcher.Name, isEnabled: true),
                new Element(YandexMusicWatcher.Name, isEnabled: false)
            };

        public virtual List<Element> LyricsProviders { get; set; } = new List<Element>
            {
                new Element(DirectoriesTrackInfoProvider.Name, isEnabled: true),
                new Element(GoogleTrackInfoProvider.Name, isEnabled: true)
            };

        public Settings()
        {
            AfterLoad += OnAfterLoad;
        }

        private void OnAfterLoad()
        {
            // JsonSettings creates double values of the default list elements
            PlayerWatchers = PlayerWatchers.Distinct().ToList();
            LyricsProviders = LyricsProviders.Distinct().ToList();
        }
    }
}
