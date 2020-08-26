using nucs.JsonSettings;
using PlayerWatching;
using System;
using System.Collections.Generic;
using System.IO;

namespace Lyrixator.Configuration
{
    public class Settings : JsonSettings
    {
        public const string ConfigurationDirectory = "settings";

        public override string FileName { get; set; } = Path.Combine(ConfigurationDirectory, "app.json");

        public virtual TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(1);

        public virtual Dictionary<string, bool> PlayerWatchers { get; set; } = new Dictionary<string, bool>
            {
                { SmtcWatcher.Name, true },
                { YandexMusicWatcher.Name, false }
            };
    }
}
