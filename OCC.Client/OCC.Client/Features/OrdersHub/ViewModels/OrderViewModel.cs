using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Client.Features.HomeHub.ViewModels.Shared;
using OCC.Shared.Models;
using OCC.Shared.DTOs;
using System;
using System.Threading.Tasks;

namespace OCC.Client.Features.OrdersHub.ViewModels
{
    /// <summary>
    /// Root ViewModel for the Orders module, orchestrating navigation between the dashboard, order list,
    /// inventory management, and supplier directories. Manages complex view states and popup visibility.
    /// </summary>
    public partial class OrderViewModel : ViewModelBase, IRecipient<OCC.Client.Messages.NavigationRequestMessage>, IRecipient<OCC.Client.ViewModels.Messages.SwitchTabMessage>
    {
        #region Private Members

        private readonly IOrderManager _orderManager;
        private readonly IDialogService _dialogService;
        private readonly Microsoft.Extensions.Logging.ILogger<OrderViewModel> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Services.Infrastructure.OrderStateService _orderStateService;

        #endregion

        #region Observables

        /// <summary>
        /// Gets the ViewModel for the high-level dashboard view.
        /// </summary>
        public OrderDashboardViewModel DashboardVM { get; }
        
        /// <summary>
        /// Gets the ViewModel for the master list of all orders.
        /// </summary>
        public OrderListViewModel OrderListVM { get; }
        
        /// <summary>
        /// Gets the ViewModel for creating or viewing detailed purchase orders.
        /// </summary>
        public CreateOrderViewModel CreateOrderVM { get; }

        /// <summary>
        /// Alias for CreateOrderVM for view binding compatibility.
        /// </summary>
        public CreateOrderViewModel OrderDetailVM => CreateOrderVM;
        
        /// <summary>
        /// Gets the ViewModel for the module's sub-navigation menu.
        /// </summary>
        public OrderMenuViewModel OrderMenu { get; }
        
        /// <summary>
        /// Gets the ViewModel for the master inventory list.
        /// </summary>
        public ItemListViewModel ItemListVM { get; }

        /// <summary>
        /// Gets the ViewModel for the live inventory stock view.
        /// </summary>
        public InventoryViewModel InventoryVM { get; }
        
        /// <summary>
        /// Gets the ViewModel for managing the supplier directory.
        /// </summary>
        public SupplierListViewModel SupplierListVM { get; }

        /// <summary>
        /// Gets or sets the currently displayed sub-view ViewModel.
        /// </summary>
        [ObservableProperty]
        private ViewModelBase? _currentView;

        /// <summary>
        /// Gets or sets the visibility state of the supplier detail editing popup.
        /// </summary>
        [ObservableProperty]
        private bool _isSupplierDetailVisible;

        /// <summary>
        /// Gets or sets the visibility state of the order receiving popup.
        /// </summary>
        [ObservableProperty]
        private bool _isReceiveOrderVisible;

        /// <summary>
        /// Gets or sets the visibility state of the order detail popup (using CreateOrderVM).
        /// </summary>
        [ObservableProperty]
        private bool _isOrderDetailVisible;

        /// <summary>
        /// Gets the ViewModel for editing supplier details.
        /// </summary>
        [ObservableProperty]
        private SupplierDetailViewModel _supplierDetailVM;

        /// <summary>
        /// Gets the ViewModel for processing order arrivals.
        /// </summary>
        [ObservableProperty]
        private ReceiveOrderViewModel _receiveOrderVM;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderViewModel"/> class with all required dependencies.
        /// Orchestrates the initialization of all child ViewModels within the module.
        /// </summary>
        public OrderViewModel(
            IOrderManager orderManager, 
            IDialogService dialogService, 
            Microsoft.Extensions.Logging.ILogger<OrderViewModel> logger,
            IServiceProvider serviceProvider,
            Services.Infrastructure.OrderStateService orderStateService)
        {
            _orderManager = orderManager;
            _dialogService = dialogService;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _orderStateService = orderStateService;

            // Initialize all child ViewModels using the DI container for consistency
            DashboardVM = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<OrderDashboardViewModel>(_serviceProvider);
            OrderListVM = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<OrderListViewModel>(_serviceProvider);
            CreateOrderVM = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<CreateOrderViewModel>(_serviceProvider);
            ItemListVM = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ItemListViewModel>(_serviceProvider);
            InventoryVM = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<InventoryViewModel>(_serviceProvider);
            SupplierListVM = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<SupplierListViewModel>(_serviceProvider);
            OrderMenu = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<OrderMenuViewModel>(_serviceProvider);
            
            // Initialize overlay ViewModels
            _supplierDetailVM = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<SupplierDetailViewModel>(_serviceProvider);
            _receiveOrderVM = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ReceiveOrderViewModel>(_serviceProvider);

            // Set the initial startup view
            CurrentView = DashboardVM;

            // Wire up navigation and interaction events across the module
            SetupEventWiring();

            // Register for global navigation requests affecting the Orders module
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<OCC.Client.Messages.NavigationRequestMessage>(this);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to switch the active tab in the Orders module.
        /// </summary>
        /// <param name="tabName">The identifier of the tab to switch to.</param>
        [RelayCommand]
        public void SetTab(string tabName)
        {
            // Always close overlays when switching main tabs
            IsOrderDetailVisible = false;
            IsSupplierDetailVisible = false;
            IsReceiveOrderVisible = false;

            switch (tabName)
            {
                case "Dashboard": 
                    if (CurrentView == DashboardVM) return;
                    CurrentView = DashboardVM; 
                    _ = DashboardVM.LoadData();
                    break;
                case "OrderList": 
                case "All Orders":
                    if (CurrentView == OrderListVM) return;
                    CurrentView = OrderListVM; 
                    OrderListVM.LoadOrders(); 
                    break;
                case "CreateOrder": 
                case "New Order":
                    if (CurrentView == CreateOrderVM) return;
                    CurrentView = CreateOrderVM;
                    // Open as a standard view instead of a popup so navigation remains accessible
                    CreateOrderVM.Reset();
                    CreateOrderVM.IsReadOnly = false;
                    _ = CreateOrderVM.LoadData(); 
                    break;
                case "Inventory": 
                    if (CurrentView == InventoryVM) return;
                    CurrentView = InventoryVM;
                    _ = InventoryVM.LoadInventoryAsync();
                    break;
                case "ItemList":
                    if (CurrentView == ItemListVM) return;
                    CurrentView = ItemListVM; 
                    _ = ItemListVM.LoadItemsAsync(); 
                    break;
                case "Suppliers": 
                    if (CurrentView == SupplierListVM) return;
                    CurrentView = SupplierListVM; 
                    _ = SupplierListVM.LoadData(); 
                    break;
            }
        }

        /// <summary>
        /// Opens the order detail popup for a new order.
        /// </summary>
        [RelayCommand]
        public async Task OpenNewOrder()
        {
            CreateOrderVM.Reset();
            CreateOrderVM.IsReadOnly = false;
            await CreateOrderVM.LoadData(); 
            IsOrderDetailVisible = true;
        }

        /// <summary>
        /// Opens the order receiving popup for the specified order.
        /// </summary>
        /// <param name="order">The order to receive.</param>
        [RelayCommand]
        public void NavigateToReceiveOrder(Order order)
        {
            ReceiveOrderVM.LoadOrder(order);
            IsReceiveOrderVisible = true;
        } 

        #endregion

        #region Methods

        /// <summary>
        /// Opens the order detail popup for an existing order in read-only mode.
        /// </summary>
        /// <param name="order">The order to view.</param>
        public async Task OpenViewOrder(Order order)
        {
            await CreateOrderVM.LoadData(); 
            await CreateOrderVM.LoadExistingOrder(order);
            IsOrderDetailVisible = true;
        }
        
        /// <summary>
        /// Fetches the full order via ID and opens it.
        /// </summary>
        public async Task LoadAndOpenOrder(Guid orderId)
        {
            IsBusy = true;
            try
            {
                var order = await _orderManager.GetOrderByIdAsync(orderId);
                if (order != null)
                {
                    await OpenViewOrder(order);
                }
                else 
                {
                   await _dialogService.ShowAlertAsync("Error", "Order not found.");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Establishes event links between ViewModels to handle complex interactions like opening popups from lists.
        /// </summary>
        private void SetupEventWiring()
        {
            // Supplier List Interactions
            SupplierListVM.AddSupplierRequested += (s, e) => { SupplierDetailVM.Load(null); IsSupplierDetailVisible = true; };
            SupplierListVM.EditSupplierRequested += (s, o) => { SupplierDetailVM.Load(o); IsSupplierDetailVisible = true; };
            SupplierDetailVM.CloseRequested += (s, e) => IsSupplierDetailVisible = false;
            SupplierDetailVM.Saved += async (s, e) => { await SupplierListVM.LoadData(); IsSupplierDetailVisible = false; };

            // Order List Interactions
            OrderListVM.ReceiveOrderRequested += async (s, summarry) => 
            {
                var order = await _orderManager.GetOrderByIdAsync(summarry.Id);
                if (order != null) 
                {
                    ReceiveOrderVM.LoadOrder(order); 
                    IsReceiveOrderVisible = true; 
                }
            };
            
            OrderListVM.ViewOrderRequested += async (s, summarry) => await LoadAndOpenOrder(summarry.Id);
            
            // Order Receipt Interactions
            ReceiveOrderVM.CloseRequested += (s, e) => IsReceiveOrderVisible = false;
            ReceiveOrderVM.OrderReceived += (s, e) => 
            { 
                IsReceiveOrderVisible = false; 
                OrderListVM.LoadOrders(); 
                _ = InventoryVM.LoadInventoryAsync();
                _ = ItemListVM.LoadItemsAsync();
            };
            
            // Create Order Logic: Handle successful creation by returning to the list
            CreateOrderVM.OrderCreated += (s, e) => { IsOrderDetailVisible = false; SetTab("OrderList"); };
            CreateOrderVM.CloseRequested += (s, e) => IsOrderDetailVisible = false;

            // Menu Interactions
            OrderMenu.TabSelected += (s, tab) => SetTab(tab);

            // Dashboard Interactions
            DashboardVM.OrderSelected += async (s, summarry) => await LoadAndOpenOrder(summarry.Id);
        }

        /// <summary>
        /// Handles global navigation requests for the Orders module.
        /// </summary>
        public void Receive(OCC.Client.Messages.NavigationRequestMessage message)
        {
            if (message.Value == "CreateOrder") SetTab("CreateOrder");
            if (message.Value == "OrderList") SetTab("OrderList");
            if (message.Value == "Suppliers") SetTab("Suppliers");
            if (message.Value == "Inventory") SetTab("Inventory");
        }

        public void Receive(OCC.Client.ViewModels.Messages.SwitchTabMessage message)
        {
             if (message.Value == "Suppliers") SetTab("Suppliers");
        }

        #endregion
    }
}


