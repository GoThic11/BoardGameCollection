using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BoardGameCollection.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visibility = true;
            if (parameter != null && parameter.ToString().ToLower() == "inverted")
            {
                visibility = false;
            }

            if (value is bool boolValue)
            {
                return boolValue == visibility ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}