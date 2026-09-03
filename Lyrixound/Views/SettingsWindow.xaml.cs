using Lyrixound.Services;
using System.Windows;

namespace Lyrixound.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
            : this(null)
        {
        }

        public SettingsWindow(ThemeService themeService)
        {
            InitializeComponent();

            if (themeService != null)
            {
                NativeWindowTheme.BindTitleBarTheme(this, themeService);
            }
        }
    }
}
