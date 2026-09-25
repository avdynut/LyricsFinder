using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Lyrixound.Views;

public partial class ColorEdit : UserControl
{
    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
        nameof(Color),
        typeof(Color),
        typeof(ColorEdit),
        new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));

    public static readonly DependencyProperty HintProperty = DependencyProperty.Register(
        nameof(Hint),
        typeof(string),
        typeof(ColorEdit),
        new PropertyMetadata("Color"));

    private const int PadWidth = 180;
    private const int PadHeight = 100;

    private double _hue;
    private double _saturation;
    private double _value = 1;
    private double _bitmapHue = double.NaN;
    private bool _suppressColorWrite;
    private bool _suppressHex;
    private bool _ignoreSwatchClick;

    public ColorEdit()
    {
        InitializeComponent();
        SyncFromColor(Color);
    }

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public string Hint
    {
        get => (string)GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ColorEdit)d;
        if (!control.IsInitialized || control._suppressColorWrite)
            return;

        control.SyncFromColor((Color)e.NewValue);
    }

    private void OnSwatchClick(object sender, RoutedEventArgs e)
    {
        if (_ignoreSwatchClick)
            return;

        PickerPopup.IsOpen = true;
    }

    private void OnPickerClosed(object sender, EventArgs e)
    {
        // The same click that dismisses the popup would otherwise reopen it.
        _ignoreSwatchClick = true;
        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => _ignoreSwatchClick = false));
    }

    private void OnPickerOpened(object sender, EventArgs e)
    {
        UpdatePadVisuals();
    }

    private void OnPresetClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Background is SolidColorBrush brush)
            Color = Color.FromArgb(Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
    }

    private void OnHexChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressHex || !TryParseHex(HexBox.Text, out var color))
            return;

        // #RGB and #ARGB are valid, but they are also prefixes of a longer hex value.
        var digits = HexDigitCount(HexBox.Text);
        var isPaste = false;
        foreach (var change in e.Changes)
        {
            if (change.AddedLength > 1)
            {
                isPaste = true;
                break;
            }
        }

        if (digits != 6 && digits != 8 && !isPaste)
            return;

        if (color != Color)
            Color = color;
    }

    private void OnHexLostFocus(object sender, RoutedEventArgs e)
    {
        if (TryParseHex(HexBox.Text, out var color) && color != Color)
            Color = color;

        ShowHex(Color);
    }

    private void OnSvDown(object sender, MouseButtonEventArgs e)
    {
        SvPad.CaptureMouse();
        UpdateSvFromPoint(e.GetPosition(SvPad));
    }

    private void OnSvMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && SvPad.IsMouseCaptured)
            UpdateSvFromPoint(e.GetPosition(SvPad));
    }

    private void OnSvUp(object sender, MouseButtonEventArgs e)
    {
        if (SvPad.IsMouseCaptured)
            SvPad.ReleaseMouseCapture();
    }

    private void OnHueDown(object sender, MouseButtonEventArgs e)
    {
        HueBar.CaptureMouse();
        UpdateHueFromPoint(e.GetPosition(HueBar));
    }

    private void OnHueMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && HueBar.IsMouseCaptured)
            UpdateHueFromPoint(e.GetPosition(HueBar));
    }

    private void OnHueUp(object sender, MouseButtonEventArgs e)
    {
        if (HueBar.IsMouseCaptured)
            HueBar.ReleaseMouseCapture();
    }

    private void OnAlphaDown(object sender, MouseButtonEventArgs e)
    {
        AlphaBar.CaptureMouse();
        UpdateAlphaFromPoint(e.GetPosition(AlphaBar));
    }

    private void OnAlphaMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && AlphaBar.IsMouseCaptured)
            UpdateAlphaFromPoint(e.GetPosition(AlphaBar));
    }

    private void OnAlphaUp(object sender, MouseButtonEventArgs e)
    {
        if (AlphaBar.IsMouseCaptured)
            AlphaBar.ReleaseMouseCapture();
    }

    private void OnPadSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdatePadVisuals();
    }

    private void SyncFromColor(Color color)
    {
        var (hue, saturation, value) = RgbToHsv(color);
        if (saturation > 0.0001)
            _hue = hue;

        _saturation = saturation;
        _value = value;
        UpdatePadVisuals();
        UpdateHexDisplay();
    }

    private void UpdateSvFromPoint(Point point)
    {
        if (SvPad.ActualWidth <= 0 || SvPad.ActualHeight <= 0)
            return;

        _saturation = Clamp(point.X / SvPad.ActualWidth);
        _value = Clamp(1 - point.Y / SvPad.ActualHeight);
        WritePickerColor();
    }

    private void UpdateHueFromPoint(Point point)
    {
        if (HueBar.ActualWidth <= 0)
            return;

        _hue = Clamp(point.X / HueBar.ActualWidth) * 360;
        WritePickerColor();
    }

    private void UpdateAlphaFromPoint(Point point)
    {
        if (AlphaBar.ActualWidth <= 0)
            return;

        var alpha = (byte)Math.Round(Clamp(point.X / AlphaBar.ActualWidth) * 255);
        var next = Color.FromArgb(alpha, Color.R, Color.G, Color.B);
        if (next == Color)
        {
            UpdatePadVisuals();
            return;
        }

        _suppressColorWrite = true;
        Color = next;
        _suppressColorWrite = false;
        UpdatePadVisuals();
        UpdateHexDisplay();
    }

    private void WritePickerColor()
    {
        var next = HsvToRgb(_hue, _saturation, _value, Color.A);
        UpdatePadVisuals();
        if (next == Color)
            return;

        _suppressColorWrite = true;
        Color = next;
        _suppressColorWrite = false;
        UpdateHexDisplay();
    }

    private void UpdateHexDisplay()
    {
        if (HexBox.IsFocused)
            return;

        ShowHex(Color);
    }

    private void ShowHex(Color color)
    {
        var text = FormatHex(color);
        if (HexBox.Text == text)
            return;

        _suppressHex = true;
        HexBox.Text = text;
        _suppressHex = false;
    }

    private void UpdatePadVisuals()
    {
        UpdateSvBitmap();

        if (SvPad.ActualWidth > 0 && SvPad.ActualHeight > 0)
        {
            Canvas.SetLeft(SvThumb, _saturation * SvPad.ActualWidth - SvThumb.Width / 2);
            Canvas.SetTop(SvThumb, (1 - _value) * SvPad.ActualHeight - SvThumb.Height / 2);
        }

            if (HueBar.ActualWidth > 0)
                Canvas.SetLeft(HueThumb, _hue / 360 * HueBar.ActualWidth - HueThumb.Width / 2);

            var opaque = HsvToRgb(_hue, _saturation, _value, 255);
            AlphaStart.Color = Color.FromArgb(0, opaque.R, opaque.G, opaque.B);
            AlphaEnd.Color = opaque;
            if (AlphaBar.ActualWidth > 0)
                Canvas.SetLeft(AlphaThumb, Color.A / 255d * AlphaBar.ActualWidth - AlphaThumb.Width / 2);
        }

    private void UpdateSvBitmap()
    {
        if (!double.IsNaN(_bitmapHue) && Math.Abs(_bitmapHue - _hue) < 0.05 && SvImage.Source != null)
            return;

        _bitmapHue = _hue;
        var pixels = new byte[PadWidth * PadHeight * 4];
        for (var y = 0; y < PadHeight; y++)
        {
            var value = 1 - y / (double)(PadHeight - 1);
            for (var x = 0; x < PadWidth; x++)
            {
                var saturation = x / (double)(PadWidth - 1);
                var color = HsvToRgb(_hue, saturation, value, 255);
                var i = (y * PadWidth + x) * 4;
                pixels[i] = color.B;
                pixels[i + 1] = color.G;
                pixels[i + 2] = color.R;
                pixels[i + 3] = 255;
            }
        }

        var bitmap = new WriteableBitmap(PadWidth, PadHeight, 96, 96, PixelFormats.Bgra32, null);
        bitmap.WritePixels(new Int32Rect(0, 0, PadWidth, PadHeight), pixels, PadWidth * 4, 0);
        bitmap.Freeze();
        SvImage.Source = bitmap;
    }

    private static int HexDigitCount(string text)
    {
        var hex = text.Trim();
        if (hex.StartsWith("#", StringComparison.Ordinal))
            hex = hex.Substring(1);

        return hex.Length;
    }

        private static string FormatHex(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

    private static bool TryParseHex(string text, out Color color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var hex = text.Trim();
        if (hex.StartsWith("#", StringComparison.Ordinal))
            hex = hex.Substring(1);

        if (hex.Length == 3 || hex.Length == 4)
        {
            var expanded = string.Empty;
            foreach (var ch in hex)
                expanded += new string(ch, 2);

            hex = expanded;
        }

        if (hex.Length == 6)
            hex = "FF" + hex;

        if (hex.Length != 8 || !uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
            return false;

        color = Color.FromArgb(
            (byte)((value >> 24) & 0xFF),
            (byte)((value >> 16) & 0xFF),
            (byte)((value >> 8) & 0xFF),
            (byte)(value & 0xFF));
        return true;
    }

    private static (double Hue, double Saturation, double Value) RgbToHsv(Color color)
    {
        var r = color.R / 255d;
        var g = color.G / 255d;
        var b = color.B / 255d;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        double hue = 0;
        if (delta > 0)
        {
            if (max == r)
                hue = 60 * (((g - b) / delta) % 6);
            else if (max == g)
                hue = 60 * (((b - r) / delta) + 2);
            else
                hue = 60 * (((r - g) / delta) + 4);

            if (hue < 0)
                hue += 360;
        }

        var saturation = max == 0 ? 0 : delta / max;
        return (hue, saturation, max);
    }

    private static Color HsvToRgb(double hue, double saturation, double value, byte alpha)
    {
        hue = ((hue % 360) + 360) % 360;
        var chroma = value * saturation;
        var x = chroma * (1 - Math.Abs(hue / 60 % 2 - 1));
        var m = value - chroma;

        double r;
        double g;
        double b;
        if (hue < 60)
        {
            r = chroma;
            g = x;
            b = 0;
        }
        else if (hue < 120)
        {
            r = x;
            g = chroma;
            b = 0;
        }
        else if (hue < 180)
        {
            r = 0;
            g = chroma;
            b = x;
        }
        else if (hue < 240)
        {
            r = 0;
            g = x;
            b = chroma;
        }
        else if (hue < 300)
        {
            r = x;
            g = 0;
            b = chroma;
        }
        else
        {
            r = chroma;
            g = 0;
            b = x;
        }

        return Color.FromArgb(alpha, ToByte(r + m), ToByte(g + m), ToByte(b + m));
    }

    private static byte ToByte(double channel)
    {
        var scaled = (int)Math.Round(channel * 255, MidpointRounding.AwayFromZero);
        if (scaled < 0)
            return 0;
        if (scaled > 255)
            return 255;

        return (byte)scaled;
    }

    private static double Clamp(double value)
    {
        if (value < 0)
            return 0;
        if (value > 1)
            return 1;

        return value;
    }
}
