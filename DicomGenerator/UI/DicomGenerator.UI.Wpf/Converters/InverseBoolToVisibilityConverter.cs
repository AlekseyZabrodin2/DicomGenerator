using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DicomGenerator.UI.Wpf.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Visibility to set when bool value is true or null.
        /// </summary>
        public Visibility NotVisible { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is null || (bool)value) ? NotVisible : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }
}
