using LyricsProviders;
using LyricsProviders.DirectoriesProvider;
using LyricsProviders.GoogleProvider;
using Newtonsoft.Json.Converters;
using NLog;
using nucs.JsonSettings;
using PlayerWatching;
using Prism.Ioc;
using Prism.Ninject;
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
                .RegisterInstance(JsonSettings.Load<DirectoriesProviderSettings>())
                .Register<ITrackInfoProvider, DirectoriesTrackInfoProvider>()
                .Register<ITrackInfoProvider, GoogleTrackInfoProvider>()
                .Register<IPlayerWatcher, SmtcWatcher>()
                .Register<IPlayerWatcher, YandexMusicWatcher>();
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
