using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using OCC.Shared.Models;
using OCC.WpfClient.Infrastructure;
using OCC.WpfClient.Infrastructure.Messages;
using OCC.WpfClient.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.WpfClient.Features.ProcurementHub.ViewModels
{
    public partial class InventoryViewModel : ViewModelBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IToastService _toastService;
        private readonly ILogger<InventoryViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<InventoryItem> _items = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        public InventoryViewModel(IInventoryService inventoryService, IToastService toastService, ILogger<InventoryViewModel> logger)
        {
            _inventoryService = inventoryService;
            _toastService = toastService;
            _logger = logger;
            Title = "Inventory Management";

            // Listen for stock updates from other hubs (e.g. Picking)
            WeakReferenceMessenger.Default.Register<StockUpdatedMessage>(this, (r, m) =>
            {
                var item = Items.FirstOrDefault(i => i.Id == m.Value.Id);
                if (item != null)
                {
                    _logger.LogInformation("Inventory item {ItemId} updated from message", m.Value.Id);
                    App.Current.Dispatcher.Invoke(async () => await LoadInventoryAsync());
                }
            });

            _logger.LogInformation("InventoryViewModel initialized");
            _ = LoadInventoryAsync();
        }

        [RelayCommand]
        private async Task LoadInventoryAsync()
        {
            try
            {
                _logger.LogInformation("Loading inventory items...");
                IsBusy = true;
                var inventory = await _inventoryService.GetInventoryAsync();
                var list = inventory.ToList();
                Items = new ObservableCollection<InventoryItem>(list);
                _logger.LogInformation("Successfully loaded {Count} inventory items", list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load inventory");
                _toastService.ShowError("Error", $"Failed to load inventory: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                 // Reset filter logic if any
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await LoadInventoryAsync();
            _toastService.ShowInfo("Inventory", "Inventory refreshed.");
        }
    }
}
