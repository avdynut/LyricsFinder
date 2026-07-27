using Lyrixound.Services;
using Lyrixound.ViewModels;
using System;
using System.Windows;
using System.Windows.Interop;

namespace Lyrixound.Views;

public partial class PaywallWindow : Window
{
    private readonly PaywallWindowViewModel _viewModel;

    public PaywallWindow(ILicenseService licenseService, ILicenseAnalytics licenseAnalytics)
    {
        InitializeComponent();
        _viewModel = new PaywallWindowViewModel(licenseService, licenseAnalytics);
        _viewModel.RequestClose += OnRequestClose;
        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    public string Source { get; set; } = "unknown";

    public static bool? Show(
        Window owner,
        ILicenseService licenseService,
        ILicenseAnalytics licenseAnalytics,
        string source)
    {
        var paywall = new PaywallWindow(licenseService, licenseAnalytics)
        {
            Owner = owner,
            Source = source
        };
        return paywall.ShowDialog();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnOpened(Source, GetOwnerWindowHandle);
    }

    private IntPtr GetOwnerWindowHandle()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd != IntPtr.Zero)
        {
            return hwnd;
        }

        if (Owner != null)
        {
            hwnd = new WindowInteropHelper(Owner).Handle;
            if (hwnd != IntPtr.Zero)
            {
                return hwnd;
            }
        }

        return Application.Current.MainWindow is Window main
            ? new WindowInteropHelper(main).Handle
            : IntPtr.Zero;
    }

    private void OnRequestClose(bool purchased)
    {
        DialogResult = purchased;
        Close();
    }
}
