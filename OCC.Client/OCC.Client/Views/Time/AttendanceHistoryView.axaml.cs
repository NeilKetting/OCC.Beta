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
            if (DataContext is ViewModels.Time.AttendanceHistoryViewModel vm && vm.SelectedRecord != null)
            {
                vm.OpenEmployeeReportCommand.Execute(vm.SelectedRecord);
            }
        }
    }
}
