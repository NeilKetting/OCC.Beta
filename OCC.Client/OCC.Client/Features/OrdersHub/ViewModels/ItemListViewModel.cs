using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using OCC.Shared.Models;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.Features.OrdersHub.ViewModels
{
    /// <summary>
    /// ViewModel for managing the master list of inventory items.
    /// Supports adding, editing, deleting, and searching for items across categories and suppliers.
    /// </summary>
    /// <summary>
    /// ViewModel for managing the master list of inventory items.
    /// Supports adding, editing, deleting, and searching for items across categories and suppliers.
    /// </summary>
    public partial class ItemListViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IOrderManager _orderManager;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ItemListViewModel> _logger;
        private readonly IServiceProvider _serviceProvider;
        private List<InventoryItem> _allItems = new();

        #endregion

        #region Observables

        /// <summary>
        /// Gets the collection of inventory items currently visible in the list.
        /// </summary>
        public ObservableCollection<InventoryItem> Items { get; } = new();

        /// <summary>
        /// Gets or sets the search query used to filter the item list.
        /// </summary>
        [ObservableProperty]
        private string _searchQuery = "";



        /// <summary>
        /// Gets or sets the currently selected item in the list.
        /// </summary>
        [ObservableProperty]
        private InventoryItem? _selectedItem;

        /// <summary>
        /// Gets or sets a value indicating whether the item detail popup is currently visible.
        /// </summary>
        [ObservableProperty]
        private bool _isDetailVisible;

        /// <summary>
        /// Gets or sets the ViewModel for the item detail view.
        /// </summary>
        [ObservableProperty]
        private ItemDetailViewModel? _detailViewModel;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemListViewModel"/> class with required dependencies.
        /// </summary>
        /// <param name="orderManager">Central manager for item and supplier operations.</param>
        /// <param name="dialogService">Service for displaying user dialogs.</param>
        /// <param name="logger">Logger for capturing diagnostic information.</param>
        /// <param name="serviceProvider">Provider for resolving child ViewModels.</param>
        public ItemListViewModel(
            IOrderManager orderManager, 
            IDialogService dialogService, 
            ILogger<ItemListViewModel> logger,
            IServiceProvider serviceProvider)
        {
            _orderManager = orderManager;
            _dialogService = dialogService;
            _logger = logger;
            _serviceProvider = serviceProvider;
            
            _ = LoadItemsAsync();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to refresh the item list.
        /// </summary>
        [RelayCommand]
        public async Task Refresh()
        {
            await LoadItemsAsync();
        }

        /// <summary>
        /// Command to initiate the process of adding a new inventory item.
        /// </summary>
        [RelayCommand]
        public void AddItem()
        {
            var categories = _allItems.Select(i => i.Category).Distinct().OrderBy(c => c).ToList();

            DetailViewModel = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ItemDetailViewModel>(_serviceProvider);
            DetailViewModel.Load(null, categories);
            
            DetailViewModel.CloseRequested += (s, e) => IsDetailVisible = false;
            DetailViewModel.ItemSaved += (s, e) => 
            {
                IsDetailVisible = false;
                _ = LoadItemsAsync();
            };
            IsDetailVisible = true;
        }

        /// <summary>
        /// Command to open the detail view for editing an existing inventory item.
        /// </summary>
        /// <param name="item">The item to edit.</param>
        [RelayCommand]
        public void EditItem(InventoryItem item)
        {
            if (item == null) return;
            
            var categories = _allItems.Select(i => i.Category).Distinct().OrderBy(c => c).ToList();

            DetailViewModel = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ItemDetailViewModel>(_serviceProvider);
            DetailViewModel.Load(item, categories);
            
            DetailViewModel.CloseRequested += (s, e) => IsDetailVisible = false;
            DetailViewModel.ItemSaved += (s, e) => 
            {
                IsDetailVisible = false;
                _ = LoadItemsAsync();
            };
            IsDetailVisible = true;
        }

        /// <summary>
        /// Command to permanently delete an inventory item.
        /// </summary>
        /// <param name="item">The inventory item to be deleted.</param>
        /// <returns>A task representing the operation.</returns>
        [RelayCommand]
        public async Task DeleteItem(InventoryItem item)
        {
             if (item == null) return;

             var confirm = await _dialogService.ShowConfirmationAsync("Confirm Delete", $"Are you sure you want to permanently delete item '{item.Description}'? This action cannot be reversed.");
             if (!confirm) return;

             try 
             {
                 BusyText = $"Deleting '{item.Description}'...";
                 IsBusy = true;
                 
                 await _orderManager.DeleteItemAsync(item.Id);
                 await LoadItemsAsync();
             }
             catch (InvalidOperationException ex)
             {
                 await _dialogService.ShowAlertAsync("Deletion Restricted", ex.Message);
             }
             catch(Exception ex)
             {
                 _logger.LogError(ex, "Failed to delete item {ItemName}", item.Description);
                 await _dialogService.ShowAlertAsync("Error", $"An unexpected error occurred while deleting: {ex.Message}");
             }
             finally
             {
                 IsBusy = false;
             }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Asynchronously retrieves the full list of inventory items and applies the current search filter.
        /// </summary>
        /// <returns>A task representing the operation.</returns>
        public async Task LoadItemsAsync()
        {
            try
            {
                BusyText = "Loading master item list...";
                IsBusy = true;
                _allItems = (await _orderManager.GetInventoryAsync()).ToList();
                FilterItems();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading master item list");
                if(_dialogService != null) await _dialogService.ShowAlertAsync("Error", $"Failed to retrieve the item list: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Filters the master item collection based on product name, supplier, or category.
        /// </summary>
        private void FilterItems()
        {
            Items.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchQuery) 
                ? _allItems 
                : _allItems.Where(i => 
                    i.Description.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    (i.Supplier != null && i.Supplier.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (i.Category != null && i.Category.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                  );

            foreach (var item in filtered)
            {
                Items.Add(item);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Re-filters the item list whenever the search query changes.
        /// </summary>
        partial void OnSearchQueryChanged(string value) => FilterItems();

        #endregion
    }
}
