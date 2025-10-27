using System.Globalization;
using UraniumUI.Icons.MaterialSymbols;

namespace PantryPal.Mobile.Converters;

public class FavoriteIconConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length > 0 && values[0] is bool isFavorite)
        {
            return isFavorite ? MaterialOutlined.Star : MaterialOutlined.Grade;
        }

        return MaterialOutlined.Grade;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

