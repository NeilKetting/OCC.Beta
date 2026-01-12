using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using OCC.Client.ViewModels.Orders;

namespace OCC.Client.Views.Orders
{
    public partial class CreateOrderView : UserControl
    {
        public CreateOrderView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SkuBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (DataContext is CreateOrderViewModel vm && sender is ComboBox box)
            {
                // Only validate if text is present and NO item is selected
                if (!string.IsNullOrWhiteSpace(box.Text) && vm.SelectedInventoryItem == null)
                {
                    vm.ValidateItemSearchCommand.Execute(box.Text);
                }
            }
        }

        private void SkuBox_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                 // Force validation on Enter
                 if (DataContext is CreateOrderViewModel vm && sender is ComboBox box)
                 {
                     if (!string.IsNullOrWhiteSpace(box.Text))
                     {
                        vm.ValidateItemSearchCommand.Execute(box.Text);
                     }
                 }
            }
        }

        private void ProductBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (DataContext is CreateOrderViewModel vm && sender is ComboBox box)
            {
                if (!string.IsNullOrWhiteSpace(box.Text) && vm.SelectedInventoryItem == null)
                {
                    vm.ValidateItemSearchCommand.Execute(box.Text);
                }
            }
        }
        
        private void ProductBox_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                 if (DataContext is CreateOrderViewModel vm && sender is ComboBox box)
                 {
                     if (!string.IsNullOrWhiteSpace(box.Text))
                     {
                        vm.ValidateItemSearchCommand.Execute(box.Text);
                     }
                 }
            }
        }
    }
}
