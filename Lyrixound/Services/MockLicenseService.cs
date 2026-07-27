using Lyrixound.Configuration;
using System;
using System.Threading.Tasks;

namespace Lyrixound.Services;

public interface IMockLicenseService : ILicenseService
{
    bool SimulatePro { get; set; }
}

public class MockLicenseService : LicenseServiceBase, IMockLicenseService
{
    private readonly Settings _settings;

    public MockLicenseService(Settings settings) : base(settings)
    {
        _settings = settings;
        SetIsPro(settings.DevSimulatePro);
    }

    public bool SimulatePro
    {
        get => _settings.DevSimulatePro;
        set
        {
            _settings.DevSimulatePro = value;
            _settings.Save();
            SetIsPro(value);
        }
    }

    public override Task RefreshAsync()
    {
        SetIsPro(_settings.DevSimulatePro);
        TouchLicenseCheck();
        return Task.CompletedTask;
    }

    public override Task<LicenseResult> PurchaseAsync(IntPtr ownerWindowHandle)
    {
        SimulatePro = true;
        return Task.FromResult(LicenseResult.Success);
    }

    public override Task<LicenseResult> RestoreAsync()
    {
        SetIsPro(_settings.DevSimulatePro);
        return Task.FromResult(_settings.DevSimulatePro ? LicenseResult.Success : LicenseResult.Failed);
    }
}
