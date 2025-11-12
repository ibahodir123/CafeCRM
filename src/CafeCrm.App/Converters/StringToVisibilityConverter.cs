using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CafeCrm.App.Converters;

public class StringToVisibilityConverter : IValueConverter
{
    public bool CollapseWhenEmpty { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var hasText = value is string text && !string.IsNullOrWhiteSpace(text);
        if (hasText)
        {
            return Visibility.Visible;
        }

        return CollapseWhenEmpty ? Visibility.Collapsed : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
