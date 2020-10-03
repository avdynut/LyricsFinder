﻿using LyricsProviders;
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
                .RegisterInstance(JsonSettings.Load<LyricsSettings>().EnableAutosave())
                .RegisterInstance(JsonSettings.Load<WindowSettings>().EnableAutosave())
                .RegisterInstance(JsonSettings.Load<DirectoriesProviderSettings>().EnableAutosave())
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
