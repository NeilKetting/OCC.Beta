using Avalonia.Controls;

namespace OCC.Client.Views.Time
{
    public partial class AttendanceHistoryView : UserControl
    {
        public AttendanceHistoryView()
        {
            InitializeComponent();
        }

        private void DataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            if (sender is DataGrid dg && dg.SelectedItem is ViewModels.Time.HistoryRecordViewModel record && 
                DataContext is ViewModels.Time.AttendanceHistoryViewModel vm)
            {
                vm.OpenEmployeeReportCommand.Execute(record);
            }
        }
    }
}
