using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Lyrixound.Converters
{
    /// <summary>
    /// Provides path to icon according to name.
    /// </summary>
    [ValueConversion(typeof(string), typeof(Uri))]
    public class NameToIconUriConverter : IValueConverter
    {
        public string IconsDirectory { get; set; }
        public string Extension { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return string.IsNullOrEmpty(value?.ToString())
                ? null
                : new Uri(Path.Combine(currentDirectory, IconsDirectory, $"{value}.{Extension}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
