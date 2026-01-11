using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using OCC.Shared.Models;
using OCC.Client.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.Orders
{
    public partial class ItemListViewModel : ViewModelBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ItemListViewModel> _logger;
        private List<InventoryItem> _allItems = new();

        public ObservableCollection<InventoryItem> Items { get; } = new();

        [ObservableProperty]
        private string _searchQuery = "";

        [ObservableProperty]
        private bool _isBusy;
        
        [ObservableProperty]
        private InventoryItem? _selectedItem;

        [ObservableProperty]
        private bool _isDetailVisible;

        [ObservableProperty]
        private InventoryDetailViewModel? _detailViewModel;

        private readonly ISupplierService _supplierService;

        public ItemListViewModel(IInventoryService inventoryService, ISupplierService supplierService, IDialogService dialogService, ILogger<ItemListViewModel> logger)
        {
            _inventoryService = inventoryService;
            _supplierService = supplierService;
            _dialogService = dialogService;
            _logger = logger;
            _ = LoadItemsAsync();
        }

        public async Task LoadItemsAsync()
        {
            try
            {
                IsBusy = true;
                _allItems = await _inventoryService.GetInventoryAsync();
                FilterItems();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading item list");
                if(_dialogService != null) await _dialogService.ShowAlertAsync("Error", "Failed to load item list.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSearchQueryChanged(string value)
        {
            FilterItems();
        }

        private void FilterItems()
        {
            Items.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchQuery) 
                ? _allItems 
                : _allItems.Where(i => 
                    i.ProductName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    (i.Supplier != null && i.Supplier.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (i.Category != null && i.Category.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                  );

            foreach (var item in filtered)
            {
                Items.Add(item);
            }
        }

        [RelayCommand]
        public async Task Refresh()
        {
            await LoadItemsAsync();
        }

        [RelayCommand]
        public void EditItem(InventoryItem item)
        {
            if (item == null) return;
            
            var categories = _allItems.Select(i => i.Category).Distinct().OrderBy(c => c).ToList();

            DetailViewModel = new InventoryDetailViewModel(_inventoryService, _dialogService, _supplierService);
            DetailViewModel.Load(item, categories);
            DetailViewModel.CloseRequested += (s, e) => IsDetailVisible = false;
            DetailViewModel.ItemSaved += (s, e) => 
            {
                IsDetailVisible = false;
                _ = LoadItemsAsync();
            };
            IsDetailVisible = true;
        }

        [RelayCommand]
        public async Task DeleteItem(InventoryItem item)
        {
             if (item == null) return;

             var confirm = await _dialogService.ShowConfirmationAsync("Delete Item", $"Are you sure you want to delete '{item.ProductName}'? This cannot be undone.");
             if (!confirm) return;

             try 
             {
                 IsBusy = true;
                 await _inventoryService.DeleteItemAsync(item.Id);
                 await LoadItemsAsync(); // Reload to refresh list
             }
             catch (InvalidOperationException ex) // Conflict
             {
                 await _dialogService.ShowAlertAsync("Cannot Delete", ex.Message);
             }
             catch(Exception ex)
             {
                 await _dialogService.ShowAlertAsync("Error", "Failed to delete item: " + ex.Message);
             }
             finally
             {
                 IsBusy = false;
             }
        }
    }
}
