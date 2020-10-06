using nucs.JsonSettings;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace Lyrixator.Configuration
{
    public class LyricsSettings : JsonSettings
    {
        public override string FileName { get; set; } = Path.Combine(Settings.ConfigurationDirectory, "lyrics.json");

        public virtual double FontSize { get; set; } = 20;
        public virtual FontFamily FontFamily { get; set; } = new FontFamily("Segoe UI");
        public virtual FontStyle FontStyle { get; set; } = FontStyles.Normal;
        public virtual FontWeight FontWeight { get; set; } = FontWeights.Normal;
        public virtual HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Center;

        public virtual Color TextColor { get; set; } = (Color)ColorConverter.ConvertFromString("#FFD6EDFF");
        public virtual Color ShadowColor { get; set; } = Colors.Black;

        public virtual double BlurRadius { get; set; } = 15;
        public virtual double ShadowDepth { get; set; }
    }
}
