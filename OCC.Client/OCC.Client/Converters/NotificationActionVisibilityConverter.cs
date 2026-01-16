using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Linq;

namespace OCC.Client.Converters
{
    public class NotificationActionVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string targetAction && parameter is string buttonType)
            {
                // Logic based on TargetAction and Button Type
                switch (buttonType)
                {
                    case "Approve":
                    case "Deny":
                        return targetAction == "UserRegistration" || 
                               targetAction == "LeaveRequest" || 
                               targetAction == "OvertimeRequest";
                    
                    case "View":
                        return targetAction == "UserRegistration" || 
                               targetAction == "LeaveRequest" || 
                               targetAction == "OvertimeRequest" || 
                               targetAction == "BugReports";
                               
                    default:
                        return true;
                }
            }
            
            // If TargetAction is null/empty, usually return false for these buttons
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
