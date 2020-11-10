using nucs.JsonSettings;
using System.Collections.Generic;
using System.Linq;

namespace LyricsProviders.DirectoriesProvider
{
    public class DirectoriesProviderSettings : JsonSettings
    {
        public override string FileName { get; set; }

        public virtual List<string> LyricsDirectories { get; set; } = new List<string> { "lyrics" };

        public virtual string LyricsFileNamePattern { get; set; } = DirectoriesTrackInfoProvider.DefaultFileNameMask;

        public DirectoriesProviderSettings()
        {
            AfterLoad += OnAfterLoad;
        }

        private void OnAfterLoad()
        {
            LyricsDirectories = LyricsDirectories.Distinct().ToList(); // JsonSettings creates double values of the default element
        }
    }
}
