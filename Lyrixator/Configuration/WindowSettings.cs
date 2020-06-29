using nucs.JsonSettings;
using System.IO;
using System.Windows;

namespace Lyrixator.Configuration
{
    public class WindowSettings : JsonSettings
    {
        public override string FileName { get; set; } = Path.Combine(Settings.ConfigurationDirectory, "window.json");

        public virtual bool DisplayInTaskbar { get; set; }

        public virtual bool Topmost { get; set; } = true;

        public virtual WindowState WindowState { get; set; } = WindowState.Normal;

        public virtual Size WindowSize { get; set; } = new Size(420, 450);

        public virtual Point? WindowPosition { get; set; }
    }
}
