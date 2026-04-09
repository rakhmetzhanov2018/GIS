using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GIS.Classes.Converters
{
    public class ColorToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
                return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            return "#00000000";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && hex.StartsWith("#"))
            {
                try
                {
                    return (Color)ColorConverter.ConvertFromString(hex);
                }
                catch
                {
                    return Colors.Black;
                }
            }
            return Colors.Black;
        }
    }
}