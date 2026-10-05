using Lyrixound.Configuration;
using Lyrixound.Services;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Lyrixound.ViewModels
{
    public class LyricsSettingsViewModel : BindableBase
    {
        private readonly LyricsSettings _lyricsSettings;
        private readonly ThemeService _themeService;
        private bool _applyingPreset;

        public double FontSize
        {
            get => Math.Round(_lyricsSettings.FontSize, 0);
            set
            {
                _lyricsSettings.FontSize = Math.Round(Math.Clamp(value, 7, 200), 0);
                RaisePropertyChanged();
                UpdateSelectedPreset();
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
                UpdateSelectedPreset();
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
                UpdateSelectedPreset();
            }
        }

        public FontFamily FontFamily
        {
            get => _lyricsSettings.FontFamily;
            set
            {
                _lyricsSettings.FontFamily = value;
                RaisePropertyChanged();
                UpdateSelectedPreset();
            }
        }

        public TextAlignment TextAlignment
        {
            get => _lyricsSettings.TextAlignment;
            set
            {
                _lyricsSettings.TextAlignment = value;
                RaisePropertyChanged();
                UpdateSelectedPreset();
            }
        }

        public Color TextColor
        {
            get => _lyricsSettings.TextColor;
            set
            {
                _lyricsSettings.TextColor = value;
                RaisePropertyChanged();
                UpdateSelectedPreset();
            }
        }

        public Color ShadowColor
        {
            get => _lyricsSettings.ShadowColor;
            set
            {
                _lyricsSettings.ShadowColor = value;
                RaisePropertyChanged();
                UpdateSelectedPreset();
            }
        }

        public Color ActiveLineBackgroundColor
        {
            get => _lyricsSettings.ActiveLineBackgroundColor;
            set
            {
                _lyricsSettings.ActiveLineBackgroundColor = value;
                RaisePropertyChanged();
                UpdateSelectedPreset();
            }
        }

        public double ShadowDepth
        {
            get => Math.Round(_lyricsSettings.ShadowDepth, 0);
            set
            {
                _lyricsSettings.ShadowDepth = Math.Round(Math.Clamp(value, -20, 20), 0);
                RaisePropertyChanged();
                UpdateSelectedPreset();
            }
        }

        public double BlurRadius
        {
            get => Math.Round(_lyricsSettings.BlurRadius, 0);
            set
            {
                _lyricsSettings.BlurRadius = Math.Round(Math.Clamp(value, 0, 100), 0);
                RaisePropertyChanged();
                UpdateSelectedPreset();
            }
        }

        public double TimeOffsetSeconds
        {
            get => Math.Round(_lyricsSettings.TimeOffsetSeconds, 1);
            set
            {
                _lyricsSettings.TimeOffsetSeconds = Math.Round(Math.Clamp(value, -10, 10), 1);
                RaisePropertyChanged();
            }
        }

        public double FloatingBackgroundOpacity
        {
            get => Math.Round(_lyricsSettings.FloatingBackgroundOpacity, 1);
            set
            {
                _lyricsSettings.FloatingBackgroundOpacity = Math.Round(Math.Clamp(value, 0, 1), 1);
                RaisePropertyChanged();
                UpdateSelectedPreset();
            }
        }

        public FontStyle FontStyle => IsItalic ? FontStyles.Italic : FontStyles.Normal;
        public FontWeight FontWeight => IsBold ? FontWeights.Bold : FontWeights.Normal;

        public IReadOnlyList<LyricsStylePreset> Presets { get; } =
        [
            new LyricsStylePreset("Classic", "Segoe UI", 18, bold: true, italic: false, TextAlignment.Center, "#FF000000", "#00000000", 0, 0, "#00000000", 0.4, useThemeTextColor: true),
            new LyricsStylePreset("Glow", "Segoe UI", 18, bold: true, italic: false, TextAlignment.Center, "#FFFFFFFF", "#FF7C4DFF", 0, 16, "#418571DE", 0.4),
            new LyricsStylePreset("Soft", "Georgia", 22, bold: false, italic: true, TextAlignment.Center, "#FFFFF8E1", "#FF5D4037", 1, 6, "#FF8D6E63", 0.3),
            new LyricsStylePreset("Poster", "Impact", 32, bold: false, italic: false, TextAlignment.Center, "#FFFFFFFF", "#FF000000", 3, 0, "#8FEB2424", 0.5),
            new LyricsStylePreset("Neon", "Consolas", 20, bold: false, italic: false, TextAlignment.Left, "#FF69F0AE", "#FF00E5FF", 0, 14, "#E60A7D69", 0.5),
        ];

        public ICommand ApplyPresetCommand { get; }

        public Dictionary<TextAlignment, PackIconKind> TextAlignments { get; } = new Dictionary<TextAlignment, PackIconKind>
        {
            { TextAlignment.Left, PackIconKind.FormatAlignLeft },
            { TextAlignment.Center, PackIconKind.FormatAlignCenter },
            { TextAlignment.Right, PackIconKind.FormatAlignRight },
            //{ TextAlignment.Justify, PackIconKind.FormatAlignJustify }
        };

        public LyricsSettingsViewModel(LyricsSettings lyricsSettings, ThemeService themeService)
        {
            _lyricsSettings = lyricsSettings;
            _themeService = themeService;
            ApplyPresetCommand = new DelegateCommand<LyricsStylePreset>(ApplyPreset);
            _themeService.ThemeChanged += (_, _) => OnThemeChanged();
            ApplyThemeToPresets();
            ApplySavedPreset();
        }

        private void ApplySavedPreset()
        {
            if (_lyricsSettings.PresetName == "Custom")
            {
                UpdateSelectedPreset();
                return;
            }

            var preset = Presets.FirstOrDefault(item => item.Name == _lyricsSettings.PresetName)
                ?? Presets.First(item => item.Name == "Classic");
            ApplyPreset(preset);
        }

        private void OnThemeChanged()
        {
            var classicSelected = Presets.First(preset => preset.Name == "Classic").IsSelected;
            ApplyThemeToPresets();
            if (classicSelected)
                TextColor = Presets.First(preset => preset.Name == "Classic").TextColor;

            UpdateSelectedPreset();
        }

        private void ApplyThemeToPresets()
        {
            foreach (var preset in Presets)
                preset.ApplyTheme();
        }

        private void UpdateSelectedPreset()
        {
            if (_applyingPreset)
                return;

            LyricsStylePreset selected = null;
            foreach (var preset in Presets)
            {
                preset.IsSelected = preset.Matches(this);
                if (preset.IsSelected)
                    selected = preset;
            }

            var name = selected?.Name ?? "Custom";
            if (_lyricsSettings.PresetName != name)
                _lyricsSettings.PresetName = name;
        }

        private void ApplyPreset(LyricsStylePreset preset)
        {
            if (preset == null)
                return;

            _applyingPreset = true;
            try
            {
                FontFamily = preset.FontFamily;
                FontSize = preset.FontSize;
                IsBold = preset.IsBold;
                IsItalic = preset.IsItalic;
                TextAlignment = preset.TextAlignment;
                TextColor = preset.TextColor;
                ShadowColor = preset.ShadowColor;
                ShadowDepth = preset.ShadowDepth;
                BlurRadius = preset.BlurRadius;
                ActiveLineBackgroundColor = preset.ActiveLineBackgroundColor;
                FloatingBackgroundOpacity = preset.BackgroundOpacity;
            }
            finally
            {
                _applyingPreset = false;
            }

            UpdateSelectedPreset();
        }
    }
}
