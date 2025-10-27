using Prism.Mvvm;
using System;

namespace Lyrixound.ViewModels
{
    public class LyricLineViewModel : BindableBase
    {
        private bool _isActive;

        public TimeSpan Timestamp { get; set; }
        public string Text { get; set; }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public LyricLineViewModel(TimeSpan timestamp, string text)
        {
            Timestamp = timestamp;
            Text = text;
        }
    }
}

