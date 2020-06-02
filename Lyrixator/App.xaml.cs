using LyricsProviders;
using PlayerWatching;
using Prism.Ioc;
using Prism.Ninject;
using System.Windows;

namespace Lyrixator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IPlayerWatcher, YandexMusicWatcher>();
            containerRegistry.Register<ITrackInfoProvider, GoogleTrackInfoProvider>();
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<Views.MainWindow>();
        }
    }
}
