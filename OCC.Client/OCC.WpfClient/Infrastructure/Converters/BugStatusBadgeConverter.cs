using OCC.Shared.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OCC.WpfClient.Infrastructure.Converters
{
    public class BugStatusBadgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BugReport bug)
            {
                string param = parameter as string ?? "Text";

                if (param == "Text")
                {
                    return bug.Status == "Open" ? bug.Type.ToString() : bug.Status;
                }
                
                if (param == "Background")
                {
                    if (bug.Status != "Open")
                    {
                        return bug.Status switch
                        {
                            "In Progress" => Brushes.Orange,
                            "Planning" => Brushes.SlateGray,
                            "Feature Update" => Brushes.DeepSkyBlue,
                            "Fixed" => Brushes.MediumSeaGreen,
                            "Waiting for Client" => Brushes.Goldenrod,
                            "Resolved" => Brushes.MediumSeaGreen,
                            "Closed" => Brushes.DarkGray,
                            _ => Brushes.SlateGray
                        };
                    }

                    return bug.Type switch
                    {
                        BugReportType.Bug => (Brush)new BrushConverter().ConvertFrom("#EF4444"),
                        BugReportType.Suggestion => (Brush)new BrushConverter().ConvertFrom("#06B6D4"),
                        BugReportType.Question => (Brush)new BrushConverter().ConvertFrom("#8B5CF6"),
                        _ => (Brush)new BrushConverter().ConvertFrom("#64748B")
                    };
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
