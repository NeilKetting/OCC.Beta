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
            if (sender is DataGrid dg && dg.SelectedItem is OCC.Shared.Models.Customer customer && 
                DataContext is CustomerManagementViewModel vm)
            {
                vm.EditCustomer(customer);
            }
        }
    }
}
