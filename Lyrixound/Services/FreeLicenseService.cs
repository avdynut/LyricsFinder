using Lyrixound.Configuration;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Lyrixound.Services;

public class FreeLicenseService : LicenseServiceBase
{
    public FreeLicenseService(Settings settings) : base(settings)
    {
    }

    public override Task RefreshAsync()
    {
        SetIsPro(false);
        TouchLicenseCheck();
        return Task.CompletedTask;
    }

    public override Task<LicenseResult> PurchaseAsync(IntPtr ownerWindowHandle)
    {
        OpenStorePage();
        return Task.FromResult(LicenseResult.StoreUnavailable);
    }

    public override Task<LicenseResult> RestoreAsync()
    {
        OpenStorePage();
        return Task.FromResult(LicenseResult.StoreUnavailable);
    }

    private static void OpenStorePage()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = $"ms-windows-store://pdp/?ProductId={ProConstants.StoreAppId}",
                UseShellExecute = true
            });
        }
        catch
        {
            // ignored
        }
    }
}
