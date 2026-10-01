using Lyrixound.Services;
using System.Windows;

namespace Lyrixound.Views
{
    public partial class LyricsSettingsWindow : Window
    {
        public LyricsSettingsWindow(ThemeService themeService)
        {
            InitializeComponent();
            NativeWindowTheme.BindTitleBarTheme(this, themeService);
        }
    }
}
