using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OCC.Client.Converters
{
    public class ActiveTabToBrushConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value?.ToString() is string activeTab && parameter?.ToString() is string tabName)
            {
                if (activeTab.Equals(tabName, StringComparison.OrdinalIgnoreCase))
                {
                    // AccentOrange
                    if (Avalonia.Application.Current.TryGetResource("AccentOrange", null, out var resource) && resource is IBrush brush)
                    {
                        return brush;
                    }
                    return Brushes.Orange; 
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
             throw new NotImplementedException();
        }
    }
}
