using Lyrixound.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lyrixound.ViewModels;

public class PaywallWindowViewModel : BindableBase
{
    private readonly ILicenseService _licenseService;
    private readonly ILicenseAnalytics _licenseAnalytics;
    private bool _purchaseInProgress;
    private string _statusMessage;
    private Func<IntPtr> _getOwnerWindowHandle;

    public PaywallWindowViewModel(ILicenseService licenseService, ILicenseAnalytics licenseAnalytics)
    {
        _licenseService = licenseService;
        _licenseAnalytics = licenseAnalytics;

        UnlockProCommand = new DelegateCommand(async () => await UnlockProAsync(), () => !PurchaseInProgress)
            .ObservesProperty(() => PurchaseInProgress);
        MaybeLaterCommand = new DelegateCommand(() => RequestClose?.Invoke(false));
    }

    public event Action<bool> RequestClose;

    public string PriceDisplay => ProConstants.ProPriceDisplay;

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

    public ICommand UnlockProCommand { get; }

    public ICommand MaybeLaterCommand { get; }

    public void OnOpened(string source, Func<IntPtr> getOwnerWindowHandle)
    {
        _getOwnerWindowHandle = getOwnerWindowHandle;
        _licenseAnalytics.PaywallShown(source);
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
                RequestClose?.Invoke(true);
            }
        }
        finally
        {
            PurchaseInProgress = false;
        }
    }
}
