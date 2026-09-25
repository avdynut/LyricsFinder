using System;
using System.Globalization;
using System.Windows.Data;

namespace Lyrixound.Converters;

public class ByteToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is byte alpha)
            return alpha / 255d;

        return 1d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
