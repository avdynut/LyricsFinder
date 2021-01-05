using nucs.JsonSettings;
using System.Windows;

namespace Lyrixound.Configuration
{
    public class WindowSettings : JsonSettings
    {
        public override string FileName { get; set; }

        public virtual bool DisplayInTaskbar { get; set; }

        public virtual bool Topmost { get; set; } = true;

        public virtual WindowState WindowState { get; set; } = WindowState.Normal;

        public virtual Size WindowSize { get; set; } = new Size(420, 450);

        public virtual Point? WindowPosition { get; set; }

        public virtual double FloatingBackgroundOpacity { get; set; } = 0.2;
    }
}
