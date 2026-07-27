using Lyrixound.Configuration;
using Lyrixound.Services;
using Prism.Commands;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Lyrixound.Views
{
    public partial class RatingReminderWindow : Window
    {
        private readonly Settings _settings;
        private readonly ILicenseService _licenseService;
        private readonly ILicenseAnalytics _licenseAnalytics;

        public ICommand MaybeLaterCommand { get; }
        public ICommand DontShowAgainCommand { get; }
        public ICommand RateNowCommand { get; }
        public ICommand SupportAuthorCommand { get; }

        public bool ShowSupportAuthor => !_licenseService.IsPro;

        public RatingReminderWindow(
            Settings settings,
            ILicenseService licenseService,
            ILicenseAnalytics licenseAnalytics)
        {
            InitializeComponent();
            _settings = settings;
            _licenseService = licenseService;
            _licenseAnalytics = licenseAnalytics;
            DataContext = this;

            MaybeLaterCommand = new DelegateCommand(OnMaybeLater);
            DontShowAgainCommand = new DelegateCommand(OnDontShowAgain);
            RateNowCommand = new DelegateCommand(OnRateNow);
            SupportAuthorCommand = new DelegateCommand(OnSupportAuthor);
        }

        private void OnMaybeLater()
        {
            DialogResult = false;
            Close();
        }

        private void OnDontShowAgain()
        {
            _settings.DontShowRatingReminder = true;
            _settings.Save();
            DialogResult = false;
            Close();
        }

        private void OnRateNow()
        {
            try
            {
                var storeUrl = $"ms-windows-store://review/?ProductId={ProConstants.StoreAppId}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = storeUrl,
                    UseShellExecute = true
                });

                _settings.DontShowRatingReminder = true;
                _settings.Save();
            }
            catch (Exception)
            {
                // If opening store fails, just close the dialog
            }

            DialogResult = true;
            Close();
        }

        private void OnSupportAuthor()
        {
            PaywallWindow.Show(this, _licenseService, _licenseAnalytics, "rating_reminder");

            if (_licenseService.IsPro)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
