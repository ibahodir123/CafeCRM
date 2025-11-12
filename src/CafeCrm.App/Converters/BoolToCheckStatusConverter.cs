using System;
using System.Globalization;
using System.Windows.Data;

namespace CafeCrm.App.Converters;

public class BoolToCheckStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var hasCheck = value is bool flag && flag;
        return hasCheck ? "✅ Чек открыт" : "⏳ Ожидание чека";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
