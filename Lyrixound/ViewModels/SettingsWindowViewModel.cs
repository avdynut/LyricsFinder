using LyricsProviders.DirectoriesProvider;
using Lyrixound.Configuration;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Foundation;

namespace Lyrixound.ViewModels
{
    public class SettingsWindowViewModel : BindableBase
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
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

        public bool RunAtStartup
        {
            get => Settings.RunAtStartup;
            set
            {
                Settings.RunAtStartup = value;
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

        public ICommand SaveSettingsCommand { get; }
        public ICommand CheckRunAtStartupCommand { get; }
        public ICommand ChangeRunAtStartupCommand { get; }

        public SettingsWindowViewModel(Settings settings, DirectoriesProviderSettings directoriesSettings)
        {
            Settings = settings;
            _directoriesSettings = directoriesSettings;

            SaveSettingsCommand = new DelegateCommand(Settings.Save);
            CheckRunAtStartupCommand = new DelegateCommand(async () => await GetRunAtStartupEnabledAsync());
            ChangeRunAtStartupCommand = new DelegateCommand(async () => await ChangeRunAtStartupEnabledAsync());
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
    }
}
