using Lyrixound.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lyrixound.ViewModels;

public class PaywallWindowViewModel : BindableBase
{
    private readonly ILicenseService _licenseService;
    private readonly ILicenseAnalytics _licenseAnalytics;
    private bool _purchaseInProgress;
    private bool _purchaseCompleted;
    private string _statusMessage;
    private Func<IntPtr> _getOwnerWindowHandle;

    public PaywallWindowViewModel(ILicenseService licenseService, ILicenseAnalytics licenseAnalytics)
    {
        _licenseService = licenseService;
        _licenseAnalytics = licenseAnalytics;

        UnlockProCommand = new DelegateCommand(async () => await UnlockProAsync(), () => !PurchaseInProgress && !PurchaseCompleted)
            .ObservesProperty(() => PurchaseInProgress)
            .ObservesProperty(() => PurchaseCompleted);
        MaybeLaterCommand = new DelegateCommand(() => RequestClose?.Invoke(false));
        CloseCommand = new DelegateCommand(() => RequestClose?.Invoke(true));
        OpenSupportEmailCommand = new DelegateCommand(OpenSupportEmail);
    }

    public event Action<bool> RequestClose;

    public string PriceDisplay => ProConstants.ProPriceDisplay;

    public string SupportEmail => ProConstants.SupportEmail;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool PurchaseInProgress
    {
        get => _purchaseInProgress;
        private set => SetProperty(ref _purchaseInProgress, value);
    }

    public bool PurchaseCompleted
    {
        get => _purchaseCompleted;
        private set => SetProperty(ref _purchaseCompleted, value);
    }

    public ICommand UnlockProCommand { get; }

    public ICommand MaybeLaterCommand { get; }

    public ICommand CloseCommand { get; }

    public ICommand OpenSupportEmailCommand { get; }

    public void OnOpened(string source, Func<IntPtr> getOwnerWindowHandle)
    {
        _getOwnerWindowHandle = getOwnerWindowHandle;
        _licenseAnalytics.PaywallShown(source);
        PurchaseCompleted = false;
        StatusMessage = null;
    }

    private async Task UnlockProAsync()
    {
        PurchaseInProgress = true;
        StatusMessage = null;

        try
        {
            var hwnd = _getOwnerWindowHandle?.Invoke() ?? IntPtr.Zero;
            var result = await _licenseService.PurchaseAsync(hwnd);
            StatusMessage = result switch
            {
                LicenseResult.Success => "Thank you for supporting Lyrixound!",
                LicenseResult.AlreadyOwned => "You already support Lyrixound on this account. Thank you!",
                LicenseResult.Cancelled => null,
                LicenseResult.StoreUnavailable => "Microsoft Store is not available in this build.",
                _ => "Purchase could not be completed. Check logs or Partner Center add-on."
            };

            if (result is LicenseResult.Success or LicenseResult.AlreadyOwned)
            {
                PurchaseCompleted = true;
            }
        }
        finally
        {
            PurchaseInProgress = false;
        }
    }

    private static void OpenSupportEmail()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = $"mailto:{ProConstants.SupportEmail}",
                UseShellExecute = true
            });
        }
        catch
        {
            // ignored
        }
    }
}
