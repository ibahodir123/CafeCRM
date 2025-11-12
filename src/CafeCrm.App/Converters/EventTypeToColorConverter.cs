using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CafeCrm.App.Converters;

public class EventTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var eventType = value as string;
        var resourceKey = eventType switch
        {
            "POS_CHECK_ASSIGNED" => "SuccessColor",
            "POS_CHECK_NO_VISIT" => "WarningColor",
            "POS_ERROR" => "ErrorColor",
            _ => "InfoColor"
        };

        return System.Windows.Application.Current?.TryFindResource(resourceKey) ?? Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
