using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using OCC.Client.ViewModels.Orders;
using Avalonia.VisualTree;
using Avalonia;

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
            this.DetachedFromVisualTree += OnDetachedFromVisualTree;
        }

        private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            // Force close any open dropdowns when navigating away
             if (this.DataContext is CreateOrderViewModel vm)
             {
                 // We can't easily iterate the virtualized DataGrid rows, 
                 // but we can ensure the ViewModel knows to reset any state if needed.
                 // However, for the AutoCompleteBox popup, it should close if the control is detached.
                 // If it's "Stuck", it might be because of a bug in Avalonia where Popups don't close on detach.
                 
                 // Try to find any open popups in the visual tree (if possible) or just rely on the fact that
                 // if we change IsDropDownOpen = false on the focused element it might help.
                 
                 var focusManager = TopLevel.GetTopLevel(this)?.FocusManager;
                 if (focusManager?.GetFocusedElement() is AutoCompleteBox box)
                 {
                     box.IsDropDownOpen = false;
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
                box.DataContext is OCC.Client.ModelWrappers.OrderLineWrapper line && 
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
                box.DataContext is OCC.Client.ModelWrappers.OrderLineWrapper line && 
                this.DataContext is CreateOrderViewModel vm)
            {
                vm.TryCommitAutoSelection(line);
            }
        }

        public void ToggleProductDropdown(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is OCC.Client.ModelWrappers.OrderLineWrapper /*line*/)
            {
                if (btn.Parent is Grid grid)
                {
                    foreach(var child in grid.Children)
                    {
                        if (child is AutoCompleteBox acBox)
                        {
                            // 1. Focus the box first
                            acBox.Focus();

                            // 2. Ensure VM knows about the text so it resets the filter to show all items
                            if (this.DataContext is CreateOrderViewModel vm)
                            {
                                var text = acBox.Text ?? string.Empty;
                                if (vm.ProductSearchText != text)
                                {
                                    vm.ProductSearchText = text;
                                }
                            }

                            // 3. Open the dropdown
                            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => {
                               acBox.PopulateComplete(); // Force population of all items
                               acBox.IsDropDownOpen = true; // Use simple true setter
                            });
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
        private void AutoCompleteBox_GotFocus(object? sender, RoutedEventArgs e)
        {
             // Logic removed to restore dropdown functionality
        }

        private void TextBox_GotFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }
    }
}
