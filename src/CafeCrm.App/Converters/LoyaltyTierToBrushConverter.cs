using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using CafeCrm.Domain.Enums;
using System.Windows;

namespace CafeCrm.App.Converters;

public class LoyaltyTierToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var tier = value as LoyaltyTier? ?? (value is LoyaltyTier t ? t : LoyaltyTier.Standard);
        var resourceKey = tier switch
        {
            LoyaltyTier.Gold or LoyaltyTier.Platinum => "VIPCustomerColor",
            LoyaltyTier.Silver => "RegularCustomerColor",
            _ => "NewCustomerColor"
        };

        return System.Windows.Application.Current?.TryFindResource(resourceKey) as Brush
            ?? new SolidColorBrush(Color.FromRgb(44, 90, 160));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
