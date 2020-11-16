using LyricsProviders;
using LyricsProviders.DirectoriesProvider;
using LyricsProviders.GoogleProvider;
using Lyrixound.Configuration;
using Newtonsoft.Json.Converters;
using NLog;
using nucs.JsonSettings;
using nucs.JsonSettings.Autosave;
using PlayerWatching;
using Prism.Ioc;
using Prism.Ninject;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly string _dataFolder;

        public App()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();

#if PORTABLE
            _dataFolder = AppDomain.CurrentDomain.BaseDirectory;
#else
            _dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), assemblyName.Name);
            Directory.CreateDirectory(_dataFolder);
#endif
            _logger.Info($"Start {assemblyName.Name} {assemblyName.Version}. Data folder={_dataFolder}");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            JsonSettings.SerializationSettings.Converters.Add(new StringEnumConverter());

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
                .Register<ITrackInfoProvider, DirectoriesTrackInfoProvider>(DirectoriesTrackInfoProvider.Name)
                .Register<ITrackInfoProvider, GoogleTrackInfoProvider>(GoogleTrackInfoProvider.Name)
                .Register<IPlayerWatcher, SmtcWatcher>(SmtcWatcher.Name)
                .Register<IPlayerWatcher, YandexMusicWatcher>(YandexMusicWatcher.Name);

            var playerWatchers = new List<IPlayerWatcher>();
            foreach (var watcher in settings.PlayerWatchers)
            {
                if (watcher.IsEnabled && containerRegistry.IsRegistered<IPlayerWatcher>(watcher.Name))
                {
                    playerWatchers.Add(Container.Resolve<IPlayerWatcher>(watcher.Name));
                }
            }

            var lyricsProviders = new List<ITrackInfoProvider>();
            foreach (var provider in settings.LyricsProviders)
            {
                if (provider.IsEnabled && containerRegistry.IsRegistered<ITrackInfoProvider>(provider.Name))
                {
                    lyricsProviders.Add(Container.Resolve<ITrackInfoProvider>(provider.Name));
                }
            }

            containerRegistry
                .RegisterInstance(new MultiPlayerWatcher(playerWatchers, settings.CheckInterval))
                .RegisterInstance(new MultiTrackInfoProvider(lyricsProviders));
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<Views.MainWindow>();
        }

        private T LoadSettings<T>(string filename) where T : JsonSettings
        {
            var filePath = Path.Combine(_dataFolder, "settings", filename);
            return JsonSettings.Load<T>(filePath).EnableAutosave();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Fatal(e.Exception, "Unhandled error");
        }
    }
}
