using Lyrixound.Configuration;
using System;
using System.Threading.Tasks;

namespace Lyrixound.Services;

public abstract class LicenseServiceBase : ILicenseService
{
    private readonly Settings _settings;
    private bool _isPro;

    protected LicenseServiceBase(Settings settings)
    {
        _settings = settings;
        _isPro = settings.CachedIsPro ?? false;
    }

    public bool IsPro => _isPro;

    public event EventHandler EntitlementChanged;

    public abstract Task RefreshAsync();

    public abstract Task<LicenseResult> PurchaseAsync(IntPtr ownerWindowHandle);

    public abstract Task<LicenseResult> RestoreAsync();

    protected void SetIsPro(bool isPro)
    {
        if (_isPro == isPro)
        {
            return;
        }

        _isPro = isPro;
        _settings.CachedIsPro = isPro;
        _settings.LicenseCheckedAt = DateTime.UtcNow;
        _settings.Save();
        EntitlementChanged?.Invoke(this, EventArgs.Empty);
    }

    protected void TouchLicenseCheck()
    {
        _settings.LicenseCheckedAt = DateTime.UtcNow;
        _settings.Save();
    }
}
