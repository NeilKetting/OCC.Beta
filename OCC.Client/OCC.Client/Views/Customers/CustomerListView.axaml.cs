using Avalonia.Controls;
using Avalonia.Input;
using OCC.Client.ViewModels.Customers;

namespace OCC.Client.Views.Customers
{
    public partial class CustomerListView : UserControl
    {
        public CustomerListView()
        {
            InitializeComponent();
        }

        private void DataGrid_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (DataContext is CustomerManagementViewModel vm && vm.SelectedCustomer != null)
            {
                vm.EditCustomer(vm.SelectedCustomer);
            }
        }
    }
}
