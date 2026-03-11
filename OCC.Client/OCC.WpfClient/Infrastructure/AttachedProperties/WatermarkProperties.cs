using System.Windows;
using System.Windows.Controls;

namespace OCC.WpfClient.Infrastructure.AttachedProperties
{
    public static class WatermarkProperties
    {
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached(
                "Watermark",
                typeof(object),
                typeof(WatermarkProperties),
                new FrameworkPropertyMetadata(null));

        public static object GetWatermark(DependencyObject d)
        {
            return (object)d.GetValue(WatermarkProperty);
        }

        public static void SetWatermark(DependencyObject d, object value)
        {
            d.SetValue(WatermarkProperty, value);
        }
    }
}
