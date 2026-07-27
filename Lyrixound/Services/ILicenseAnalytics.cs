namespace Lyrixound.Services;

public interface ILicenseAnalytics
{
    void PaywallShown(string source);

    void PurchaseStarted();

    void PurchaseSucceeded();

    void PurchaseFailed(string reason);
}
