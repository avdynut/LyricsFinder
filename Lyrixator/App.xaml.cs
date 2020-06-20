using LyricsProviders;
using NLog;
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
            containerRegistry.Register<IPlayerWatcher, YandexMusicWatcher>();
            containerRegistry.Register<ITrackInfoProvider, GoogleTrackInfoProvider>();
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
