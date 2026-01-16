using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Core;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OCC.Client.ViewModels.Orders
{
    /// <summary>
    /// ViewModel for managing and viewing inventory stock levels.
    /// Provides filtering and detail view navigation for inventory items.
    /// </summary>
    /// <summary>
    /// ViewModel for managing and viewing inventory stock levels.
    /// Provides filtering and detail view navigation for inventory items.
    /// </summary>
    public partial class InventoryViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IOrderManager _orderManager;
        private readonly IDialogService _dialogService;
        private readonly IInventoryImportService _importService;
        private readonly ILogger<InventoryViewModel> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthService _authService;
        private List<InventoryItem> _allItems = new();

        #endregion

        #region Observables

        /// <summary>
        /// Gets the collection of inventory items matching the current filter.
        /// </summary>
        public ObservableCollection<InventoryItem> InventoryItems { get; } = new();

        /// <summary>
        /// Gets or sets the search query to filter inventory items by name.
        /// </summary>
        [ObservableProperty]
        private string _searchQuery = "";

        /// <summary>
        /// Gets or sets the currently selected branch filter.
        /// </summary>
        [ObservableProperty]
        private Branch? _selectedBranch;

        /// <summary>
        /// Gets the list of available branches for filtering.
        /// </summary>
        public List<Branch> AvailableBranches { get; } = Enum.GetValues<Branch>().ToList();

        /// <summary>
        /// Gets or sets a value indicating whether an asynchronous operation is in progress.
        /// </summary>
        [ObservableProperty]
        private bool _isBusy;

        /// <summary>
        /// Gets or sets the text to be displayed when <see cref="IsBusy"/> is true.
        /// </summary>
        [ObservableProperty]
        private string _busyText = "Please wait...";

        /// <summary>
        /// Gets or sets a value indicating whether the item details popup is visible.
        /// </summary>
        [ObservableProperty]
        private bool _isDetailVisible;

        /// <summary>
        /// Gets or sets the inventory item currently selected for viewing or editing.
        /// </summary>
        [ObservableProperty]
        private InventoryItem? _selectedInventoryItem;

        /// <summary>
        /// Gets or sets the ViewModel for the inventory item detail view.
        /// </summary>
        [ObservableProperty]
        private ItemDetailViewModel? _detailViewModel;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryViewModel"/> class with required dependencies.
        /// </summary>
        /// <param name="orderManager">Manager providing centralized order and inventory operations.</param>
        /// <param name="dialogService">Service for displaying user notifications.</param>
        /// <param name="logger">Logger for capturing diagnostic information.</param>
        /// <param name="serviceProvider">Service provider for resolving child ViewModels via DI.</param>
        /// <param name="authService">Service for accessing current user context.</param>
        public InventoryViewModel(
            IOrderManager orderManager, 
            IDialogService dialogService, 
            IInventoryImportService importService,
            ILogger<InventoryViewModel> logger,
            IServiceProvider serviceProvider,
            IAuthService authService)
        {
            _orderManager = orderManager;
            _dialogService = dialogService;
            _importService = importService;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _authService = authService;
            
            // Default to user's branch if available, otherwise JHB
            SelectedBranch = _authService.CurrentUser?.Branch ?? Branch.JHB;

            _ = LoadInventoryAsync();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to manually refresh the inventory list.
        /// </summary>
        [RelayCommand]
        public async Task RefreshInventory()
        {
            await LoadInventoryAsync();
        }

        /// <summary>
        /// Command to open the detail view for a specific inventory item in edit mode.
        /// </summary>
        /// <param name="item">The inventory item to edit.</param>
        [RelayCommand]
        public void EditInventoryItem(InventoryItem item)
        {
            if (item == null) return;
            
            var categories = _allItems.Select(i => i.Category).Distinct().OrderBy(c => c).ToList();

            DetailViewModel = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ItemDetailViewModel>(_serviceProvider);
            DetailViewModel.Load(item, categories); // Pass branch if needed, or item has it all
            
            DetailViewModel.CloseRequested += (s, e) => IsDetailVisible = false;
            DetailViewModel.ItemSaved += (s, e) => 
            {
                IsDetailVisible = false;
                _ = LoadInventoryAsync();
            };
            
            IsDetailVisible = true;
        }

        /// <summary>
        /// Command to permanently delete an inventory item.
        /// </summary>
        /// <param name="item">The item to be deleted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [RelayCommand]
        public async Task DeleteInventoryItem(InventoryItem item)
        {
             if (item == null) return;
             await _dialogService.ShowAlertAsync("Locked", "Inventory item deletion is currently disabled to maintain historical stock integrity.");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Asynchronously loads all inventory items from the Order Manager and applies the current search filter.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadInventoryAsync()
        {
            try
            {
                BusyText = "Loading inventory...";
                IsBusy = true;
                _allItems = (await _orderManager.GetInventoryAsync()).ToList();
                FilterItems();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading inventory list");
                if(_dialogService != null) await _dialogService.ShowAlertAsync("Error", "Failed to retrieve inventory items. Please try again later.");
            }
             finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Filters the full collection of items based on the current search query.
        /// </summary>
        private void FilterItems()
        {
            InventoryItems.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchQuery) 
                ? _allItems 
                : _allItems.Where(i => i.ProductName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

            foreach (var item in filtered)
            {
                InventoryItems.Add(item);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Responds to changes in the search query by reapplying the filter to the inventory list.
        /// </summary>
        partial void OnSearchQueryChanged(string value) => FilterItems();

        #endregion
    }
}
