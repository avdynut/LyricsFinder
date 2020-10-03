using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Lyrixator.Converters
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
            return new Uri(Path.Combine(Environment.CurrentDirectory, IconsDirectory, $"{value}.{Extension}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
