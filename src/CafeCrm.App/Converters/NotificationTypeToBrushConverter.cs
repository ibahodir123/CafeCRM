using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CafeCrm.Application.Abstractions.Notifications;

namespace CafeCrm.App.Converters;

public class NotificationTypeToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var resourceKey = value switch
        {
            NotificationType.Success => "SuccessColor",
            NotificationType.Error => "ErrorColor",
            NotificationType.Warning => "WarningColor",
            NotificationType.Info => "InfoColor",
            _ => "InfoColor"
        };

        return System.Windows.Application.Current.TryFindResource(resourceKey) as Brush
            ?? new SolidColorBrush(Colors.DimGray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
