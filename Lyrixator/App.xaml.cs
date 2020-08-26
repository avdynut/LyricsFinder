using LyricsProviders;
using LyricsProviders.DirectoriesProvider;
using LyricsProviders.GoogleProvider;
using Lyrixator.Configuration;
using Newtonsoft.Json.Converters;
using NLog;
using nucs.JsonSettings;
using nucs.JsonSettings.Autosave;
using PlayerWatching;
using Prism.Ioc;
using Prism.Ninject;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace Lyrixator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            JsonSettings.SerializationSettings.Converters.Add(new StringEnumConverter());

            containerRegistry
                .Register<ITrackInfoProvider, DirectoriesTrackInfoProvider>(DirectoriesTrackInfoProvider.Name)
                .Register<ITrackInfoProvider, GoogleTrackInfoProvider>(GoogleTrackInfoProvider.Name)
                .Register<IPlayerWatcher, SmtcWatcher>(SmtcWatcher.Name)
                .Register<IPlayerWatcher, YandexMusicWatcher>(YandexMusicWatcher.Name);

            var settings = JsonSettings.Load<Settings>().EnableAutosave();

            var playerWatchers = new List<IPlayerWatcher>();
            foreach (var watcher in settings.PlayerWatchers)
            {
                if (watcher.Value && containerRegistry.IsRegistered<IPlayerWatcher>(watcher.Key))
                {
                    playerWatchers.Add(Container.Resolve<IPlayerWatcher>(watcher.Key));
                }
            }

            var lyricsProviders = new List<ITrackInfoProvider>();
            foreach (var provider in settings.LyricsProviders)
            {
                if (provider.Value && containerRegistry.IsRegistered<ITrackInfoProvider>(provider.Key))
                {
                    lyricsProviders.Add(Container.Resolve<ITrackInfoProvider>(provider.Key));
                }
            }

            containerRegistry
                .RegisterInstance(settings)
                .RegisterInstance(new MultiPlayerWatcher(playerWatchers, settings.CheckInterval))
                .RegisterInstance(new MultiTrackInfoProvider(lyricsProviders));
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<Views.MainWindow>();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Fatal(e.Exception, "Unhandled error");
        }
    }
}
