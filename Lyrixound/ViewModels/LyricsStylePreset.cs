using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Lyrixound.ViewModels;

public sealed class LyricsStylePreset : INotifyPropertyChanged
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
        double backgroundOpacity,
        bool useThemeTextColor = false)
    {
        Name = name;
        FontFamily = new FontFamily(fontFamily);
        FontSize = fontSize;
        IsBold = bold;
        IsItalic = italic;
        TextAlignment = textAlignment;
        _useThemeTextColor = useThemeTextColor;
        TextColor = ParseColor(textColor);
        ShadowColor = ParseColor(shadowColor);
        ShadowDepth = shadowDepth;
        BlurRadius = blurRadius;
        ActiveLineBackgroundColor = ParseColor(activeLineBackgroundColor);
        BackgroundOpacity = backgroundOpacity;
        TextBrush = Freeze(TextColor);
    }

    public string Name { get; }
    public FontFamily FontFamily { get; }
    public double FontSize { get; }
    public bool IsBold { get; }
    public bool IsItalic { get; }
    public FontWeight FontWeight => IsBold ? FontWeights.Bold : FontWeights.Normal;
    public FontStyle FontStyle => IsItalic ? FontStyles.Italic : FontStyles.Normal;
    public TextAlignment TextAlignment { get; }
    public Color TextColor { get; private set; }
    public SolidColorBrush TextBrush { get; private set; }

    private readonly bool _useThemeTextColor;
    public Color ShadowColor { get; }
    public double ShadowDepth { get; }
    public double BlurRadius { get; }
    public Color ActiveLineBackgroundColor { get; }
    public double BackgroundOpacity { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;

            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public bool Matches(LyricsSettingsViewModel settings) =>
        string.Equals(FontFamily.Source, settings.FontFamily?.Source, System.StringComparison.OrdinalIgnoreCase)
        && FontSize == settings.FontSize
        && IsBold == settings.IsBold
        && IsItalic == settings.IsItalic
        && TextAlignment == settings.TextAlignment
        && TextColor == settings.TextColor
        && ShadowColor == settings.ShadowColor
        && ShadowDepth == settings.ShadowDepth
        && BlurRadius == settings.BlurRadius
        && ActiveLineBackgroundColor == settings.ActiveLineBackgroundColor
        && BackgroundOpacity == settings.FloatingBackgroundOpacity;

    public void ApplyTheme()
    {
        if (!_useThemeTextColor || Application.Current?.TryFindResource("MaterialDesignBody") is not SolidColorBrush body)
            return;

        var color = body.Color;
        if (TextColor == color)
            return;

        TextColor = color;
        TextBrush = Freeze(color);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextColor)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextBrush)));
    }

    private static SolidColorBrush Freeze(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static Color ParseColor(string value) => (Color)ColorConverter.ConvertFromString(value);
}
