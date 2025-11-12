using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CafeCrm.App.Converters;

public class BoolToTableColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var brushKey = value is bool hasCheck && hasCheck
            ? "TableWithCheckColor"
            : "TableWithoutCheckColor";

        return System.Windows.Application.Current.TryFindResource(brushKey) as Brush
            ?? new SolidColorBrush(Colors.SlateGray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
