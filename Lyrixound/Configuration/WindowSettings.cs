using Nucs.JsonSettings;
using Nucs.JsonSettings.Autosave;
using System.Windows;

namespace Lyrixound.Configuration
{
    [Autosave]
    public class WindowSettings : JsonSettings
    {
        public override string FileName { get; set; }

        public virtual bool DisplayInTaskbar { get; set; }

        public virtual bool IsClickThrough { get; set; }

        public virtual bool Topmost { get; set; } = true;

        public virtual WindowState WindowState { get; set; } = WindowState.Normal;

        public virtual Size WindowSize { get; set; } = new Size(420, 450);

        public virtual Point? WindowPosition { get; set; }
    }
}
