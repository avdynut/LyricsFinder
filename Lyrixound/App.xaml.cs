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

        public static string DataFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Assembly.GetExecutingAssembly().GetName().Name);

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            Directory.CreateDirectory(DataFolder);
            _logger.Info($"Load settings from {DataFolder}");

            JsonSettings.SerializationSettings.Converters.Add(new StringEnumConverter());

            containerRegistry
                .Register<ITrackInfoProvider, DirectoriesTrackInfoProvider>(DirectoriesTrackInfoProvider.Name)
                .Register<ITrackInfoProvider, GoogleTrackInfoProvider>(GoogleTrackInfoProvider.Name)
                .Register<IPlayerWatcher, SmtcWatcher>(SmtcWatcher.Name)
                .Register<IPlayerWatcher, YandexMusicWatcher>(YandexMusicWatcher.Name);

            var settings = LoadSettings<Settings>("app.json");

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
                .RegisterInstance(settings)
                .RegisterInstance(LoadSettings<LyricsSettings>("lyrics.json"))
                .RegisterInstance(LoadSettings<WindowSettings>("window.json"))
                .RegisterInstance(LoadSettings<DirectoriesProviderSettings>("directories_provider.json"))
                .RegisterInstance(LoadSettings<GoogleProviderSettings>("google_provider.json"))
                .RegisterInstance(new MultiPlayerWatcher(playerWatchers, settings.CheckInterval))
                .RegisterInstance(new MultiTrackInfoProvider(lyricsProviders));
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<Views.MainWindow>();
        }

        private T LoadSettings<T>(string filename) where T : JsonSettings
        {
            var filePath = Path.Combine(DataFolder, "settings", filename);
            return JsonSettings.Load<T>(filePath).EnableAutosave();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Fatal(e.Exception, "Unhandled error");
        }
    }
}
