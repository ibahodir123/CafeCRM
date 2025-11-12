using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CafeCrm.App.Converters;

public class InverseStringToVisibilityConverter : IValueConverter
{
    public bool CollapseWhenVisible { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var hasText = value is string text && !string.IsNullOrWhiteSpace(text);
        if (hasText)
        {
            return CollapseWhenVisible ? Visibility.Collapsed : Visibility.Hidden;
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
