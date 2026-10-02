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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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

        private static readonly Duration ShowDuration = TimeSpan.FromSeconds(0.3);
        private static readonly Duration FadeDuration = TimeSpan.FromSeconds(0.5);

        private static readonly Uri FreeIconUri = new Uri("pack://application:,,,/lyrics.ico");
        private static readonly Uri ProIconUri = new Uri("pack://application:,,,/lyrics-pro.ico");

        [LibraryImport("user32.dll")]
        private static partial int GetWindowLong(IntPtr hwnd, int index);

        [LibraryImport("user32.dll")]
        private static partial int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private SolidColorBrush _lyricsPanelBrush;
        private readonly WindowSettings _settings;
        private readonly LyricsSettings _lyricsSettings;
        private readonly ILicenseService _licenseService;
        private readonly ILicenseAnalytics _licenseAnalytics;
        private readonly ThemeService _themeService;
        private bool _isClickThrough;
        private bool _titleBarHovered;
        private bool _searchPanelDismissed;
        private LyricsSettingsWindow _lyricsSettingsWindow;

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
            Loaded += (_, _) =>
            {
                ApplyProUi();
                if (DataContext is ViewModels.MainWindowViewModel viewModel)
                {
                    viewModel.PropertyChanged += OnViewModelPropertyChanged;
                    viewModel.Track.PropertyChanged += OnTrackPropertyChanged;
                }

                UpdateSearchPanelForContent();
            };
            ContentRoot.SizeChanged += (_, e) =>
            {
                ContentRoot.Clip = new RectangleGeometry(new Rect(e.NewSize), 8, 8);
            };
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
            if (ContentRoot != null)
            {
                ContentRoot.Background = _lyricsPanelBrush;
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
                : _licenseService.IsPro ? "Dark mode" : "Dark mode 👑";
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
            return hit;
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
            UpdateChrome();
            UpdateSearchPanelForContent();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            Lyrics.IsReadOnly = true;
            UpdateChrome();
            UpdateSearchPanelForContent();
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
            if (DataContext is ViewModels.MainWindowViewModel viewModel)
            {
                viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                viewModel.Track.PropertyChanged -= OnTrackPropertyChanged;
            }

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
                Lyrics.IsReadOnly = true;
                SetSearchPanelOpen(false);
                UpdateChrome(animate: false);
            }
            else
            {
                SetWindowLong(hwnd, GWL_EXSTYLE, extStyle & ~WS_EX_TRANSPARENT & ~WS_EX_NOACTIVATE);
                UpdateChrome();
                UpdateSearchPanelForContent();
            }
        }

        private void OnTitleBarMouseEnter(object sender, MouseEventArgs e)
        {
            _titleBarHovered = true;
            UpdateChrome();
        }

        private void OnTitleBarMouseLeave(object sender, MouseEventArgs e)
        {
            _titleBarHovered = false;
            UpdateChrome();
        }

        private void OnLyricsChromeAreaMouseEnter(object sender, MouseEventArgs e)
        {
            Dispatcher.BeginInvoke(() => UpdateChrome(), DispatcherPriority.Input);
        }

        private void OnLyricsChromeAreaMouseLeave(object sender, MouseEventArgs e)
        {
            Dispatcher.BeginInvoke(() => UpdateChrome(), DispatcherPriority.Input);
        }

        private void UpdateChrome(bool animate = true)
        {
            var allowChrome = !_isClickThrough && IsActive;
            SetChromeElementVisible(TitleBarButtons, allowChrome && _titleBarHovered, animate);
            var overLyricsChrome = LyricsPanel.IsMouseOver || TrackHeader.IsMouseOver;
            SetChromeElementVisible(LyricsChrome, allowChrome && overLyricsChrome, animate);
            var hasSyncedLyrics = DataContext is ViewModels.MainWindowViewModel viewModel && viewModel.Track.HasSyncedLyrics;
            SetChromeElementVisible(TimeOffsetBar, allowChrome && LyricsPanel.IsMouseOver && hasSyncedLyrics, animate);

            var engaged = allowChrome && IsActive;
            var scrollBars = engaged ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
            Lyrics.VerticalScrollBarVisibility = scrollBars;
            ScrollViewer.SetVerticalScrollBarVisibility(SyncedLyricsList, scrollBars);
            ContentRoot.Background.Opacity = engaged ? 1 : _lyricsSettings.FloatingBackgroundOpacity;
        }

        private static void SetChromeElementVisible(UIElement element, bool visible, bool animate)
        {
            element.IsHitTestVisible = visible;
            var target = visible ? 1.0 : 0.0;
            if (animate)
            {
                element.BeginAnimation(OpacityProperty, new DoubleAnimation(target, visible ? ShowDuration : FadeDuration));
            }
            else
            {
                element.BeginAnimation(OpacityProperty, null);
                element.Opacity = target;
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModels.MainWindowViewModel.TrackSession))
            {
                _searchPanelDismissed = false;
            }

            if (e.PropertyName is nameof(ViewModels.MainWindowViewModel.SearchInProgress)
                or nameof(ViewModels.MainWindowViewModel.TrackSession))
            {
                UpdateSearchPanelForContent();
            }
        }

        private void OnTrackPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModels.TrackViewModel.Lyrics))
            {
                UpdateSearchPanelForContent();
            }

            if (e.PropertyName == nameof(ViewModels.TrackViewModel.HasSyncedLyrics))
            {
                UpdateChrome();
            }
        }

        private void UpdateSearchPanelForContent()
        {
            if (!Dispatcher.CheckAccess())
            {
                InvokeOnDispatcher(UpdateSearchPanelForContent);
                return;
            }

            if (!IsActive)
            {
                _searchPanelDismissed = false;
                SetSearchPanelOpen(false);
                return;
            }

            if (_isClickThrough || DataContext is not ViewModels.MainWindowViewModel viewModel)
            {
                return;
            }

            if (viewModel.SearchInProgress)
            {
                return;
            }

            if (viewModel.Track.Lyrics?.Text?.Length > 0)
            {
                _searchPanelDismissed = false;
                SetSearchPanelOpen(false);
                return;
            }

            if (_searchPanelDismissed)
                return;

            SetSearchPanelOpen(true);
        }

        private void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            var open = SearchPanel.Visibility != Visibility.Visible;
            _searchPanelDismissed = !open;
            SetSearchPanelOpen(open);
            if (open)
            {
                Activate();
                Dispatcher.BeginInvoke(() => SearchTitleBox.Focus(), DispatcherPriority.Input);
            }

            if (DataContext is ViewModels.MainWindowViewModel viewModel && !(viewModel.Track.Lyrics?.Text?.Length > 0))
            {
                _ = viewModel.DetectCurrentTrackAsync();
            }
        }

        private void SetSearchPanelOpen(bool isOpen)
        {
            SearchPanel.Visibility = isOpen ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnSearchPanelKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;

            _searchPanelDismissed = true;
            SetSearchPanelOpen(false);
            e.Handled = true;
        }

        private void OnTimeOffsetDecreaseClick(object sender, RoutedEventArgs e) => AdjustTimeOffset(-0.1);

        private void OnTimeOffsetIncreaseClick(object sender, RoutedEventArgs e) => AdjustTimeOffset(0.1);

        private void AdjustTimeOffset(double delta)
        {
            if (DataContext is not ViewModels.MainWindowViewModel viewModel)
                return;

            var settings = viewModel.LyricsSettings;
            settings.TimeOffsetSeconds = Math.Round(settings.TimeOffsetSeconds + delta, 1);
        }

        private void OnLyricsSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            if (_lyricsSettingsWindow != null)
            {
                _lyricsSettingsWindow.Activate();
                return;
            }

            if (DataContext is not ViewModels.MainWindowViewModel { LyricsSettings: var lyricsSettings })
                return;

            _lyricsSettingsWindow = new LyricsSettingsWindow(_themeService)
            {
                Owner = this,
                DataContext = lyricsSettings
            };
            lyricsSettings.PropertyChanged += OnLyricsSettingsPropertyChanged;
            _lyricsSettingsWindow.Closed += (_, _) =>
            {
                lyricsSettings.PropertyChanged -= OnLyricsSettingsPropertyChanged;
                _lyricsSettingsWindow = null;
            };
            _lyricsSettingsWindow.Show();
        }

        private void OnLyricsSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LyricsSettings.FloatingBackgroundOpacity))
            {
                UpdateChrome(animate: false);
            }
        }

        private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_themeService) { Owner = this };
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
