using LyricsProviders.DirectoriesProvider;
using Lyrixator.Configuration;
using Prism.Mvvm;
using System;
using System.Linq;

namespace Lyrixator.ViewModels
{
    public class SettingsWindowViewModel : BindableBase
    {
        private readonly LyricsSettings _lyricsSettings;
        private readonly DirectoriesProviderSettings _directoriesSettings;

        public Settings Settings { get; }

        public double CheckInterval
        {
            get => Settings.CheckInterval.TotalSeconds;
            set
            {
                Settings.CheckInterval = TimeSpan.FromSeconds(value);
                RaisePropertyChanged();
            }
        }

        public string LyricsDirectory
        {
            get => _directoriesSettings.LyricsDirectories.FirstOrDefault();
            set
            {
                _directoriesSettings.LyricsDirectories[0] = value;
                RaisePropertyChanged();
            }
        }

        public string FileNamePattern
        {
            get => _directoriesSettings.LyricsFileNamePattern;
            set
            {
                if (value.Contains(DirectoriesTrackInfoProvider.ArtistMask) && value.Contains(DirectoriesTrackInfoProvider.TitleMask))
                {
                    _directoriesSettings.LyricsFileNamePattern = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SettingsWindowViewModel(Settings settings, LyricsSettings lyricsSettings, DirectoriesProviderSettings directoriesSettings)
        {
            Settings = settings;
            _lyricsSettings = lyricsSettings;
            _directoriesSettings = directoriesSettings;
        }
    }
}
