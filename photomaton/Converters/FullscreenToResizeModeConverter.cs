using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace photomaton.Converters
{
    public class FullscreenToResizeModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ResizeMode.NoResize : ResizeMode.CanResize;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (ResizeMode)value == ResizeMode.NoResize;
        }
    }
}
