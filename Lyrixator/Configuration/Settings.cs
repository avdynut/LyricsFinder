using nucs.JsonSettings;
using System.IO;

namespace Lyrixator.Configuration
{
    public class Settings : JsonSettings
    {
        public const string ConfigurationDirectory = "Configuration";

        public override string FileName { get; set; } = Path.Combine(ConfigurationDirectory, "app.json");
    }
}
