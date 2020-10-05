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
        private readonly List<Element> _defaultWatchers = new List<Element>
            {
                new Element(SmtcWatcher.Name, isEnabled: true),
                new Element(YandexMusicWatcher.Name, isEnabled: false)
            };

        private readonly List<Element> _defaultProviders = new List<Element>
            {
                new Element(DirectoriesTrackInfoProvider.Name, isEnabled: true),
                new Element(GoogleTrackInfoProvider.Name, isEnabled: true)
            };

        public const string ConfigurationDirectory = "settings";

        public override string FileName { get; set; } = Path.Combine(ConfigurationDirectory, "app.json");

        public virtual TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(3);

        public virtual List<Element> PlayerWatchers { get; set; } = new List<Element>();

        public virtual List<Element> LyricsProviders { get; set; } = new List<Element>();

        public Settings()
        {
            AfterLoad += OnAfterLoad;
        }

        private void OnAfterLoad()
        {
            PlayerWatchers = PlayerWatchers.Union(_defaultWatchers).ToList();
            LyricsProviders = LyricsProviders.Union(_defaultProviders).ToList();
        }
    }
}
