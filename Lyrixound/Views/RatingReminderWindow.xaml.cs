using Lyrixound.Configuration;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Lyrixound.Views
{
    /// <summary>
    /// Interaction logic for RatingReminderWindow.xaml
    /// </summary>
    public partial class RatingReminderWindow : Window
    {
        private const string StoreAppId = "9MSQSDJH510N";
        private readonly Settings _settings;

        public ICommand MaybeLaterCommand { get; }
        public ICommand DontShowAgainCommand { get; }
        public ICommand RateNowCommand { get; }

        public RatingReminderWindow(Settings settings)
        {
            InitializeComponent();
            _settings = settings;
            DataContext = this;

            MaybeLaterCommand = new DelegateCommand(OnMaybeLater);
            DontShowAgainCommand = new DelegateCommand(OnDontShowAgain);
            RateNowCommand = new DelegateCommand(OnRateNow);
        }

        private void OnMaybeLater()
        {
            // Just close the window, will show again after 3 more launches
            DialogResult = false;
            Close();
        }

        private void OnDontShowAgain()
        {
            // Mark to never show again
            _settings.DontShowRatingReminder = true;
            _settings.Save();
            DialogResult = false;
            Close();
        }

        private void OnRateNow()
        {
            // Open the Store page for rating
            try
            {
                var storeUrl = $"ms-windows-store://review/?ProductId={StoreAppId}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = storeUrl,
                    UseShellExecute = true
                });
                
                // Mark as done so we don't show again
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
    }
}

