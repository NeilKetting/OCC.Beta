using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OCC.Client.ViewModels.Time;

namespace OCC.Client.Views.Time
{
    public partial class DailyTimesheetView : UserControl
    {
        public DailyTimesheetView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
