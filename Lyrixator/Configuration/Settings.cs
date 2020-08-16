using nucs.JsonSettings;
using System;
using System.IO;

namespace Lyrixator.Configuration
{
    public class Settings : JsonSettings
    {
        public const string ConfigurationDirectory = "settings";

        public override string FileName { get; set; } = Path.Combine(ConfigurationDirectory, "app.json");

        public virtual TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(1);

        public virtual string LyricsDirectory { get; set; } = "lyrics";
    }
}
