using System.Linq;
using System.Collections.ObjectModel;
using OCC.Client.ViewModels.Core;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls.Selection;
using OCC.Client.ModelWrappers;
using OCC.Shared.Models;

namespace OCC.Client.ViewModels.Dev
{
    public partial class TestViewModel : ViewModelBase
    {
        [ObservableProperty]
        private FlatTreeDataGridSource<OrderLineWrapper> _source;

        [ObservableProperty]
        private ObservableCollection<OrderLineWrapper> _dataGridSource;

        public ObservableCollection<string> AvailableUOMs { get; } = new() { "ea", "m", "kg", "L", "m2", "m3", "box", "roll", "pack" };
        public ObservableCollection<string> DummyItems { get; } = new() { "STEEL-001", "CEMENT-050", "BRICK-RED", "SAND-RIV", "TIMBER-4X2" };

        public TestViewModel()
        {
            var data = new ObservableCollection<OrderLineWrapper>();
            Source = new FlatTreeDataGridSource<OrderLineWrapper>(data);
            
            // Item Column (Template)
            Source.Columns.Add(new TemplateColumn<OrderLineWrapper>(
                "Item",
                "ItemTemplate",
                null,
                new GridLength(2, GridUnitType.Star)));

            // Description
            Source.Columns.Add(new TemplateColumn<OrderLineWrapper>(
                "Description",
                "DescriptionTemplate",
                null,
                new GridLength(3, GridUnitType.Star)));

            // Qty
            Source.Columns.Add(new TemplateColumn<OrderLineWrapper>(
                "Qty",
                "QtyTemplate",
                null,
                GridLength.Auto));

            // Unit
            Source.Columns.Add(new TemplateColumn<OrderLineWrapper>(
                "Unit",
                "UnitTemplate",
                null,
                GridLength.Auto));

            // Price
            Source.Columns.Add(new TemplateColumn<OrderLineWrapper>(
                "Price",
                "PriceTemplate",
                null,
                GridLength.Auto));

            // Total
            Source.Columns.Add(new TemplateColumn<OrderLineWrapper>(
                "Total",
                "TotalTemplate",
                null,
                GridLength.Auto));

            // Disable selection to prevent focus interception
            Source.Selection = null;

            // Initialize DataGridSource
            DataGridSource = new ObservableCollection<OrderLineWrapper>();

            // Start with EXACTLY one row in both
            var firstLine = new OrderLineWrapper(new OrderLine { UnitOfMeasure = "ea" });
            data.Add(firstLine);
            
            var dgFirstLine = new OrderLineWrapper(new OrderLine { UnitOfMeasure = "ea" });
            DataGridSource.Add(dgFirstLine);

            // Listen for additions to subscribe to PropertyChanged
            foreach (var item in data)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
            data.CollectionChanged += Items_CollectionChanged;

            foreach (var item in DataGridSource)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
            DataGridSource.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (OrderLineWrapper item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }
        }

        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OrderLineWrapper.LineTotal) ||
                e.PropertyName == nameof(OrderLineWrapper.ItemCode) ||
                e.PropertyName == nameof(OrderLineWrapper.Description))
            {
                if (sender is OrderLineWrapper line)
                {
                    CheckForNewRow(line);
                }
            }
        }

        private void CheckForNewRow(OrderLineWrapper changedItem)
        {
            // Determine which collection this item belongs to
            ObservableCollection<OrderLineWrapper>? targetCollection = null;
            if (Source.Items is ObservableCollection<OrderLineWrapper> treeItems && treeItems.Contains(changedItem))
            {
                targetCollection = treeItems;
            }
            else if (DataGridSource.Contains(changedItem))
            {
                targetCollection = DataGridSource;
            }

            if (targetCollection != null)
            {
                var lastItem = targetCollection.LastOrDefault();
                if (lastItem == changedItem) // Only add if the changed item IS the last item
                {
                    // More explicit check for content to avoid accidental row adds
                    bool hasProduct = !string.IsNullOrWhiteSpace(lastItem.ItemCode);
                    bool hasDesc = !string.IsNullOrWhiteSpace(lastItem.Description);
                    bool hasPrice = lastItem.LineTotal > 0;

                    if (hasProduct || hasDesc || hasPrice)
                    {
                        // Check if we ALREADY have an empty row at the very bottom
                        var existsEmpty = targetCollection.Any(x => string.IsNullOrWhiteSpace(x.ItemCode) && 
                                                                   string.IsNullOrWhiteSpace(x.Description) && 
                                                                   x.LineTotal == 0);
                        
                        if (!existsEmpty)
                        {
                            targetCollection.Add(new OrderLineWrapper(new OrderLine { UnitOfMeasure = "ea" }));
                        }
                    }
                }
            }
        }

        [RelayCommand]
        public void RemoveLine(OrderLineWrapper line)
        {
            if (line == null) return;

            ObservableCollection<OrderLineWrapper>? targetCollection = null;
            if (Source.Items is ObservableCollection<OrderLineWrapper> treeItems && treeItems.Contains(line))
            {
                targetCollection = treeItems;
            }
            else if (DataGridSource.Contains(line))
            {
                targetCollection = DataGridSource;
            }

            if (targetCollection != null)
            {
                if (targetCollection.Count > 1) // Keep at least one row
                {
                    targetCollection.Remove(line);
                }
                else
                {
                    // Clear it instead if it's the last one
                    line.ItemCode = "";
                    line.Description = "";
                    line.QuantityOrdered = 0;
                    line.UnitPrice = 0;
                }
            }
        }

        [RelayCommand]
        public void ChangeUom(string uom)
        {
            // Simulation logic
        }

        private ObservableCollection<OrderLineWrapper> GenerateDummyData()
        {
            return new ObservableCollection<OrderLineWrapper>();
        }
    }
}
