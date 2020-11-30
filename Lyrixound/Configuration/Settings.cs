using LyricsProviders.DirectoriesProvider;
using LyricsProviders.GoogleProvider;
using nucs.JsonSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lyrixound.Configuration
{
    public class Settings : JsonSettings
    {
        private readonly List<Element> _defaultProviders = new List<Element>
            {
                new Element(DirectoriesTrackInfoProvider.Name, isEnabled: true),
                new Element(GoogleTrackInfoProvider.Name, isEnabled: true)
            };

        public override string FileName { get; set; }

        public virtual TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(3);

        public virtual ObservableCollection<Element> LyricsProviders { get; set; } = new ObservableCollection<Element>();

        public Settings()
        {
            AfterLoad += OnAfterLoad;
        }

        private void OnAfterLoad()
        {
            LyricsProviders = new ObservableCollection<Element>(LyricsProviders.Union(_defaultProviders));
        }
    }
}
