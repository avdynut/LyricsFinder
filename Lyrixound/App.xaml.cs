using LyricsProviders;
using LyricsProviders.DirectoriesProvider;
using LyricsProviders.Genius;
using LyricsProviders.GoogleProvider;
using LyricsProviders.LrcLib;
using LyricsProviders.LyricsOvh;
using LyricsProviders.MusixMatch;
using Lyrixound.Configuration;
using Lyrixound.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using Nucs.JsonSettings;
using Nucs.JsonSettings.Autosave;
using Prism.Ioc;
using Prism.Ninject;
using SmtcWatcher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace Lyrixound
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        public const string HelpUrl = "https://lyrixound.app/";

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly string _dataFolder;

        private static readonly AssemblyName _assemblyName = Assembly.GetExecutingAssembly().GetName();
        public static string AppName { get; } = _assemblyName.Name;
        public static string AppNameWithVersion { get; } = $"{AppName} v{_assemblyName.Version.ToString(3)}";

        public App()
        {
#if PORTABLE
            _dataFolder = AppDomain.CurrentDomain.BaseDirectory;
#else
            _dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);
            Directory.CreateDirectory(_dataFolder);
#endif
            _logger.Info($"Start {AppNameWithVersion}. Data folder={_dataFolder}");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            JsonSettings.SerializationSettings.Converters.Add(new StringEnumConverter());
            JsonSettings.SerializationSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            var settings = LoadSettings<Settings>("app.json");
            var directoriesSettings = LoadSettings<DirectoriesProviderSettings>("directories_provider.json");
            if (directoriesSettings.LyricsDirectories.Count == 0)
            {
                var lyricsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "lyrics");
                directoriesSettings.LyricsDirectories.Add(lyricsFolder);
                directoriesSettings.Save();
            }

            containerRegistry
                .RegisterInstance(settings)
                .RegisterInstance(directoriesSettings)
                .RegisterInstance(LoadSettings<LyricsSettings>("lyrics.json"))
                .RegisterInstance(LoadSettings<WindowSettings>("window.json"))
                .RegisterInstance(LoadSettings<GoogleProviderSettings>("google_provider.json"))
                .RegisterSingleton<ILicenseAnalytics, NLogLicenseAnalytics>();

#if DEBUG
            containerRegistry.RegisterSingleton<ILicenseService, MockLicenseService>();
#elif PORTABLE
            containerRegistry.RegisterSingleton<ILicenseService, FreeLicenseService>();
#else
            containerRegistry.RegisterSingleton<ILicenseService, StoreLicenseService>();
#endif

            containerRegistry.RegisterSingleton<ThemeService>();

            containerRegistry
                .Register<ITrackInfoProvider, DirectoriesTrackInfoProvider>(DirectoriesTrackInfoProvider.Name)
                .Register<ITrackInfoProvider, LrcLibTrackInfoProvider>(LrcLibTrackInfoProvider.Name)
                .Register<ITrackInfoProvider, MusixmatchTrackInfoProvider>(MusixmatchTrackInfoProvider.Name)
                .Register<ITrackInfoProvider, GeniusTrackInfoProvider>(GeniusTrackInfoProvider.Name)
                .Register<ITrackInfoProvider, LyricsOvhTrackInfoProvider>(LyricsOvhTrackInfoProvider.Name)
                .Register<ITrackInfoProvider, GoogleTrackInfoProvider>(GoogleTrackInfoProvider.Name);

            var providersByName = new Dictionary<string, ITrackInfoProvider>();
            foreach (var provider in settings.LyricsProviders)
            {
                if (containerRegistry.IsRegistered<ITrackInfoProvider>(provider.Name))
                    providersByName[provider.Name] = Container.Resolve<ITrackInfoProvider>(provider.Name);
            }

            containerRegistry
                .RegisterInstance(new CyclicalSmtcWatcher(settings.CheckInterval))
                .RegisterInstance(new MultiTrackInfoProvider(() => ActiveProviders(settings, providersByName)));
        }

        protected override Window CreateShell()
        {
            var mainWindow = Container.Resolve<Views.MainWindow>();

            // Show rating reminder on every third launch
            var settings = Container.Resolve<Settings>();
            ShowRatingReminderIfNeeded(settings, mainWindow);

            return mainWindow;
        }

        private void ShowRatingReminderIfNeeded(Settings settings, Window owner)
        {
            // Increment launch count
            settings.LaunchCount++;
            settings.Save();

            // Check if we should show the reminder
            // Show every 3rd launch, but not if user said "don't show again"
            if (!settings.DontShowRatingReminder && settings.LaunchCount % 3 == 0)
            {
                // Show the rating reminder after the main window is loaded
                owner.Loaded += (s, e) =>
                {
                    var ratingWindow = new Views.RatingReminderWindow(
                        settings,
                        Container.Resolve<ILicenseService>(),
                        Container.Resolve<ILicenseAnalytics>())
                    {
                        Owner = owner
                    };
                    ratingWindow.ShowDialog();
                };
            }
        }

        private T LoadSettings<T>(string filename) where T : JsonSettings
        {
            var filePath = Path.Combine(_dataFolder, "settings", filename);
            try
            {
                return JsonSettings.Load<T>(filePath).EnableAutosave();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to load settings from {filename}, resetting to defaults");

                if (File.Exists(filePath))
                {
                    var backupPath = filePath + ".bak";
                    try { File.Copy(filePath, backupPath, overwrite: true); } catch { }
                    File.Delete(filePath);
                }

                return JsonSettings.Load<T>(filePath).EnableAutosave();
            }
        }

        private static List<ITrackInfoProvider> ActiveProviders(
            Settings settings,
            Dictionary<string, ITrackInfoProvider> providersByName)
        {
            var active = new List<ITrackInfoProvider>();
            foreach (var provider in settings.LyricsProviders.ToArray())
            {
                if (provider.IsEnabled && providersByName.TryGetValue(provider.Name, out var implementation))
                    active.Add(implementation);
            }

            return active;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Fatal(e.Exception, "Unhandled error");
            e.Handled = true;
        }
    }
}
