using System;
using System.Globalization;
using Avalonia.Data.Converters;
using OCC.Shared.Models;

namespace OCC.Client.Converters
{
    public class GuidToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isNotEmpty = false;
            if (value is Guid guid)
            {
                isNotEmpty = guid != Guid.Empty;
            }
            
            if (parameter?.ToString() == "!")
            {
                return !isNotEmpty;
            }
            return isNotEmpty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }

    public class InvertBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return value;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return value;
        }
    }

    public class NullToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isNotNull = value != null;
            if (parameter?.ToString() == "!") return !isNotNull;
            return isNotNull;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }

    public class EmployeeToNameConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Employee employee)
            {
                return employee.DisplayName;
            }
            
            if (value is string s) return s;

            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Employee employee) return employee.DisplayName;
            return value;
        }
    }
}
