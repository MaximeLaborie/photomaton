using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace photomaton.Converters
{
    public class FullscreenToWindowStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? WindowState.Maximized : WindowState.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (WindowState)value == WindowState.Maximized;
        }
    }
}
