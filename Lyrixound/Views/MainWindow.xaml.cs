using Lyrixound.Configuration;
using MaterialDesignThemes.Wpf;
using NLog;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Lyrixound.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly Brush _lyricsPanelBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFAFAFA"));
        private readonly WindowSettings _settings;

        public MainWindow(WindowSettings settings)
        {
            _settings = settings;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            RestoreWindowParameters();
            LyricsPanel.Background = TextSettings.Background = _lyricsPanelBrush;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            _settings.WindowSize = sizeInfo.NewSize;
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            _settings.WindowPosition = new Point(Left, Top);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            var iconKind = WindowState == WindowState.Maximized ? PackIconKind.WindowRestore : PackIconKind.WindowMaximize;
            MaximizeButton.Content = new PackIcon { Kind = iconKind };
            _settings.WindowState = WindowState;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            LyricsPanel.Background.Opacity = TextSettings.Background.Opacity = 1;
            TextSettings.Visibility = Visibility.Visible;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            Lyrics.IsReadOnly = true;
            LyricsPanel.Background.Opacity = TextSettings.Background.Opacity = 0.2;
            TextSettings.IsExpanded = false;
            TextSettings.Visibility = Visibility.Collapsed;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            DragMove();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _logger.Trace("Disposing before exit");

            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            TrayIcon.Dispose();

            _settings.Topmost = Topmost;
            _settings.Save();
            base.OnClosing(e);
        }

        private void RestoreWindowParameters()
        {
            Width = _settings.WindowSize.Width;
            Height = _settings.WindowSize.Height;
            WindowState = _settings.WindowState;
            Topmost = _settings.Topmost;

            if (_settings.WindowPosition.HasValue)
            {
                Left = _settings.WindowPosition.Value.X;
                Top = _settings.WindowPosition.Value.Y;
            }
        }

        private void OnTaskbarIconTrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                MinimizeWindow();
            }
        }

        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            MinimizeWindow();
        }

        private void OnMaximizeButtonClick(object sender, RoutedEventArgs e)
        {
            MaximizeWindow();
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnTitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                MaximizeWindow();
            }
        }

        private void MinimizeWindow()
        {
            ShowInTaskbar = true;
            WindowState = WindowState.Minimized;
            ShowInTaskbar = _settings.DisplayInTaskbar;
        }

        private void MaximizeWindow()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void OnLyricsMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Lyrics.IsReadOnly = false;
        }

        private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow { Owner = this };
            settingsWindow.ShowDialog();
        }
    }
}
