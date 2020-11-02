using nucs.JsonSettings;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace Lyrixound.Configuration
{
    public class LyricsSettings : JsonSettings
    {
        public override string FileName { get; set; } = Path.Combine(Settings.ConfigurationDirectory, "lyrics.json");

        public virtual double FontSize { get; set; } = 25;
        public virtual bool IsItalic { get; set; }
        public virtual bool IsBold { get; set; } = true;
        public virtual FontFamily FontFamily { get; set; } = new FontFamily("Segoe UI");
        public virtual TextAlignment TextAlignment { get; set; } = TextAlignment.Center;

        public virtual Color TextColor { get; set; } = (Color)ColorConverter.ConvertFromString("#FFE6E6FF");
        public virtual Color ShadowColor { get; set; } = Colors.Black;

        public virtual double ShadowDepth { get; set; } = 1;
        public virtual double BlurRadius { get; set; } = 5;
    }
}
