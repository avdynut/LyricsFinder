using System.Windows;
using System.Windows.Media;

namespace Lyrixound.ViewModels;

public sealed class LyricsStylePreset
{
    public LyricsStylePreset(
        string name,
        string fontFamily,
        double fontSize,
        bool bold,
        bool italic,
        TextAlignment textAlignment,
        string textColor,
        string shadowColor,
        double shadowDepth,
        double blurRadius,
        string activeLineBackgroundColor,
        double backgroundOpacity)
    {
        Name = name;
        FontFamily = new FontFamily(fontFamily);
        FontSize = fontSize;
        IsBold = bold;
        IsItalic = italic;
        TextAlignment = textAlignment;
        TextColor = ParseColor(textColor);
        ShadowColor = ParseColor(shadowColor);
        ShadowDepth = shadowDepth;
        BlurRadius = blurRadius;
        ActiveLineBackgroundColor = ParseColor(activeLineBackgroundColor);
        BackgroundOpacity = backgroundOpacity;
        TextBrush = new SolidColorBrush(TextColor);
        TextBrush.Freeze();
    }

    public string Name { get; }
    public FontFamily FontFamily { get; }
    public double FontSize { get; }
    public bool IsBold { get; }
    public bool IsItalic { get; }
    public FontWeight FontWeight => IsBold ? FontWeights.Bold : FontWeights.Normal;
    public FontStyle FontStyle => IsItalic ? FontStyles.Italic : FontStyles.Normal;
    public TextAlignment TextAlignment { get; }
    public Color TextColor { get; }
    public SolidColorBrush TextBrush { get; }
    public Color ShadowColor { get; }
    public double ShadowDepth { get; }
    public double BlurRadius { get; }
    public Color ActiveLineBackgroundColor { get; }
    public double BackgroundOpacity { get; }

    private static Color ParseColor(string value) => (Color)ColorConverter.ConvertFromString(value);
}
