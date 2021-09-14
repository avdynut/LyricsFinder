using LyricsProviders.DirectoriesProvider;
using Lyrixound.Configuration;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using NLog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Foundation;

namespace Lyrixound.ViewModels
{
    public class SettingsWindowViewModel : ObservableValidator
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly DirectoriesProviderSettings _directoriesSettings;

        public Settings Settings { get; }

        [Required]
        public double CheckInterval
        {
            get => Settings.CheckInterval.TotalSeconds;
            set => SetProperty(CheckInterval, value, Settings, (s, v) => s.CheckInterval = TimeSpan.FromSeconds(v));
        }

        public bool RunAtStartup
        {
            get => Settings.RunAtStartup;
            set => SetProperty(RunAtStartup, value, Settings, (s, v) => s.RunAtStartup = v);
        }

        [Required]
        public string LyricsDirectory
        {
            get => _directoriesSettings.LyricsDirectories.FirstOrDefault();
            set => SetProperty(LyricsDirectory, value, _directoriesSettings, (s, v) => s.LyricsDirectories[0] = v);
        }

        [Required]
        [CustomValidation(typeof(SettingsWindowViewModel), nameof(ValidateFileNamePattern))]
        public string FileNamePattern
        {
            get => _directoriesSettings.LyricsFileNamePattern;
            set => SetProperty(FileNamePattern, value, _directoriesSettings, (s, v) => s.LyricsFileNamePattern = v);
        }

        public ICommand SaveSettingsCommand { get; }
        public ICommand CheckRunAtStartupCommand { get; }
        public ICommand ChangeRunAtStartupCommand { get; }

        public SettingsWindowViewModel(Settings settings, DirectoriesProviderSettings directoriesSettings)
        {
            Settings = settings;
            _directoriesSettings = directoriesSettings;

            SaveSettingsCommand = new RelayCommand(Settings.Save);
            CheckRunAtStartupCommand = new AsyncRelayCommand(GetRunAtStartupEnabledAsync);
            ChangeRunAtStartupCommand = new AsyncRelayCommand(ChangeRunAtStartupEnabledAsync);
        }

        private static IAsyncOperation<StartupTask> GetStartupTaskAsync() => StartupTask.GetAsync(App.AppName);

        private async Task GetRunAtStartupEnabledAsync()
        {
            try
            {
                var startupTask = await GetStartupTaskAsync();
                RunAtStartup = startupTask.State == StartupTaskState.Enabled;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Cannot get run at startup status");
                RunAtStartup = false;
            }
        }

        private async Task ChangeRunAtStartupEnabledAsync()
        {
            try
            {
                var startupTask = await GetStartupTaskAsync();

                if (RunAtStartup)
                {
                    await startupTask.RequestEnableAsync();
                }
                else
                {
                    startupTask.Disable();
                }
                RunAtStartup = startupTask.State == StartupTaskState.Enabled;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"Cannot set run at startup to {RunAtStartup}");
                RunAtStartup = false;
            }
        }

        private static ValidationResult ValidateFileNamePattern(string value, ValidationContext context)
        {
            return value.Contains(DirectoriesTrackInfoProvider.ArtistMask) && value.Contains(DirectoriesTrackInfoProvider.TitleMask)
                ? ValidationResult.Success
                : new($"Filename pattern must contain '{DirectoriesTrackInfoProvider.ArtistMask}' and '{DirectoriesTrackInfoProvider.TitleMask}'");
        }
    }
}
