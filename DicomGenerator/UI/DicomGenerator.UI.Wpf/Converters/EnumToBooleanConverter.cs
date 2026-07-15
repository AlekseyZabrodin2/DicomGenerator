using System;
using System.Globalization;
using System.Windows.Data;

namespace DicomGenerator.UI.Wpf.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            if (Enum.TryParse(value.GetType(), parameter.ToString(), out var enumValue))
            {
                return value.Equals(enumValue);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
                return parameter;

            return Binding.DoNothing;
        }
    }
}
