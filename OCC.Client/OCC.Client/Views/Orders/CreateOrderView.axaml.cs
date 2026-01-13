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


        
        private void ProductBox_KeyUp(object? sender, KeyEventArgs e)
        {
             if (DataContext is CreateOrderViewModel vm && sender is AutoCompleteBox box)
             {
                 // Pass text to VM to filter the shared list
                 if (vm.ProductSearchText != box.Text)
                 {
                     vm.ProductSearchText = box.Text ?? string.Empty;
                 }
                 
                 // Standard Enter key validation
                 if (e.Key == Key.Enter || e.Key == Key.Return)
                 {
                     if (!string.IsNullOrWhiteSpace(box.Text))
                     {
                        vm.ValidateItemSearchCommand.Execute(box.Text);
                     }
                 }
             }
        }

        public void GridProductBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is AutoCompleteBox box && 
                box.DataContext is OCC.Shared.Models.OrderLine line && 
                box.SelectedItem is OCC.Shared.Models.InventoryItem item &&
                this.DataContext is CreateOrderViewModel vm)
            {
                vm.UpdateLineFromSelection(line, item);
                box.IsDropDownOpen = false; 
            }
        }

        public void GridProductBox_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is AutoCompleteBox box && 
                box.DataContext is OCC.Shared.Models.OrderLine line && 
                this.DataContext is CreateOrderViewModel vm)
            {
                vm.TryCommitAutoSelection(line);
            }
        }

        public void ToggleProductDropdown(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is OCC.Shared.Models.OrderLine)
            {
                // Navigate up visual tree to find sibling AutoCompleteBox (or find by name if in same template context)
                // Since Button and ACBox are in same Grid in DataTemplate, we can try to find the ACBox.
                
                // Simpler: The button is in the visual tree. 
                // We need to set IsDropDownOpen on the ACBox.
                // The ACBox is likely obtainable via the parent grid's children.
                
                if (btn.Parent is Grid grid)
                {
                    foreach(var child in grid.Children)
                    {
                        if (child is AutoCompleteBox acBox)
                        {
                            acBox.IsDropDownOpen = !acBox.IsDropDownOpen;
                            if (acBox.IsDropDownOpen) acBox.Focus();
                            break;
                        }
                    }
                }
            }
        }
    }
}
