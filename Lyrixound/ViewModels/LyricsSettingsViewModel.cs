using Lyrixound.Configuration;
using MaterialDesignThemes.Wpf;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Lyrixound.ViewModels
{
    public class LyricsSettingsViewModel : BindableBase
    {
        private readonly LyricsSettings _lyricsSettings;

        public double FontSize
        {
            get => _lyricsSettings.FontSize;
            set
            {
                _lyricsSettings.FontSize = value;
                RaisePropertyChanged();
            }
        }

        public bool IsItalic
        {
            get => _lyricsSettings.IsItalic;
            set
            {
                _lyricsSettings.IsItalic = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(FontStyle));
            }
        }

        public bool IsBold
        {
            get => _lyricsSettings.IsBold;
            set
            {
                _lyricsSettings.IsBold = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(FontWeight));
            }
        }

        public FontFamily FontFamily
        {
            get => _lyricsSettings.FontFamily;
            set
            {
                _lyricsSettings.FontFamily = value;
                RaisePropertyChanged();
            }
        }

        public TextAlignment TextAlignment
        {
            get => _lyricsSettings.TextAlignment;
            set
            {
                _lyricsSettings.TextAlignment = value;
                RaisePropertyChanged();
            }
        }

        public Color TextColor
        {
            get => _lyricsSettings.TextColor;
            set
            {
                _lyricsSettings.TextColor = value;
                RaisePropertyChanged();
            }
        }

        public Color ShadowColor
        {
            get => _lyricsSettings.ShadowColor;
            set
            {
                _lyricsSettings.ShadowColor = value;
                RaisePropertyChanged();
            }
        }

        public double ShadowDepth
        {
            get => _lyricsSettings.ShadowDepth;
            set
            {
                _lyricsSettings.ShadowDepth = value;
                RaisePropertyChanged();
            }
        }

        public double BlurRadius
        {
            get => _lyricsSettings.BlurRadius;
            set
            {
                _lyricsSettings.BlurRadius = value;
                RaisePropertyChanged();
            }
        }

        public double TimeOffsetMilliseconds
        {
            get => _lyricsSettings.TimeOffsetMilliseconds;
            set
            {
                _lyricsSettings.TimeOffsetMilliseconds = value;
                RaisePropertyChanged();
            }
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
