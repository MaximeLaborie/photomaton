using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace photomaton.Converters
{
    public abstract class BaseBooleanConverter<T> : IValueConverter
    {
        public abstract T ValueIfTrue {  get; }

        public abstract T ValueIfFalse {  get; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ValueIfTrue : ValueIfFalse;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return EqualityComparer<T>.Default.Equals((T)value, ValueIfTrue);
        }
    }

    public class InverseBooleanToVisibilityConverter : BaseBooleanConverter<Visibility>
    {
        public override Visibility ValueIfTrue { get { return Visibility.Collapsed; } }

        public override Visibility ValueIfFalse { get { return Visibility.Visible; } }
    }

    public class InverseBoolToNoMouseConverter : BaseBooleanConverter<Cursor>
    {
        public override Cursor ValueIfTrue { get { return Cursors.None; } }

        public override Cursor ValueIfFalse { get { return null; } }
    }
}
