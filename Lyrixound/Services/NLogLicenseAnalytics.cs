using NLog;

namespace Lyrixound.Services;

public class NLogLicenseAnalytics : ILicenseAnalytics
{
    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    public void PaywallShown(string source) =>
        _logger.Info("License analytics: paywall_shown source={Source}", source);

    public void PurchaseStarted() =>
        _logger.Info("License analytics: purchase_started");

    public void PurchaseSucceeded() =>
        _logger.Info("License analytics: purchase_succeeded");

    public void PurchaseFailed(string reason) =>
        _logger.Warn("License analytics: purchase_failed reason={Reason}", reason);
}
