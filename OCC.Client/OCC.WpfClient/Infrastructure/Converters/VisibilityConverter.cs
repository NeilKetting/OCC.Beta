using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OCC.WpfClient.Infrastructure.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = false;

            if (value is bool b)
            {
                isVisible = b;
            }
            else if (value is string s)
            {
                isVisible = !string.IsNullOrWhiteSpace(s);
            }
            else
            {
                isVisible = value != null;
            }

            // Invert logic if parameter is 'Invert'
            if (parameter?.ToString()?.Equals("Invert", StringComparison.OrdinalIgnoreCase) == true)
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
