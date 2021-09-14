using Lyrixound.Services;
using Newtonsoft.Json.Converters;
using NLog;
using nucs.JsonSettings;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace Lyrixound
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string HelpUrl = "https://lyrixound.blogspot.com/";

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private static readonly AssemblyName _assemblyName = Assembly.GetExecutingAssembly().GetName();
        public static string AppName { get; } = _assemblyName.Name;
        public static string AppNameWithVersion { get; } = $"{AppName} v{_assemblyName.Version.ToString(3)}";

        public App()
        {
            JsonSettings.SerializationSettings.Converters.Add(new StringEnumConverter());
            _logger.Info($"Start {AppNameWithVersion}. Data folder={SettingsService.DataFolder}");
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Fatal(e.Exception, "Unhandled error");
        }
    }
}
