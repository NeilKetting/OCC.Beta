using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.Features.OrdersHub.ViewModels
{
    public partial class InventoryLookupViewModel : ViewModelBase
    {
        private readonly IOrderManager _orderManager;
        private readonly IDialogService _dialogService;
        private readonly ILogger<InventoryLookupViewModel> _logger;

        private IEnumerable<InventoryItem> _allInventoryMaster = Enumerable.Empty<InventoryItem>();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private InventoryItem? _selectedItem;

        [ObservableProperty]
        private bool _isAddingNew;

        [ObservableProperty]
        private string _newDescription = string.Empty;

        [ObservableProperty]
        private string _newUOM = "ea";

        [ObservableProperty]
        private string _newCategory = "General";

        [ObservableProperty]
        private bool _isInputtingNewCategory;

        public ObservableCollection<InventoryItem> FilteredItems { get; } = new();
        public ObservableCollection<string> Categories { get; } = new();

        protected InventoryLookupViewModel() 
        {
            _orderManager = null!;
            _dialogService = null!;
            _logger = null!;
        }

        public InventoryLookupViewModel(
            IOrderManager orderManager,
            IDialogService dialogService,
            ILogger<InventoryLookupViewModel> logger)
        {
            _orderManager = orderManager;
            _dialogService = dialogService;
            _logger = logger;
        }

        public virtual void Initialize(IEnumerable<InventoryItem> inventory)
        {
            _allInventoryMaster = inventory;
            
            var cats = inventory.Select(x => x.Category).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c);
            Categories.Clear();
            foreach(var c in cats) Categories.Add(c);
            if (!Categories.Contains("General")) Categories.Add("General");

            Filter();
        }

        [RelayCommand]
        public void Filter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredItems.Clear();
                foreach (var item in _allInventoryMaster) FilterItems(item);
                return;
            }

            var search = SearchText.Trim();
            var filtered = _allInventoryMaster
                .Where(i => (i.Description != null && i.Description.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                            (i.Sku != null && i.Sku.Contains(search, StringComparison.OrdinalIgnoreCase)))
                .GroupBy(i => (i.Sku ?? "").ToLower()).Select(g => g.First())
                .OrderByDescending(i => i.Sku != null && i.Sku.Equals(search, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(i => i.Description != null && i.Description.Equals(search, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(i => i.Sku != null && i.Sku.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(i => i.Description != null && i.Description.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                .ThenBy(i => i.Description)
                .ToList();

            FilteredItems.Clear();
            foreach (var item in filtered) FilteredItems.Add(item);
        }

        private void FilterItems(InventoryItem item)
        {
            // Simple helper for initialization/clear
            FilteredItems.Add(item);
        }

        [RelayCommand]
        public void ToggleQuickAdd()
        {
            IsAddingNew = !IsAddingNew;
            if (IsAddingNew)
            {
                NewDescription = "";
                NewUOM = "ea";
                NewCategory = "General";
                IsInputtingNewCategory = false;
            }
        }

        [RelayCommand]
        public void ToggleNewCategoryMode()
        {
            IsInputtingNewCategory = !IsInputtingNewCategory;
            NewCategory = IsInputtingNewCategory ? "" : "General";
        }

        [RelayCommand]
        public async Task QuickCreateProduct(string? supplierName = null)
        {
            if (string.IsNullOrWhiteSpace(NewDescription)) return;
            
            try
            {
                var created = await _orderManager.QuickCreateProductAsync(NewDescription, NewUOM, NewCategory, supplierName ?? string.Empty);
                
                // Add to master and local lists
                var masterList = _allInventoryMaster.ToList();
                masterList.Add(created);
                _allInventoryMaster = masterList;

                if (!Categories.Contains(created.Category)) Categories.Add(created.Category);
                
                SelectedItem = created;
                IsAddingNew = false;
                
                Filter(); // Refresh list
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to quick create product");
                await _dialogService.ShowAlertAsync("Error", "Failed to create product.");
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            Filter();
        }
    }
}
