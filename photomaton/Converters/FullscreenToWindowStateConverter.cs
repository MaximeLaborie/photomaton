using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace photomaton.Converters
{
    public class FullscreenToWindowStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? WindowStyle.None : WindowStyle.SingleBorderWindow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (WindowStyle)value == WindowStyle.None;
        }
    }
}
