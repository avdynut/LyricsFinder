using Lyrixound.Configuration;
using Lyrixound.Services;
using MaterialDesignThemes.Wpf;
using NLog;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lyrixound.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WM_NCHITTEST = 0x0084;
        private const int HTCLIENT = 1;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const double ResizeBorderThickness = 6;

        private static readonly Uri FreeIconUri = new Uri("pack://application:,,,/lyrics.ico");
        private static readonly Uri ProIconUri = new Uri("pack://application:,,,/lyrics-pro.ico");

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private SolidColorBrush _lyricsPanelBrush;
        private readonly WindowSettings _settings;
        private readonly LyricsSettings _lyricsSettings;
        private readonly ILicenseService _licenseService;
        private readonly ILicenseAnalytics _licenseAnalytics;
        private readonly ThemeService _themeService;
        private bool _isClickThrough;

        public MainWindow(
            WindowSettings settings,
            LyricsSettings lyricsSettings,
            ILicenseService licenseService,
            ILicenseAnalytics licenseAnalytics,
            ThemeService themeService)
        {
            _settings = settings;
            _lyricsSettings = lyricsSettings;
            _licenseService = licenseService;
            _licenseAnalytics = licenseAnalytics;
            _themeService = themeService;
            InitializeComponent();

            _licenseService.EntitlementChanged += OnEntitlementChanged;
            _themeService.ThemeChanged += OnThemeChanged;
            Loaded += (_, _) => ApplyProUi();
        }

        private void OnEntitlementChanged(object sender, EventArgs e) => InvokeOnDispatcher(ApplyProUi);

        private void OnThemeChanged(object sender, EventArgs e)
        {
            InvokeOnDispatcher(() =>
            {
                ApplyLyricsPanelBrush();
                ApplyThemeChrome();
            });
        }

        private void InvokeOnDispatcher(Action action)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.Invoke(action);
            }
        }

        private void ApplyProUi()
        {
            ApplyProBranding();
            ApplyThemeChrome();
        }

        private void ApplyProBranding()
        {
            var isPro = _licenseService.IsPro;
            var uri = isPro ? ProIconUri : FreeIconUri;
            var image = new BitmapImage(uri);

            Title = isPro ? "LyrixoundPro" : App.AppName;
            Icon = image;
            TrayIcon.IconSource = image;
            if (AppIconImage != null)
            {
                AppIconImage.Source = image;
            }

            if (TrayTooltipLabel != null)
            {
                TrayTooltipLabel.Content = isPro
                    ? $"LyrixoundPro v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}"
                    : App.AppNameWithVersion;
            }

            if (SupportAuthorMenuItem != null)
            {
                SupportAuthorMenuItem.Visibility = isPro
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        private void OnSyncedLyricsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScrollToCenterCurrentLine();
        }

        private void OnSyncedLyricsListSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScrollToCenterCurrentLine();
        }

        private void ScrollToCenterCurrentLine()
        {
            if (SyncedLyricsList?.SelectedItem == null)
                return;

            var selectedIndex = SyncedLyricsList.SelectedIndex;
            if (selectedIndex < 0)
                return;

            // Get the ScrollViewer from the ListBox
            var scrollViewer = FindVisualChild<ScrollViewer>(SyncedLyricsList);
            if (scrollViewer == null)
                return;

            // Get the container for the selected item
            var container = SyncedLyricsList.ItemContainerGenerator.ContainerFromIndex(selectedIndex) as ListBoxItem;
            if (container != null)
            {
                // Get the position of the item relative to the ListBox
                var transform = container.TransformToAncestor(SyncedLyricsList);
                var position = transform.Transform(new Point(0, 0));

                // Calculate offset to center the item
                var itemHeight = container.ActualHeight;
                var viewportHeight = scrollViewer.ViewportHeight;
                var currentOffset = scrollViewer.VerticalOffset;

                // Target: center of viewport
                var targetOffset = currentOffset + position.Y - (viewportHeight / 2) + (itemHeight / 2);

                scrollViewer.ScrollToVerticalOffset(Math.Max(0, targetOffset));
            }
            else
            {
                // Fallback: estimate scroll position if container not generated
                var itemCount = SyncedLyricsList.Items.Count;
                if (itemCount > 0 && scrollViewer.ExtentHeight > 0)
                {
                    var estimatedItemHeight = scrollViewer.ExtentHeight / itemCount;
                    var targetOffset = (selectedIndex * estimatedItemHeight) - (scrollViewer.ViewportHeight / 2);
                    scrollViewer.ScrollToVerticalOffset(Math.Max(0, targetOffset));
                }
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var result = FindVisualChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            RestoreWindowParameters();
            ApplyLyricsPanelBrush();
        }

        private void ApplyLyricsPanelBrush()
        {
            var color = TryFindResource("MaterialDesignPaper") is SolidColorBrush paper
                ? paper.Color
                : (Color)ColorConverter.ConvertFromString("#FFFAFAFA");
            var opacity = _lyricsPanelBrush?.Opacity ?? 1;
            _lyricsPanelBrush = new SolidColorBrush(color) { Opacity = opacity };
            if (LyricsPanel != null && TextSettings != null)
            {
                LyricsPanel.Background = TextSettings.Background = _lyricsPanelBrush;
            }
        }

        private void ApplyThemeChrome()
        {
            if (ThemeButton == null)
            {
                return;
            }

            var isDark = _themeService.IsDarkThemeActive;
            ThemeButton.Content = new PackIcon
            {
                Kind = isDark ? PackIconKind.WhiteBalanceSunny : PackIconKind.WeatherNight
            };
            ThemeButton.ToolTip = isDark
                ? "Light mode"
                : _licenseService.IsPro ? "Dark mode" : "Dark mode (Pro)";
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WM_NCHITTEST || _isClickThrough || WindowState != WindowState.Normal)
                return IntPtr.Zero;

            var packed = lParam.ToInt64();
            var mouse = PointFromScreen(new Point((short)packed, (short)(packed >> 16)));
            var onLeft = mouse.X <= ResizeBorderThickness;
            var onRight = mouse.X >= ActualWidth - ResizeBorderThickness;
            var onTop = mouse.Y <= ResizeBorderThickness;
            var onBottom = mouse.Y >= ActualHeight - ResizeBorderThickness;

            var hit = (onTop, onBottom, onLeft, onRight) switch
            {
                (true, _, true, _) => HTTOPLEFT,
                (true, _, _, true) => HTTOPRIGHT,
                (true, _, _, _) => HTTOP,
                (_, true, true, _) => HTBOTTOMLEFT,
                (_, true, _, true) => HTBOTTOMRIGHT,
                (_, true, _, _) => HTBOTTOM,
                (_, _, true, _) => HTLEFT,
                (_, _, _, true) => HTRIGHT,
                _ => HTCLIENT
            };

            if (hit == HTCLIENT)
                return IntPtr.Zero;

            handled = true;
            return (IntPtr)hit;
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

            if (_isClickThrough)
                return;

            Lyrics.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            ScrollViewer.SetVerticalScrollBarVisibility(SyncedLyricsList, ScrollBarVisibility.Auto);
            LyricsPanel.Background.Opacity = TextSettings.Background.Opacity = 1;
            TopPanel.Visibility = Visibility.Visible;
            TextSettings.Visibility = Visibility.Visible;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            ApplyDeactivatedVisuals();
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
            _licenseService.EntitlementChanged -= OnEntitlementChanged;
            _themeService.ThemeChanged -= OnThemeChanged;

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

        private void OnClickThroughToggle(object sender, RoutedEventArgs e)
        {
            _isClickThrough = !_isClickThrough;

            var hwnd = new WindowInteropHelper(this).Handle;
            var extStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

            if (_isClickThrough)
            {
                SetWindowLong(hwnd, GWL_EXSTYLE, extStyle | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE);
                ApplyDeactivatedVisuals();
                TopPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                SetWindowLong(hwnd, GWL_EXSTYLE, extStyle & ~WS_EX_TRANSPARENT & ~WS_EX_NOACTIVATE);
                TopPanel.Visibility = Visibility.Visible;
            }
        }

        private void ApplyDeactivatedVisuals()
        {
            Lyrics.IsReadOnly = true;
            Lyrics.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            ScrollViewer.SetVerticalScrollBarVisibility(SyncedLyricsList, ScrollBarVisibility.Hidden);
            LyricsPanel.Background.Opacity = TextSettings.Background.Opacity = _lyricsSettings.FloatingBackgroundOpacity;
            TextSettings.IsExpanded = false;
            TextSettings.Visibility = Visibility.Collapsed;
        }

        private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow { Owner = this };
            settingsWindow.ShowDialog();
            ApplyProUi();
        }

        private void OnThemeButtonClick(object sender, RoutedEventArgs e)
        {
            if (!_licenseService.IsPro)
            {
                PaywallWindow.Show(this, _licenseService, _licenseAnalytics, "theme");
                ApplyProUi();
                return;
            }

            _themeService.ToggleDarkTheme();
        }

        private void OnSupportAuthorClick(object sender, RoutedEventArgs e)
        {
            PaywallWindow.Show(this, _licenseService, _licenseAnalytics, "tray");
            ApplyProUi();
        }
    }
}
