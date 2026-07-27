using System;
using System.Threading.Tasks;

namespace Lyrixound.Services;

public interface ILicenseService : IProEntitlementService
{
    Task RefreshAsync();

    /// <param name="ownerWindowHandle">
    /// HWND of the owner window for the Store purchase dialog (required for packaged WPF).
    /// </param>
    Task<LicenseResult> PurchaseAsync(IntPtr ownerWindowHandle);

    Task<LicenseResult> RestoreAsync();
}
