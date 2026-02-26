using Newtonsoft.Json;
using nucs.JsonSettings;
using System.Windows;
using System.Windows.Media;

namespace Lyrixound.Configuration
{
    public class LyricsSettings : JsonSettings
    {
        public override string FileName { get; set; }

        public virtual double FontSize { get; set; } = 25;
        public virtual bool IsItalic { get; set; }
        public virtual bool IsBold { get; set; } = true;
        public virtual FontFamily FontFamily { get; set; } = new FontFamily("Segoe UI");
        public virtual TextAlignment TextAlignment { get; set; } = TextAlignment.Center;

        public virtual Color TextColor { get; set; } = (Color)ColorConverter.ConvertFromString("#FFE6E6FF");
        public virtual Color ShadowColor { get; set; } = Colors.Black;
        public virtual Color ActiveLineBackgroundColor { get; set; } = (Color)ColorConverter.ConvertFromString("#FF9C27B0");

        public virtual double ShadowDepth { get; set; } = 1;
        public virtual double BlurRadius { get; set; } = 5;

        public virtual double TimeOffsetSeconds { get; set; } = 0.5;

        // for backward compatibility with old settings files
        [JsonProperty(nameof(TimeOffsetMilliseconds))]
        private double TimeOffsetMilliseconds { set => TimeOffsetSeconds = value / 1000.0; }

        public virtual double FloatingBackgroundOpacity { get; set; } = 0;
    }
}
