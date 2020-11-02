using LyricsProviders.DirectoriesProvider;
using LyricsProviders.GoogleProvider;
using nucs.JsonSettings;
using PlayerWatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Lyrixound.Configuration
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

        public virtual ObservableCollection<Element> PlayerWatchers { get; set; } = new ObservableCollection<Element>();

        public virtual ObservableCollection<Element> LyricsProviders { get; set; } = new ObservableCollection<Element>();

        public Settings()
        {
            AfterLoad += OnAfterLoad;
        }

        private void OnAfterLoad()
        {
            PlayerWatchers = new ObservableCollection<Element>(PlayerWatchers.Union(_defaultWatchers));
            LyricsProviders = new ObservableCollection<Element>(LyricsProviders.Union(_defaultProviders));
        }
    }
}
