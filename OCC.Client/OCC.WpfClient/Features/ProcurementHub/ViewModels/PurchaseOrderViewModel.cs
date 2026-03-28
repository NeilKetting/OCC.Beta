using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using OCC.Shared.Models;
using OCC.WpfClient.Features.ProcurementHub.Models;
using OCC.WpfClient.Infrastructure;
using OCC.WpfClient.Services.Interfaces;
using System.Collections.ObjectModel;

namespace OCC.WpfClient.Features.ProcurementHub.ViewModels
{
    public partial class PurchaseOrderViewModel : ViewModelBase
    {
        private readonly IOrderService _orderService;
        private readonly ISupplierService _supplierService;
        private readonly IProjectService _projectService;
        private readonly IInventoryService _inventoryService;
        private readonly INavigationService _navigationService;
        private readonly ILogger<PurchaseOrderViewModel> _logger;

        [ObservableProperty]
        private OrderWrapper? _currentOrder;

        [ObservableProperty]
        private ObservableCollection<Supplier> _suppliers = new();

        [ObservableProperty]
        private ObservableCollection<Project> _projects = new();

        [ObservableProperty]
        private ObservableCollection<InventoryItem> _inventoryItems = new();

        [ObservableProperty]
        private Supplier? _selectedSupplier;

        [ObservableProperty]
        private Project? _selectedProject;

        public PurchaseOrderViewModel(
            IOrderService orderService,
            ISupplierService supplierService,
            IProjectService projectService,
            IInventoryService inventoryService,
            INavigationService navigationService,
            ILogger<PurchaseOrderViewModel> logger)
        {
            _orderService = orderService;
            _supplierService = supplierService;
            _projectService = projectService;
            _inventoryService = inventoryService;
            _navigationService = navigationService;
            _logger = logger;

            Title = "Create Purchase Order";
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                
                var suppliersTask = _supplierService.GetSuppliersAsync();
                var projectsTask = _projectService.GetProjectsAsync();
                var inventoryTask = _inventoryService.GetInventoryAsync();

                await Task.WhenAll(suppliersTask, projectsTask, inventoryTask);

                Suppliers = new ObservableCollection<Supplier>(await suppliersTask);
                Projects = new ObservableCollection<Project>(await projectsTask);
                InventoryItems = new ObservableCollection<InventoryItem>(await inventoryTask);

                if (CurrentOrder == null)
                {
                    var order = await _orderService.CreateNewOrderTemplateAsync(OrderType.PurchaseOrder);
                    CurrentOrder = new OrderWrapper(order);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading PO data");
                ErrorMessage = "Failed to load required data. Please try again.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSelectedSupplierChanged(Supplier? value)
        {
            if (value != null && CurrentOrder != null)
            {
                CurrentOrder.SupplierId = value.Id;
                CurrentOrder.SupplierName = value.Name;
                CurrentOrder.EntityAddress = value.Address;
                CurrentOrder.EntityTel = value.Phone;
                CurrentOrder.EntityVatNo = value.VatNumber;
            }
        }

        partial void OnSelectedProjectChanged(Project? value)
        {
            if (value != null && CurrentOrder != null)
            {
                CurrentOrder.ProjectId = value.Id;
                CurrentOrder.ProjectName = value.Name;
                CurrentOrder.Attention = value.ProjectManager ?? string.Empty;
            }
        }

        [RelayCommand]
        private void AddLine()
        {
            if (CurrentOrder == null) return;

            var newline = new OrderLine
            {
                Id = Guid.NewGuid(),
                OrderId = CurrentOrder.Id,
                QuantityOrdered = 1,
                UnitPrice = 0
            };

            CurrentOrder.Lines.Add(new OrderLineWrapper(newline, CurrentOrder));
        }

        [RelayCommand]
        private void RemoveLine(OrderLineWrapper line)
        {
            CurrentOrder?.Lines.Remove(line);
        }

        [RelayCommand]
        private async Task SaveOrderAsync()
        {
            if (CurrentOrder == null) return;

            try
            {
                IsBusy = true;
                bool isNew = CurrentOrder.Id == Guid.Empty;
                if (!isNew)
                {
                    var existing = await _orderService.GetOrderAsync(CurrentOrder.Id);
                    isNew = (existing == null);
                }

                // Simplified: Always create for now if Id is from template
                await _orderService.CreateOrderAsync(CurrentOrder.Model);
                _navigationService.NavigateTo<ProcurementViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving order");
                ErrorMessage = "Failed to save the order.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            _navigationService.NavigateTo<ProcurementViewModel>();
        }

        [RelayCommand]
        private void UpdateLineItem(OrderLineWrapper line)
        {
            // When an item is selected from a dropdown in the grid
            var item = InventoryItems.FirstOrDefault(i => i.Sku == line.ItemCode);
            if (item != null)
            {
                line.InventoryItemId = item.Id;
                line.Description = item.Description;
                line.UnitOfMeasure = item.UnitOfMeasure;
                line.UnitPrice = item.AverageCost;
                line.UpdateCalculations();
            }
        }
    }
}
