using System.Globalization;
using UraniumUI.Icons.MaterialSymbols;

namespace PantryPal.Mobile.Converters;

public class FavoriteIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isFavorite)
        {
            return isFavorite ? MaterialOutlined.Star_half : MaterialOutlined.Star;
        }

        return MaterialOutlined.Star;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

