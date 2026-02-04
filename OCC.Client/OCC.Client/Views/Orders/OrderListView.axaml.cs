using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Orders
{
    public partial class OrderListView : UserControl
    {
        public OrderListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void DataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            if (sender is DataGrid dg && dg.SelectedItem is OCC.Shared.Models.Order order && 
                DataContext is ViewModels.Orders.OrderListViewModel vm)
            {
                vm.ViewOrderCommand.Execute(order);
            }
        }
    }
}
