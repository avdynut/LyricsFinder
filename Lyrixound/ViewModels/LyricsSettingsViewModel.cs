using Lyrixound.Configuration;
using MaterialDesignThemes.Wpf;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Lyrixound.ViewModels
{
    public class LyricsSettingsViewModel : ObservableObject
    {
        private readonly LyricsSettings _lyricsSettings;

        public double FontSize
        {
            get => _lyricsSettings.FontSize;
            set => SetProperty(FontSize, value, _lyricsSettings, (s, v) => s.FontSize = v);
        }

        public bool IsItalic
        {
            get => _lyricsSettings.IsItalic;
            set
            {
                if (SetProperty(IsItalic, value, _lyricsSettings, (s, v) => s.IsItalic = v))
                {
                    OnPropertyChanged(nameof(FontStyle));
                }
            }
        }

        public bool IsBold
        {
            get => _lyricsSettings.IsBold;
            set
            {
                if (SetProperty(IsBold, value, _lyricsSettings, (s, v) => s.IsBold = v))
                {
                    OnPropertyChanged(nameof(FontWeight));
                }
            }
        }

        public FontFamily FontFamily
        {
            get => _lyricsSettings.FontFamily;
            set => SetProperty(FontFamily, value, _lyricsSettings, (s, v) => s.FontFamily = v);
        }

        public TextAlignment TextAlignment
        {
            get => _lyricsSettings.TextAlignment;
            set => SetProperty(TextAlignment, value, _lyricsSettings, (s, v) => s.TextAlignment = v);
        }

        public Color TextColor
        {
            get => _lyricsSettings.TextColor;
            set => SetProperty(TextColor, value, _lyricsSettings, (s, v) => s.TextColor = v);
        }

        public Color ShadowColor
        {
            get => _lyricsSettings.ShadowColor;
            set => SetProperty(ShadowColor, value, _lyricsSettings, (s, v) => s.ShadowColor = v);
        }

        public double ShadowDepth
        {
            get => _lyricsSettings.ShadowDepth;
            set => SetProperty(ShadowDepth, value, _lyricsSettings, (s, v) => s.ShadowDepth = v);
        }

        public double BlurRadius
        {
            get => _lyricsSettings.BlurRadius;
            set => SetProperty(BlurRadius, value, _lyricsSettings, (s, v) => s.BlurRadius = v);
        }

        public FontStyle FontStyle => IsItalic ? FontStyles.Italic : FontStyles.Normal;
        public FontWeight FontWeight => IsBold ? FontWeights.Bold : FontWeights.Normal;

        public Dictionary<TextAlignment, PackIconKind> TextAlignments { get; } = new Dictionary<TextAlignment, PackIconKind>
        {
            { TextAlignment.Left, PackIconKind.FormatAlignLeft },
            { TextAlignment.Center, PackIconKind.FormatAlignCenter },
            { TextAlignment.Right, PackIconKind.FormatAlignRight },
            //{ TextAlignment.Justify, PackIconKind.FormatAlignJustify }
        };

        public LyricsSettingsViewModel(LyricsSettings lyricsSettings)
        {
            _lyricsSettings = lyricsSettings;
        }
    }
}
