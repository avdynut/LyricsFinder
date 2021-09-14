using Lyrixound.Configuration;
using Lyrixound.Services;
using Lyrixound.ViewModels;
using System.Windows.Controls;

namespace Lyrixound.Views
{
    /// <summary>
    /// Interaction logic for LyricsSettingsView
    /// </summary>
    public partial class LyricsSettingsView : UserControl
    {
        public LyricsSettingsView()
        {
            InitializeComponent();

            var lyricsSettings = SettingsService.LoadSettings<LyricsSettings>("lyrics.json");
            DataContext = new LyricsSettingsViewModel(lyricsSettings);
        }
    }
}
