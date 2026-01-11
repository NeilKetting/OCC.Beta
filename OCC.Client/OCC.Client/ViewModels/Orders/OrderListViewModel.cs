using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Core;
using OCC.Client.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace OCC.Client.ViewModels.Orders
{
    public partial class OrderListViewModel : ViewModelBase, IRecipient<Messages.EntityUpdatedMessage>
    {
        private readonly IOrderService _orderService;
        private readonly IDialogService _dialogService;
        private List<Order> _allOrders = new();

        public ObservableCollection<Order> Orders { get; } = new();
        
        public event EventHandler<Order>? ReceiveOrderRequested;

        [ObservableProperty]
        private string _searchQuery = "";

        [ObservableProperty]
        private bool _isBusy;
        
        [ObservableProperty]
        private Order? _selectedOrder;

        public OrderListViewModel(IOrderService orderService, IDialogService dialogService)
        {
            _orderService = orderService;
            _dialogService = dialogService;
            
            // Register for Real-time Updates
            WeakReferenceMessenger.Default.Register(this);
            
            LoadOrders();
        }

        public void Receive(Messages.EntityUpdatedMessage message)
        {
            if (message.Value.EntityType == "Order")
            {
                // Refresh list if Orders change (e.g. Status update from Receive)
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(LoadOrders);
            }
        }

        // public OrderListViewModel() { } // Design-time

        public async void LoadOrders()
        {
            try
            {
                IsBusy = true;
                _allOrders = await _orderService.GetOrdersAsync();
                FilterOrders();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading orders: {ex.Message}");
                if (_dialogService != null) 
                    await _dialogService.ShowAlertAsync("Error", $"Critical Error loading orders: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSearchQueryChanged(string value)
        {
            FilterOrders();
        }

        [ObservableProperty]
        private OrderDateFilter _timeFilter = OrderDateFilter.All;

        [ObservableProperty]
        private decimal _filteredTotal;

        public List<string> TimeFilters { get; } = Enum.GetNames(typeof(OrderDateFilter)).Select(f => f.Replace("_", " ")).ToList();

        // Helper to convert string back if binding to string
        partial void OnTimeFilterChanged(OrderDateFilter value)
        {
            FilterOrders();
        }

        private void FilterOrders()
        {
            Orders.Clear();
            
            var query = _allOrders.AsEnumerable();

            // 1. Text Search
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                query = query.Where(o => o.OrderNumber.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) 
                                      || (o.SupplierName?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            // 2. Date Filter
            var now = DateTime.Now.Date;
            switch (TimeFilter)
            {
                case OrderDateFilter.Today:
                    query = query.Where(o => o.OrderDate.Date == now);
                    break;
                case OrderDateFilter.Yesterday:
                    query = query.Where(o => o.OrderDate.Date == now.AddDays(-1));
                    break;
                case OrderDateFilter.This_Week:
                    // Fix for Sunday (0) to map to 7 for Monday-based week
                    int dayDiff = (int)now.DayOfWeek == 0 ? 6 : (int)now.DayOfWeek - 1; 
                    var startOfWeek = now.AddDays(-dayDiff);
                    var endOfWeek = startOfWeek.AddDays(7);
                    query = query.Where(o => o.OrderDate.Date >= startOfWeek && o.OrderDate.Date < endOfWeek);
                    break;
                 case OrderDateFilter.Last_Week:
                    int dayDiffLast = (int)now.DayOfWeek == 0 ? 6 : (int)now.DayOfWeek - 1;
                    var startOfLastWeek = now.AddDays(-dayDiffLast).AddDays(-7);
                    var endOfLastWeek = startOfLastWeek.AddDays(7);
                    query = query.Where(o => o.OrderDate.Date >= startOfLastWeek && o.OrderDate.Date < endOfLastWeek);
                    break;
                case OrderDateFilter.This_Month:
                    var startOfMonth = new DateTime(now.Year, now.Month, 1);
                    query = query.Where(o => o.OrderDate.Date >= startOfMonth && o.OrderDate.Date < startOfMonth.AddMonths(1));
                    break;
                case OrderDateFilter.Last_Month:
                    var startOfLastMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                    query = query.Where(o => o.OrderDate.Date >= startOfLastMonth && o.OrderDate.Date < startOfLastMonth.AddMonths(1));
                    break;
                case OrderDateFilter.All:
                default:
                    break;
            }

            var result = query.OrderByDescending(o => o.OrderDate).ToList(); // Sort by newest first

            foreach (var order in result)
            {
                Orders.Add(order);
            }

            FilteredTotal = result.Sum(o => o.TotalAmount);
        }

        [RelayCommand]
        public async Task DeleteOrder(Order order)
        {
            if (order == null) return;

            try 
            {
                await _orderService.DeleteOrderAsync(order.Id);
                Orders.Remove(order);
                _allOrders.Remove(order);
            }
            catch(Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"Error deleting order: {ex.Message}");
            }
        }
        
        [RelayCommand]
        public void RequestReceiveOrder(Order order)
        {
            if (order == null) return;
            ReceiveOrderRequested?.Invoke(this, order);
        }

        public event EventHandler<Order>? ViewOrderRequested;

        [RelayCommand]
        public void ViewOrder(Order order)
        {
            if (order == null) return;
            ViewOrderRequested?.Invoke(this, order);
        }
    }

    public enum OrderDateFilter
    {
        All,
        Today,
        Yesterday,
        This_Week,
        Last_Week,
        This_Month,
        Last_Month
    }
}
