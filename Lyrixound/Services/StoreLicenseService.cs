using Lyrixound.Configuration;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace Lyrixound.Services;

public class StoreLicenseService : LicenseServiceBase
{
    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly ILicenseAnalytics _analytics;
    private StoreContext _storeContext;

    public StoreLicenseService(Settings settings, ILicenseAnalytics analytics) : base(settings)
    {
        _analytics = analytics;
    }

    public override async Task RefreshAsync()
    {
        var context = TryGetStoreContext();
        if (context == null)
        {
            return;
        }

        try
        {
            var license = await context.GetAppLicenseAsync();
            // AddOnLicenses keys are Store IDs; also match developer Product ID via InAppOfferToken.
            var isPro = license.AddOnLicenses.Any(pair =>
                pair.Value.IsActive &&
                (string.Equals(pair.Key, ProConstants.ProStoreId, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(pair.Value.InAppOfferToken, ProConstants.ProProductId, StringComparison.Ordinal)));
            SetIsPro(isPro);
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "Failed to refresh Store license; keeping cached entitlement");
            TouchLicenseCheck();
        }
    }

    public override async Task<LicenseResult> PurchaseAsync(IntPtr ownerWindowHandle)
    {
        _analytics.PurchaseStarted();

        var context = TryGetStoreContext();
        if (context == null)
        {
            _analytics.PurchaseFailed("store_unavailable");
            return LicenseResult.StoreUnavailable;
        }

        try
        {
            // Required for Desktop Bridge / WPF: bind purchase UI to an owner HWND.
            StoreContextWindowInitializer.Initialize(context, ownerWindowHandle);

            var result = await context.RequestPurchaseAsync(ProConstants.ProStoreId);
            switch (result.Status)
            {
                case StorePurchaseStatus.Succeeded:
                    await RefreshAsync();
                    _analytics.PurchaseSucceeded();
                    return LicenseResult.Success;

                case StorePurchaseStatus.AlreadyPurchased:
                    await RefreshAsync();
                    _analytics.PurchaseSucceeded();
                    return LicenseResult.AlreadyOwned;

                case StorePurchaseStatus.NotPurchased:
                    _analytics.PurchaseFailed("cancelled");
                    return LicenseResult.Cancelled;

                default:
                    _analytics.PurchaseFailed(result.ExtendedError?.Message ?? result.Status.ToString());
                    return LicenseResult.Failed;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Store purchase failed");
            _analytics.PurchaseFailed(ex.Message);
            return LicenseResult.Failed;
        }
    }

    public override async Task<LicenseResult> RestoreAsync()
    {
        var context = TryGetStoreContext();
        if (context == null)
        {
            return LicenseResult.StoreUnavailable;
        }

        try
        {
            await RefreshAsync();
            return IsPro ? LicenseResult.Success : LicenseResult.Failed;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Store restore failed");
            return LicenseResult.Failed;
        }
    }

    private StoreContext TryGetStoreContext()
    {
        try
        {
            return _storeContext ??= StoreContext.GetDefault();
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "Store context is not available");
            return null;
        }
    }
}
