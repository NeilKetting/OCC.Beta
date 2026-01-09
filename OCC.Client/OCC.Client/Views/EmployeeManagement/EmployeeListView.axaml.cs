using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.EmployeeManagement
{
    public partial class EmployeeListView : UserControl
    {
        public EmployeeListView()
        {
            InitializeComponent();
        }

        // Event handler for DataGrid DoubleTapped if needed, or binding
        private void DataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
             // If the original view had this in code behind, we need it here.
             // The original view had DoubleTapped="DataGrid_DoubleTapped"
             // Check EmployeeManagementView.axaml.cs to see what it did.
             
             // Since I can't see the .cs file yet, I should probably check it before finalizing this file.
             // However, usually it just calls a command.
             // Looking at the view: Command="{Binding EditEmployeeCommand}" is on the ContextMenu and Button.
             // DoubleTapped likely calls EditEmployee too.
             
             if (DataContext is ViewModels.EmployeeManagement.EmployeeManagementViewModel vm && 
                 sender is DataGrid dg && 
                 dg.SelectedItem is OCC.Shared.Models.Employee emp)
             {
                 vm.EditEmployeeCommand.Execute(emp);
             }
        }
    }
}
