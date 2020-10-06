using Lyrixator.Configuration;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Media;

namespace Lyrixator.ViewModels
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

        public FontFamily FontFamily
        {
            get => _lyricsSettings.FontFamily;
            set
            {
                _lyricsSettings.FontFamily = value;
                RaisePropertyChanged();
            }
        }

        public FontStyle FontStyle
        {
            get => _lyricsSettings.FontStyle;
            set
            {
                _lyricsSettings.FontStyle = value;
                RaisePropertyChanged();
            }
        }

        public FontWeight FontWeight
        {
            get => _lyricsSettings.FontWeight;
            set
            {
                _lyricsSettings.FontWeight = value;
                RaisePropertyChanged();
            }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get => _lyricsSettings.HorizontalAlignment;
            set
            {
                _lyricsSettings.HorizontalAlignment = value;
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

        public double BlurRadius
        {
            get => _lyricsSettings.BlurRadius;
            set
            {
                _lyricsSettings.BlurRadius = value;
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

        public LyricsSettingsViewModel(LyricsSettings lyricsSettings)
        {
            _lyricsSettings = lyricsSettings;
        }
    }
}
