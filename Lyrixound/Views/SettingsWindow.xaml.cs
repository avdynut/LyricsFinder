using Lyrixound.Services;
using Lyrixound.ViewModels;
using System.Windows;

namespace Lyrixound.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            DataContext = new SettingsWindowViewModel(SettingsService.Settings, SettingsService.DirectoriesSettings);
        }
    }
}
