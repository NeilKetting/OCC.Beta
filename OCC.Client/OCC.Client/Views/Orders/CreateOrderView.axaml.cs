using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using OCC.Client.ViewModels.Orders;
using Avalonia.VisualTree;

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
            if (sender is Button btn && btn.DataContext is OCC.Shared.Models.OrderLine /*line*/)
            {
                if (btn.Parent is Grid grid)
                {
                    foreach(var child in grid.Children)
                    {
                        if (child is AutoCompleteBox acBox)
                        {
                            // 1. Focus the box first
                            acBox.Focus();

                            // 2. Ensure VM knows about the text (or lack thereof) so it resets the filter to show all items
                            if (this.DataContext is CreateOrderViewModel vm)
                            {
                                // If the box text is different from VM (e.g. we moved from another row), sync it.
                                // If box is empty, this ensures VM clears the filter and shows all items.
                                var text = acBox.Text ?? string.Empty;
                                if (vm.ProductSearchText != text)
                                {
                                    vm.ProductSearchText = text;
                                }
                            }

                            // 3. Open the dropdown
                            // Note: If FilterMode is None and ItemSource is populated, this should work.
                            acBox.IsDropDownOpen = !acBox.IsDropDownOpen;
                            break;
                        }
                    }
                }
            }
        }
        public void UOM_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
             // Close the flyout when an item is selected
             if (sender is ListBox listBox && listBox.IsVisible)
             {
                 // Find the parent FlyoutPresenter to close the Popup
                 // Or simpler: Just unselect if needed, but we want to close the popup.
                 // In Avalonia, the Flyout doesn't have a direct "Close" on the content.
                 // But we can find the popup?
                 // Actually, standard behavior for ListBox in Flyout doesn't auto-close.
                 // We can use the attached property or search visual tree.
                 
                 // Robust way: Find the Popup and close it.
                 var popup = listBox.FindAncestorOfType<Avalonia.Controls.Primitives.Popup>();
                 if (popup != null)
                 {
                     popup.IsOpen = false;
                 }
             }
        }
    }
}
