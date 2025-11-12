using System;
using System.Globalization;
using System.Windows.Data;

namespace CafeCrm.App.Converters;

public class BoolToVisitTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool isTakeaway && isTakeaway
            ? "Навынос"
            : "В зале";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
