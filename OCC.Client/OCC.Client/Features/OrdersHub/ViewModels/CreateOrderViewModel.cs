using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using OCC.Client.Messages;
using OCC.Client.ModelWrappers; // NEW
using OCC.Client.Services;
using OCC.Client.Services.Infrastructure;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Features.OrdersHub.UseCases;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.Features.OrdersHub.ViewModels
{
    /// <summary>
    /// ViewModel for creating, editing, and viewing purchase orders, sales orders, and returns.
    /// Manages complex order states, line item calculations, and coordinates with inventory and supplier services via the Order Manager.
    /// </summary>
    public partial class CreateOrderViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IOrderManager _orderManager;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;
        private readonly OrderStateService _orderStateService;
        private readonly ILogger<CreateOrderViewModel> _logger;
        private readonly IPdfService _pdfService;
        private readonly IOrderLifecycleService _lifecycle;
        private readonly OrderSubmissionUseCase _submissionUseCase;

        private bool _isNewOrder;

        #endregion

        #region Observables

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SubmitButtonText))]
        private OrderWrapper _currentOrder = null!;

        public string SubmitButtonText => !_isNewOrder ? "UPDATE ORDER" : "CREATE ORDER";

        [ObservableProperty]
        private bool _isReadOnly;

        [ObservableProperty]
        private InventoryLookupViewModel _inventory;

        [ObservableProperty]
        private SupplierSelectorViewModel _suppliers;

        [ObservableProperty]
        private OrderLinesViewModel _lines;

        public ObservableCollection<Customer> Customers { get; } = new();
        public ObservableCollection<Project> Projects { get; } = new();
        
        public OrderMenuViewModel OrderMenu { get; }

        public List<string> AvailableUOMs { get; } = new() { "ea", "m", "kg", "L", "m2", "m3", "box", "roll", "pack" };
        public List<Branch> Branches { get; } = Enum.GetValues<Branch>().Cast<Branch>().ToList();

        [ObservableProperty]
        private Customer? _selectedCustomer;

        [ObservableProperty]
        private bool _isPurchaseOrder;
        
        [ObservableProperty]
        private bool _isSalesOrder;
        
        [ObservableProperty]
        private bool _isReturnOrder;

        [ObservableProperty]
        private bool _isOfficeDelivery = true;
        
        [ObservableProperty]
        private bool _isSiteDelivery;
        
        [ObservableProperty]
        [property: CustomValidation(typeof(CreateOrderViewModel), nameof(ValidateProjectSelection))]
        private Project? _selectedProject;

        [ObservableProperty]
        private bool _shouldPrintOrder;

        [ObservableProperty]
        private bool _shouldEmailOrder;

        [ObservableProperty]
        private bool _isAddingProjectAddress;
        
        [ObservableProperty]
        private string _newProjectAddress = string.Empty;

        #endregion

        #region Constructors

        public CreateOrderViewModel(
            IOrderManager orderManager,
            IDialogService dialogService,
            IAuthService authService,
            ILogger<CreateOrderViewModel> logger,
            IPdfService pdfService,
            OrderStateService orderStateService,
            IOrderLifecycleService lifecycle,
            OrderSubmissionUseCase submissionUseCase,
            OrderMenuViewModel orderMenu,
            OrderLinesViewModel lines,
            InventoryLookupViewModel inventory,
            SupplierSelectorViewModel suppliers)
        {
            _orderManager = orderManager;
            _dialogService = dialogService;
            _authService = authService;
            _logger = logger;
            _pdfService = pdfService;
            _orderStateService = orderStateService;
            _lifecycle = lifecycle;
            _submissionUseCase = submissionUseCase;

            OrderMenu = orderMenu;
            Lines = lines;
            Inventory = inventory;
            Suppliers = suppliers;

            OrderMenu.TabSelected += OnMenuTabSelected;
            
            // Forward selection changes
            Inventory.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(Inventory.SelectedItem)) OnSelectedInventoryItemChanged(Inventory.SelectedItem);
            };
            Suppliers.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(Suppliers.SelectedSupplier)) OnSelectedSupplierChanged(Suppliers.SelectedSupplier);
            };

            Reset();
        }

        public CreateOrderViewModel() 
        {
            _orderManager = null!;
            _dialogService = null!;
            _authService = null!;
            _logger = null!;
            _pdfService = null!;
            _orderStateService = null!;
            _lifecycle = null!;
            _submissionUseCase = null!;
            OrderMenu = null!;
            Inventory = null!;
            Suppliers = null!;
            CurrentOrder = new OrderWrapper(new Order { OrderNumber = "DEMO-0000" });
        }


        #endregion

        #region Commands

        [RelayCommand]
        public void ChangeOrderType(OrderType type)
        {
             CurrentOrder.OrderType = type;
             var temp = _orderManager.CreateNewOrderTemplate(type);
             CurrentOrder.OrderNumber = temp.OrderNumber;
             UpdateOrderTypeFlags();
             OnPropertyChanged(nameof(CurrentOrder));
        }

        [RelayCommand] public void ToggleQuickAddProduct() => Inventory.ToggleQuickAdd();
        [RelayCommand] public void ToggleNewCategoryMode() => Inventory.ToggleNewCategoryMode();
        [RelayCommand] public void ToggleQuickAddSupplier() => Suppliers.ToggleQuickAdd();
        [RelayCommand] public async Task QuickCreateProduct() => await Inventory.QuickCreateProduct(Suppliers.SelectedSupplier?.Name);
        [RelayCommand] public async Task QuickCreateSupplier() => await Suppliers.QuickCreateSupplier();

        [RelayCommand]
        public async Task PreviewOrder()
        {
             try
             {
                 if (Suppliers.SelectedSupplier == null && IsPurchaseOrder)
                 {
                      await _dialogService.ShowAlertAsync("Validation", "Please select a supplier first.");
                      return;
                 }
                 var path = await _pdfService.GenerateOrderPdfAsync(GetSanitizedOrder());
                 new System.Diagnostics.Process { StartInfo = new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true } }.Start();
             }
             catch(Exception ex)
             {
                 _logger.LogError(ex, "Failed to preview order");
                 await _dialogService.ShowAlertAsync("Error", "Failed to generate print preview.");
             }
        }

        [RelayCommand]
        public async Task DownloadPrint()
        {
             try
             {
                 if (Suppliers.SelectedSupplier == null && IsPurchaseOrder)
                 {
                      await _dialogService.ShowAlertAsync("Validation", "Please select a supplier first.");
                      return;
                 }
                 var path = await _pdfService.GenerateOrderPdfAsync(GetSanitizedOrder(), isPrintVersion: true);
                 new System.Diagnostics.Process { StartInfo = new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true } }.Start();
             }
             catch(Exception ex)
             {
                 _logger.LogError(ex, "Failed to generate print version");
                  await _dialogService.ShowAlertAsync("Error", "Failed to generate grayscale version.");
             }
        }
        
        [RelayCommand]
        public async Task EmailOrder()
        {
             try
             {
                 if (Suppliers.SelectedSupplier == null && IsPurchaseOrder)
                 {
                      await _dialogService.ShowAlertAsync("Validation", "Please select a supplier first.");
                      return;
                 }
                 var path = await _pdfService.GenerateOrderPdfAsync(GetSanitizedOrder(), isPrintVersion: false);
                 new System.Diagnostics.Process { StartInfo = new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true } }.Start();
             }
             catch(Exception ex)
             {
                 _logger.LogError(ex, "Failed to generate email version");
                 await _dialogService.ShowAlertAsync("Error", "Failed to generate color version.");
             }
        }

        private Order GetSanitizedOrder()
        {
            CurrentOrder.CommitToModel();
            var model = CurrentOrder.Model;
            var meaningfulLines = model.Lines
                .Where(l => !string.IsNullOrWhiteSpace(l.ItemCode) || !string.IsNullOrWhiteSpace(l.Description))
                .Where(l => l.QuantityOrdered > 0)
                .ToList();
            model.Lines = new ObservableCollection<OrderLine>(meaningfulLines);
            return model;
        }

        [RelayCommand(CanExecute = nameof(CanSubmitOrder))]
        public async Task SubmitOrder()
        {
            if (IsBusy) return;

            var options = new UseCases.OrderSubmissionOptions(ShouldPrintOrder, ShouldEmailOrder, _isNewOrder);
            var (success, result) = await _submissionUseCase.ExecuteAsync(CurrentOrder, options);

            if (success)
            {
                OrderCreated?.Invoke(this, EventArgs.Empty);
                Reset();
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        [RelayCommand]
        public void Cancel()
        {
            Reset();
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        public void ToggleAddProjectAddress() => IsAddingProjectAddress = !IsAddingProjectAddress;

        [RelayCommand]
        public async Task SaveProjectAddress()
        {
             if (string.IsNullOrWhiteSpace(NewProjectAddress) || SelectedProject == null) return;
             
             try
             {
                 var project = await _orderManager.GetProjectByIdAsync(SelectedProject.Id);
                 if (project != null)
                 {
                     project.StreetLine1 = NewProjectAddress;
                     await _orderManager.UpdateProjectAsync(project);
                     SelectedProject.StreetLine1 = NewProjectAddress;
                     CurrentOrder.EntityAddress = NewProjectAddress;
                     OnPropertyChanged(nameof(CurrentOrder));
                     IsAddingProjectAddress = false;
                 }
             }
             catch(Exception ex)
             {
                 _logger.LogError(ex, "Failed to update project address");
                 await _dialogService.ShowAlertAsync("Error", "Failed to save project address.");
             }
        }

        #endregion

        #region Methods

        public event EventHandler? OrderCreated;
        public event EventHandler? CloseRequested;

        private void Initialize()
        {
            ErrorsChanged -= OnErrorsChanged;
            ErrorsChanged += OnErrorsChanged;
            CurrentOrder.ErrorsChanged -= OnCurrentOrderErrorsChanged;
            CurrentOrder.ErrorsChanged += OnCurrentOrderErrorsChanged;
            
            ValidateAllProperties();
            CurrentOrder.Validate();
            
            SubmitOrderCommand.NotifyCanExecuteChanged();
        }

        private void OnErrorsChanged(object? sender, System.ComponentModel.DataErrorsChangedEventArgs e) => SubmitOrderCommand.NotifyCanExecuteChanged();
        private void OnCurrentOrderErrorsChanged(object? sender, System.ComponentModel.DataErrorsChangedEventArgs e) => SubmitOrderCommand.NotifyCanExecuteChanged();

        public bool CanSubmitOrder => !HasErrors && !CurrentOrder.HasErrors;

        public async Task LoadData() => await _lifecycle.LoadInitialDataAsync(this);
        public void Reset() => _lifecycle.Reset(this);
        public async Task LoadExistingOrder(Order order) => await _lifecycle.LoadOrderAsync(this, order);

        public void PrepareForOrder(Order order, bool isNew = false)
        {
            _isNewOrder = isNew;
            CurrentOrder = new OrderWrapper(order);
            
            if (CurrentOrder.SupplierId.HasValue)
                Suppliers.SelectedSupplier = Suppliers.FilteredSuppliers.FirstOrDefault(s => s.Id == CurrentOrder.SupplierId.Value);
            else if (!string.IsNullOrEmpty(CurrentOrder.SupplierName))
                Suppliers.SelectedSupplier = Suppliers.FilteredSuppliers.FirstOrDefault(s => s.Name.Equals(CurrentOrder.SupplierName, StringComparison.OrdinalIgnoreCase));

            if (CurrentOrder.CustomerId.HasValue)
                SelectedCustomer = Customers.FirstOrDefault(c => c.Id == CurrentOrder.CustomerId.Value);

            if (CurrentOrder.ProjectId.HasValue)
                SelectedProject = Projects.FirstOrDefault(p => p.Id == CurrentOrder.ProjectId.Value);

            IsSiteDelivery = CurrentOrder.DestinationType == OrderDestinationType.Site;
            IsOfficeDelivery = !IsSiteDelivery;
            IsReadOnly = CurrentOrder.Status == OrderStatus.Completed || CurrentOrder.Status == OrderStatus.Cancelled;

            Lines.IsReadOnly = IsReadOnly;
            Lines.Initialize(CurrentOrder, Enumerable.Empty<InventoryItem>()); // Master list is managed centraly

            UpdateOrderTypeFlags();
            OnPropertyChanged(nameof(SubmitButtonText));
            Initialize();
        }

        private void UpdateOrderTypeFlags()
        {
            IsPurchaseOrder = CurrentOrder.OrderType == OrderType.PurchaseOrder;
            IsSalesOrder = CurrentOrder.OrderType == OrderType.SalesOrder;
            IsReturnOrder = CurrentOrder.OrderType == OrderType.ReturnToInventory;
        }

        public static ValidationResult? ValidateProjectSelection(Project? project, ValidationContext context)
        {
            var vm = (CreateOrderViewModel)context.ObjectInstance;
            return (vm.IsSalesOrder && project == null) ? new ValidationResult("Project is required.") : ValidationResult.Success;
        }

        partial void OnCurrentOrderChanged(OrderWrapper value) 
        {
            if (value != null)
            {
                value.PropertyChanged -= OnOrderPropertyChanged;
                value.PropertyChanged += OnOrderPropertyChanged;
            }
            UpdateOrderTypeFlags();
            Suppliers.Filter(CurrentOrder?.Branch ?? Branch.CPT);
        }

        private void OnOrderPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OrderWrapper.Branch))
            {
                Suppliers.Filter(CurrentOrder?.Branch ?? Branch.CPT);
            }
        }

        partial void OnIsOfficeDeliveryChanged(bool value) { if (value) { CurrentOrder.DestinationType = OrderDestinationType.Stock; IsSiteDelivery = false; } }
        partial void OnIsSiteDeliveryChanged(bool value) { if (value) { CurrentOrder.DestinationType = OrderDestinationType.Site; IsOfficeDelivery = false; if (SelectedProject != null) CurrentOrder.EntityAddress = SelectedProject.StreetLine1; } }
        
        partial void OnSelectedProjectChanged(Project? value)
        {
             if (value != null)
             {
                 CurrentOrder.ProjectId = value.Id;
                 CurrentOrder.ProjectName = value.Name;
                 if (IsSalesOrder || IsSiteDelivery) CurrentOrder.EntityAddress = value.StreetLine1;
                 OnPropertyChanged(nameof(CurrentOrder));
             }
             else
             {
                 CurrentOrder.ProjectId = null;
                 CurrentOrder.ProjectName = string.Empty;
             }
        }

        [RelayCommand]
        public async Task ValidateItemSearch(string searchText)
        {
             if (string.IsNullOrWhiteSpace(searchText)) return;
             
             // This might still need local Inventory list or delegate to Inventory VM
             // For now, delegate to Inventory VM?
             // Inventory.ValidateItemSearch is not implemented yet.
             
             var exists = Inventory.FilteredItems.Any(i => i.Sku.Equals(searchText, StringComparison.OrdinalIgnoreCase) || i.Description.Equals(searchText, StringComparison.OrdinalIgnoreCase));
             if (!exists)
             {
                 bool create = await _dialogService.ShowConfirmationAsync("Item Not Found", $"'{searchText}' not found.\n\nCreate it now?");
                 if (create)
                 {
                     _orderStateService.SaveState(CurrentOrder.Model, Lines.NewLine.Model, searchText);
                     WeakReferenceMessenger.Default.Send(new NavigationRequestMessage("InventoryDetail_Create", searchText));
                 }
             }
        }

        private void OnSelectedInventoryItemChanged(InventoryItem? value)
        {
            if (value != null)
            {
                Lines.NewLine = new OrderLineWrapper(new OrderLine 
                { 
                    InventoryItemId = value.Id, 
                    Description = value.Description ?? "", 
                    ItemCode = value.Sku ?? "", 
                    UnitOfMeasure = value.UnitOfMeasure ?? "", 
                    UnitPrice = value.AverageCost 
                });
                
                if (Suppliers.SelectedSupplier == null && !string.IsNullOrWhiteSpace(value.Supplier))
                {
                    var match = Suppliers.FilteredSuppliers.FirstOrDefault(s => s.Name.Equals(value.Supplier, StringComparison.OrdinalIgnoreCase));
                    if (match != null) Suppliers.SelectedSupplier = match;
                }
            }
        }

        private void OnSelectedSupplierChanged(Supplier? value)
        {
            if (value != null)
            {
                CurrentOrder.SupplierId = value.Id;
                CurrentOrder.SupplierName = value.Name;
                CurrentOrder.EntityAddress = value.Address;
                CurrentOrder.EntityTel = value.Phone;
                CurrentOrder.EntityVatNo = value.VatNumber;
                CurrentOrder.Attention = value.ContactPerson; 
                OnPropertyChanged(nameof(CurrentOrder));
                SubmitOrderCommand.NotifyCanExecuteChanged();
            }
        }

        private void OnMenuTabSelected(object? sender, string tabName)
        {
            switch (tabName)
            {
                case "Dashboard": WeakReferenceMessenger.Default.Send(new NavigationRequestMessage("Orders")); break;
                case "All Orders": WeakReferenceMessenger.Default.Send(new NavigationRequestMessage("OrderList")); break;
                case "Inventory": WeakReferenceMessenger.Default.Send(new NavigationRequestMessage("Inventory")); break;
                case "ItemList": WeakReferenceMessenger.Default.Send(new NavigationRequestMessage("ItemList")); break;
                case "Suppliers": WeakReferenceMessenger.Default.Send(new NavigationRequestMessage("Suppliers")); break;
                case "New Order": IsReadOnly = false; break;
            }
        }

        #endregion
    }
}
