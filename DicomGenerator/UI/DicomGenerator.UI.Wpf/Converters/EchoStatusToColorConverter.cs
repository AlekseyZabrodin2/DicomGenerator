using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DicomGenerator.UI.Wpf.Converters
{
    public class EchoStatusToColorConverter : IValueConverter
    {
        private static readonly Dictionary<string, Brush> _statusColors = new()
    {
        { "successful", Brushes.YellowGreen },
        { "success", Brushes.YellowGreen },
        { "error", Brushes.Red },
        { "failed", Brushes.Red },
        { "fail", Brushes.Red },
        { "connecting", Brushes.Orange },
        { "please enter", Brushes.Orange },
    };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status && !string.IsNullOrWhiteSpace(status))
            {
                var lowerStatus = status.ToLowerInvariant();

                foreach (var kvp in _statusColors)
                {
                    if (lowerStatus.Contains(kvp.Key))
                        return kvp.Value;
                }
            }

            return Brushes.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
