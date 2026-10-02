using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Lyrixound.Views
{
    public partial class RepeatStepper : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(RepeatStepper),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
            nameof(Step),
            typeof(double),
            typeof(RepeatStepper),
            new PropertyMetadata(0.1));

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(RepeatStepper),
            new PropertyMetadata(-10.0));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(RepeatStepper),
            new PropertyMetadata(10.0));

        public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register(
            nameof(DecimalPlaces),
            typeof(int),
            typeof(RepeatStepper),
            new PropertyMetadata(1, OnValueChanged));

        public static readonly DependencyProperty HintProperty = DependencyProperty.Register(
            nameof(Hint),
            typeof(string),
            typeof(RepeatStepper));

        public static readonly DependencyProperty DecreaseToolTipProperty = DependencyProperty.Register(
            nameof(DecreaseToolTip),
            typeof(string),
            typeof(RepeatStepper));

        public static readonly DependencyProperty IncreaseToolTipProperty = DependencyProperty.Register(
            nameof(IncreaseToolTip),
            typeof(string),
            typeof(RepeatStepper));

        public static readonly DependencyProperty ValueToolTipProperty = DependencyProperty.Register(
            nameof(ValueToolTip),
            typeof(string),
            typeof(RepeatStepper));

        public RepeatStepper()
        {
            InitializeComponent();
            Loaded += (_, _) => ShowValue();
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double Step
        {
            get => (double)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public int DecimalPlaces
        {
            get => (int)GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, value);
        }

        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        public string DecreaseToolTip
        {
            get => (string)GetValue(DecreaseToolTipProperty);
            set => SetValue(DecreaseToolTipProperty, value);
        }

        public string IncreaseToolTip
        {
            get => (string)GetValue(IncreaseToolTipProperty);
            set => SetValue(IncreaseToolTipProperty, value);
        }

        public string ValueToolTip
        {
            get => (string)GetValue(ValueToolTipProperty);
            set => SetValue(ValueToolTipProperty, value);
        }

        private void OnDecreaseClick(object sender, RoutedEventArgs e) => Adjust(-Step);

        private void OnIncreaseClick(object sender, RoutedEventArgs e) => Adjust(Step);

        private void OnValueLostFocus(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(ValueBox.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out var number))
                Value = Round(number);

            ShowValue();
        }

        private void Adjust(double delta) => Value = Round(Value + delta);

        private double Round(double value) =>
            Math.Round(Math.Clamp(value, Minimum, Maximum), DecimalPlaces);

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
            ((RepeatStepper)sender).ShowValue();

        private void ShowValue()
        {
            if (!IsLoaded || ValueBox.IsKeyboardFocusWithin)
                return;

            ValueBox.Text = Value.ToString("F" + DecimalPlaces, CultureInfo.CurrentCulture);
        }
    }
}
