using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Messages;
using OCC.Client.Models;
using OCC.Client.Services.Infrastructure;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Client.ViewModels.Messages; // For NavigationRequestMessage
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Orders
{
    public partial class RestockReviewViewModel : ViewModelBase
    {
        private readonly IOrderManager _orderManager;
        private readonly OrderStateService _orderStateService;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private ObservableCollection<SupplierRestockGroup> _groups = new();

        public OrderMenuViewModel OrderMenu { get; }

        public RestockReviewViewModel(IOrderManager orderManager, OrderStateService orderStateService, OrderMenuViewModel orderMenu)
        {
            _orderManager = orderManager;
            _orderStateService = orderStateService;
            OrderMenu = orderMenu;
            OrderMenu.TabSelected += OnMenuTabSelected;
        }

        private void OnMenuTabSelected(object? sender, string tabName)
        {
             switch (tabName)
            {
                case "Dashboard":
                    WeakReferenceMessenger.Default.Send(new NavigationRequestMessage("Orders"));
                    break;
                case "All Orders":
                    WeakReferenceMessenger.Default.Send(new NavigationRequestMessage("OrderList"));
                    break;
                case "Inventory":
                    WeakReferenceMessenger.Default.Send(new NavigationRequestMessage("Inventory"));
                    break;
                case "ItemList":
                     WeakReferenceMessenger.Default.Send(new NavigationRequestMessage("ItemList"));
                     break;
                case "Suppliers":
                    WeakReferenceMessenger.Default.Send(new NavigationRequestMessage("Suppliers"));
                    break;
                case "New Order":
                    WeakReferenceMessenger.Default.Send(new NavigationRequestMessage("CreateOrder"));
                    break;
            }
        }

        public RestockReviewViewModel() { _orderManager = null!; _orderStateService = null!; OrderMenu = null!; }

        public async Task LoadData()
        {
            IsBusy = true;
            try
            {
                var candidates = await _orderManager.GetRestockCandidatesAsync();
                
                var grouped = candidates.GroupBy(c => c.Item.Supplier)
                                        .Select(g => new SupplierRestockGroup(g.Key, g.ToList()))
                                        .OrderByDescending(g => g.Items.Count)
                                        .ToList();

                Groups.Clear();
                foreach (var g in grouped) Groups.Add(g);
                
                if (!Groups.Any())
                {
                     WeakReferenceMessenger.Default.Send(new ToastNotificationMessage(new ToastMessage { Title = "Stock Status", Message = "All items are well stocked!", Type = ToastType.Success }));
                     Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void CreateOrder(SupplierRestockGroup group)
        {
            if (group == null) return;

            // Generate Order
            var order = _orderManager.CreateNewOrderTemplate(OrderType.PurchaseOrder);
            order.SupplierName = group.SupplierName;
            // Try to set ID if we can find it, but name is enough for display usually.
            // Ideally we'd look up the supplier to get the ID.
            // We can do that asynchronously or just ignore ID if create view handles "Name only" -> "Select Supplier".
            // But CreateView validates "SelectedSupplier" which is an object.
            // We'll trust that CreateOrderView tries to match SupplierName string to an object on load.
            // (Checking CreateOrderViewModel: OnSelectedInventoryItemChanged tries to set SelectedSupplier by name match. 
            // LoadExistingOrder tries to set by ID.
            // If we just pass a fake order with no ID, it might not select the supplier in the dropdown.
            // We should try to lookup the supplier ID if possible or pass it via state??
            // Actually, we can just rely on the user selecting it if it's missing, but better to pre-fill.
            // Since we don't have Supplier ID in InventoryItem (just name), we rely on string match.
            // CreateOrderViewModel does not have logic to auto-select Supplier from String Name on 'LoadExistingOrder' unless we add it.
            // Wait, CreateOrderViewModel -> LoadExistingOrder DOES NOT do name matching.
            // I should add name matching there or do it here.
            // I'll do it here: Get Suppliers, find match.
            
            // Async workaround:
            PrepareOrderAndNavigate(group);
        }

        private async void PrepareOrderAndNavigate(SupplierRestockGroup group)
        {
            IsBusy = true;
            try
            {
                var suppliers = await _orderManager.GetSuppliersAsync();
                var supplierObj = suppliers.FirstOrDefault(s => s.Name.Equals(group.SupplierName, StringComparison.OrdinalIgnoreCase));

                var order = _orderManager.CreateNewOrderTemplate(OrderType.PurchaseOrder);
                order.Notes = $"Restock for {group.SupplierName}";
                order.ExpectedDeliveryDate = DateTime.Today.AddDays(7); // Default
                
                if (supplierObj != null)
                {
                    order.SupplierId = supplierObj.Id;
                    order.SupplierName = supplierObj.Name;
                    order.EntityAddress = supplierObj.Address;
                    order.EntityTel = supplierObj.Phone;
                    order.Attention = supplierObj.ContactPerson;
                }
                else
                {
                    order.SupplierName = group.SupplierName;
                }

                foreach (var candidate in group.Items)
                {
                    var item = candidate.Item;
                    // Calculate Order Qty
                    // Target = ReorderPoint * 2 (as per original logic in Manager)
                    // Or just ReorderPoint - (Hand + Order)?
                    // User didn't specify formula, so I'll stick to the "Safe Buffer" logic: Target = ReorderPoint * 2.
                    double target = item.ReorderPoint * 2;
                    double needed = target - (item.QuantityOnHand + candidate.QuantityOnOrder);
                    if (needed < 1) needed = 1;

                    order.Lines.Add(new OrderLine
                    {
                        InventoryItemId = item.Id,
                        ItemCode = item.Sku,
                        Description = item.ProductName,
                        Category = item.Category,
                        UnitOfMeasure = item.UnitOfMeasure,
                        UnitPrice = item.AverageCost,
                        QuantityOrdered = needed,
                        LineTotal = (decimal)needed * item.AverageCost,
                        VatAmount = ((decimal)needed * item.AverageCost) * 0.15m
                    });
                }

                _orderStateService.SaveState(order, null);
                WeakReferenceMessenger.Default.Send(new NavigationRequestMessage("CreateOrder"));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void Close()
        {
             WeakReferenceMessenger.Default.Send(new NavigationRequestMessage("Orders"));
        }
    }

    public class SupplierRestockGroup
    {
        public string SupplierName { get; }
        public ObservableCollection<RestockCandidate> Items { get; }

        public SupplierRestockGroup(string name, System.Collections.Generic.List<RestockCandidate> items)
        {
            SupplierName = string.IsNullOrEmpty(name) ? "Unknown Supplier" : name;
            Items = new ObservableCollection<RestockCandidate>(items);
        }
        
        // Helper for UI badges
        public int Count => Items.Count;
    }
}
