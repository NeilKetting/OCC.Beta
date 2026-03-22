using System;
using System.Globalization;
using System.Windows.Data;
using OCC.Shared.DTOs;

namespace OCC.WpfClient.Infrastructure.Converters
{
    public class UserInitialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ChatUserDto user)
            {
                string initials = "";
                if (!string.IsNullOrEmpty(user.FirstName)) initials += char.ToUpper(user.FirstName[0]);
                if (!string.IsNullOrEmpty(user.LastName)) initials += char.ToUpper(user.LastName[0]);
                return initials;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
