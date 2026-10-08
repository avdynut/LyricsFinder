using LyricsProviders.DirectoriesProvider;
using Lyrixound.Configuration;
using Lyrixound.Services;
using Lyrixound.Views;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Foundation;

namespace Lyrixound.ViewModels
{
    public class SettingsWindowViewModel : BindableBase, IDisposable
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly DirectoriesProviderSettings _directoriesSettings;
        private readonly ILicenseService _licenseService;
        private readonly ILicenseAnalytics _licenseAnalytics;
        private readonly ThemeService _themeService;
        private bool _disposed;

        public SettingsWindowViewModel(
            Settings settings,
            DirectoriesProviderSettings directoriesSettings,
            ILicenseService licenseService,
            ILicenseAnalytics licenseAnalytics,
            ThemeService themeService)
        {
            Settings = settings;
            _directoriesSettings = directoriesSettings;
            _licenseService = licenseService;
            _licenseAnalytics = licenseAnalytics;
            _themeService = themeService;

            _licenseService.EntitlementChanged += OnEntitlementChanged;

            SaveSettingsCommand = new DelegateCommand(Settings.Save);
            ClosingCommand = new DelegateCommand(OnClosing);
            InitializeCommand = new DelegateCommand(async () => await InitializeAsync());
            ChangeRunAtStartupCommand = new DelegateCommand(async () => await ChangeRunAtStartupEnabledAsync());
            UnlockProCommand = new DelegateCommand(ShowPaywall);
            RestorePurchasesCommand = new DelegateCommand(async () => await RestorePurchasesAsync());
            OpenSupportEmailCommand = new DelegateCommand(OpenSupportEmail);
            OpenWebsiteCommand = new DelegateCommand(OpenWebsite);
            RateAppCommand = new DelegateCommand(RateApp);
        }

        public Settings Settings { get; }

        public bool IsPro => _licenseService.IsPro;

        public bool IsDarkTheme
        {
            get => _themeService.IsDarkThemeActive;
            set
            {
                if (value && !_licenseService.IsPro)
                {
                    ShowPaywall();
                    RaisePropertyChanged();
                    return;
                }

                if (_licenseService.IsPro)
                {
                    _themeService.SetDarkTheme(value);
                }

                RaisePropertyChanged();
            }
        }

        public string ProStatusText => IsPro
            ? "Thank you for supporting Lyrixound!"
            : "You are using the free version.";

        public string SupportEmail => ProConstants.SupportEmail;

        public bool CanSimulatePro => _licenseService is IMockLicenseService;

        public bool SimulatePro
        {
            get => _licenseService is IMockLicenseService mock && mock.SimulatePro;
            set
            {
                if (_licenseService is IMockLicenseService mock)
                {
                    mock.SimulatePro = value;
                    RaisePropertyChanged(nameof(SimulatePro));
                    RaiseProStatusChanged();
                }
            }
        }

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
        public ICommand ClosingCommand { get; }
        public ICommand InitializeCommand { get; }
        public ICommand ChangeRunAtStartupCommand { get; }
        public ICommand UnlockProCommand { get; }
        public ICommand RestorePurchasesCommand { get; }
        public ICommand OpenSupportEmailCommand { get; }
        public ICommand OpenWebsiteCommand { get; }
        public ICommand RateAppCommand { get; }

        private void OnClosing()
        {
            Settings.Save();
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _licenseService.EntitlementChanged -= OnEntitlementChanged;
        }

        private void OnEntitlementChanged(object sender, EventArgs e)
        {
            RaiseProStatusChanged();
        }

        private async Task InitializeAsync()
        {
            await RefreshLicenseAsync();
            await GetRunAtStartupEnabledAsync();
        }

        private async Task RefreshLicenseAsync()
        {
            try
            {
                await _licenseService.RefreshAsync();
                RaiseProStatusChanged();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to refresh license status");
            }
        }

        private void ShowPaywall()
        {
            var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                ?? Application.Current.MainWindow;

            PaywallWindow.Show(owner, _licenseService, _licenseAnalytics, "settings");
            RaiseProStatusChanged();
        }

        private async Task RestorePurchasesAsync()
        {
            try
            {
                var result = await _licenseService.RestoreAsync();
                if (result == LicenseResult.Success)
                {
                    _licenseAnalytics.PurchaseSucceeded();
                }
                else if (result != LicenseResult.Cancelled)
                {
                    _licenseAnalytics.PurchaseFailed(result.ToString());
                }

                RaiseProStatusChanged();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to restore purchases");
                _licenseAnalytics.PurchaseFailed(ex.Message);
            }
        }

        private void RaiseProStatusChanged()
        {
            RaisePropertyChanged(nameof(IsPro));
            RaisePropertyChanged(nameof(ProStatusText));
            RaisePropertyChanged(nameof(IsDarkTheme));
        }

        private void OpenWebsite()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = App.HelpUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to open the Lyrixound website");
            }
        }

        private void RateApp()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"ms-windows-store://review/?ProductId={ProConstants.StoreAppId}",
                    UseShellExecute = true
                });

                Settings.DontShowRatingReminder = true;
                Settings.Save();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to open the Microsoft Store review page");
            }
        }

        private static void OpenSupportEmail()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:{ProConstants.SupportEmail}",
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                // ignored
            }
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
