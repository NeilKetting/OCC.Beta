using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OCC.Client.Features.EmployeeHub.ViewModels;

namespace OCC.Client.Features.EmployeeHub.Views
{
    public partial class EmployeeListView : UserControl
    {
        public EmployeeListView()
        {
            InitializeComponent();
        }

        private void DataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            if (DataContext is EmployeeManagementViewModel vm && 
                sender is DataGrid dg && 
                dg.SelectedItem is OCC.Shared.Models.Employee emp)
            {
                vm.EditEmployeeCommand.Execute(emp);
            }
        }
    }
}
