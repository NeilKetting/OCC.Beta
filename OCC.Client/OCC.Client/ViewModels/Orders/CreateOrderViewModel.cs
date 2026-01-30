using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Core;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OCC.Client.Services.Infrastructure;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.ViewModels.Messages;
using OCC.Client.Models;
using OCC.Client.Messages;
using OCC.Client.ModelWrappers; // NEW
using Avalonia.Threading;

namespace OCC.Client.ViewModels.Orders
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
        private readonly Guid _instanceId = Guid.NewGuid();
        private readonly ILogger<CreateOrderViewModel> _logger;
        private readonly IPdfService _pdfService;
        private bool _isUpdatingSearchText;
        private List<Supplier> _allSuppliers = new();

        #endregion

        #region Observables



        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SubmitButtonText))]
        private OrderWrapper _currentOrder = null!; // Renamed from NewOrder

        /// <summary>
        /// Gets the localizable text for the submission button.
        /// </summary>
        public string SubmitButtonText => (CurrentOrder?.Id != Guid.Empty && CurrentOrder?.Id != null) ? "UPDATE ORDER" : "CREATE ORDER";

        [ObservableProperty]
        private bool _isReadOnly;

        /// <summary>
        /// Gets the current order subtotal from the model.
        /// </summary>
        // public decimal OrderSubTotal => NewOrder?.SubTotal ?? 0; // Removed, now binding to CurrentOrder.SubTotal
        
        /// <summary>
        /// Gets the current order VAT total from the model.
        /// </summary>
        // public decimal OrderVat => NewOrder?.VatTotal ?? 0; // Removed
        
        /// <summary>
        /// Gets the current order total amount from the model.
        /// </summary>
        // public decimal OrderTotal => NewOrder?.TotalAmount ?? 0; // Removed

        [ObservableProperty]
        private OrderLineWrapper _newLine = new(new OrderLine());
        
        [ObservableProperty]
        private ObservableCollection<Supplier> _suppliers = new();
        
        [ObservableProperty]
        private ObservableCollection<Customer> _customers = new();
        
        [ObservableProperty]
        private ObservableCollection<Project> _projects = new();
        
        [ObservableProperty]
        private ObservableCollection<InventoryItem> _inventoryItems = new();

        /// <summary>
        /// Gets the inventory items filtered by SKU for lookup.
        /// </summary>
        public ObservableCollection<InventoryItem> FilteredInventoryItemsBySku { get; } = new();
        
        /// <summary>
        /// Gets the inventory items filtered by name for lookup.
        /// </summary>
        public ObservableCollection<InventoryItem> FilteredInventoryItemsByName { get; } = new();

        [ObservableProperty]
        private string _skuSearchText = string.Empty;

        [ObservableProperty]
        private string _productSearchText = string.Empty;
        
        /// <summary>
        /// Gets the list of available units of measure.
        /// </summary>
        public List<string> AvailableUOMs { get; } = new() { "ea", "m", "kg", "L", "m2", "m3", "box", "roll", "pack" };
        
        /// <summary>
        /// Gets the list of available branches.
        /// </summary>
        public List<Branch> Branches { get; } = Enum.GetValues<Branch>().Cast<Branch>().ToList();
        
        /// <summary>
        /// Gets the list of product categories derived from inventory.
        /// </summary>
        public ObservableCollection<string> ProductCategories { get; } = new();
        
        [ObservableProperty]
        [property: CustomValidation(typeof(CreateOrderViewModel), nameof(ValidateSupplierSelection))]
        private Supplier? _selectedSupplier;
        
        [ObservableProperty]
        private Customer? _selectedCustomer;
        
        [ObservableProperty]
        private InventoryItem? _selectedInventoryItem;

        [ObservableProperty]
        private bool _isPurchaseOrder;
        
        [ObservableProperty]
        private bool _isSalesOrder;
        
        [ObservableProperty]
        private bool _isReturnOrder;

        [ObservableProperty]
        private bool _isAddingNewProduct;
        
        [ObservableProperty]
        private string _newDescription = string.Empty;

        [ObservableProperty]
        private string _newProductUOM = "ea";

        [ObservableProperty]
        private string _newProductCategory = "General";

        [ObservableProperty]
        private bool _isInputtingNewCategory;
        
        [ObservableProperty]
        private bool _isAddingNewSupplier;
        
        [ObservableProperty]
        private string _newSupplierName = string.Empty;
        
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



        [ObservableProperty]
        private bool _isSkuDropDownOpen;

        [ObservableProperty]
        private bool _isProductDropDownOpen;

        public OrderMenuViewModel OrderMenu { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateOrderViewModel"/> class with required dependencies.
        /// </summary>
        public CreateOrderViewModel(
            IOrderManager orderManager,
            IDialogService dialogService,
            IAuthService authService,
            ILogger<CreateOrderViewModel> logger,
            IPdfService pdfService,
            OrderStateService orderStateService,
            OrderMenuViewModel orderMenu)
        {
            _orderManager = orderManager;
            _dialogService = dialogService;
            _authService = authService;
            _logger = logger;
            _pdfService = pdfService;
            _orderStateService = orderStateService;
            OrderMenu = orderMenu;
            OrderMenu.TabSelected += OnMenuTabSelected;
            
            Reset();
        }

        /// <summary>
        /// Design-time constructor.
        /// </summary>
        public CreateOrderViewModel() 
        {
            _orderManager = null!;
            _dialogService = null!;
            _authService = null!;
            _logger = null!;
            _pdfService = null!;
            _orderStateService = null!;
            OrderMenu = null!;
            CurrentOrder = new OrderWrapper(new Order { OrderNumber = "DEMO-0000" });
        }

        private void RecalculateTotals()
        {
            // Model properties are computed (read-only), so we just notify the view to re-read them.
            OnPropertyChanged(nameof(CurrentOrder.SubTotal));
            OnPropertyChanged(nameof(CurrentOrder.VatTotal));
            OnPropertyChanged(nameof(CurrentOrder.TotalAmount));
        }

        private void OnLineChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OrderLineWrapper.LineTotal) || 
                e.PropertyName == nameof(OrderLineWrapper.ItemCode) || 
                e.PropertyName == nameof(OrderLineWrapper.Description))
            {
                RecalculateTotals();
                if (sender is OrderLineWrapper line)
                {
                    CheckForNewRow(line);
                }
            }
        }

        private void CheckForNewRow(OrderLineWrapper changedItem)
        {
            if (CurrentOrder?.Lines == null || IsReadOnly) return;

            var lastItem = CurrentOrder.Lines.LastOrDefault();
            if (lastItem == changedItem)
            {
                bool hasProduct = !string.IsNullOrWhiteSpace(lastItem.ItemCode);
                bool hasDesc = !string.IsNullOrWhiteSpace(lastItem.Description);
                bool hasPrice = lastItem.LineTotal > 0;

                if (hasProduct || hasDesc || hasPrice)
                {
                    var existsEmpty = CurrentOrder.Lines.Any(x => string.IsNullOrWhiteSpace(x.ItemCode) && 
                                                                string.IsNullOrWhiteSpace(x.Description) && 
                                                                x.LineTotal == 0);
                    
                    if (!existsEmpty)
                    {
                        CurrentOrder.Lines.Add(new OrderLineWrapper(new OrderLine { UnitOfMeasure = "ea" }));
                    }
                }
            }
        }

        private void SetupLineListeners()
        {
             // Remove old if any (naive approach, assume fresh start or handle in logic)
             CurrentOrder.Lines.CollectionChanged -= Lines_CollectionChanged;
             CurrentOrder.Lines.CollectionChanged += Lines_CollectionChanged;

             foreach(var line in CurrentOrder.Lines)
             {
                 line.PropertyChanged -= OnLineChanged;
                 line.PropertyChanged += OnLineChanged;
             }
        }

        private void Lines_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach(OrderLineWrapper item in e.NewItems) item.PropertyChanged += OnLineChanged;

            if (e.OldItems != null)
                foreach(OrderLineWrapper item in e.OldItems) item.PropertyChanged -= OnLineChanged;

            RecalculateTotals();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Changes the order type and updates the number prefix accordingly.
        /// </summary>
        [RelayCommand]
        public void ChangeOrderType(OrderType type)
        {
             CurrentOrder.OrderType = type;
             var temp = _orderManager.CreateNewOrderTemplate(type);
             CurrentOrder.OrderType = temp.OrderType;
             CurrentOrder.OrderNumber = temp.OrderNumber;
             UpdateOrderTypeFlags();
             OnPropertyChanged(nameof(CurrentOrder));
        }

        /// <summary>
        /// Adds a new line item to the order based on the current selection.
        /// </summary>
        [RelayCommand]
        public async Task AddLine()
        {
            if (string.IsNullOrWhiteSpace(NewLine.Description) && SelectedInventoryItem == null) return;

            NewLine.CalculateTotal(CurrentOrder.TaxRate);
            
            if (SelectedInventoryItem != null && SelectedInventoryItem.AverageCost > 0)
            {
                decimal threshold = SelectedInventoryItem.AverageCost * 1.10m;
                if (NewLine.UnitPrice > threshold)
                {
                    decimal pctIncrease = ((NewLine.UnitPrice - SelectedInventoryItem.AverageCost) / SelectedInventoryItem.AverageCost) * 100m;
                    bool confirm = await _dialogService.ShowConfirmationAsync(
                        "Price Increase Warning", 
                        $"The price of R{NewLine.UnitPrice:F2} is {pctIncrease:F0}% higher than average.\n\nAccept and update average cost?");
                    
                    if (confirm)
                    {
                        SelectedInventoryItem.AverageCost = NewLine.UnitPrice;
                        try { await _orderManager.UpdateInventoryItemAsync(SelectedInventoryItem); }
                        catch(Exception ex) { _logger.LogError(ex, "Failed to update avg cost"); }
                    }
                }
            }

            NewLine.CommitToModel();

            // Smart-add: if the first line is empty, replace it. Otherwise, find the first empty row or insert at 0.
            bool added = false;
            for (int i = 0; i < CurrentOrder.Lines.Count; i++)
            {
                var line = CurrentOrder.Lines[i];
                if (string.IsNullOrWhiteSpace(line.ItemCode) && string.IsNullOrWhiteSpace(line.Description) && line.QuantityOrdered == 0)
                {
                    CurrentOrder.Lines[i] = NewLine;
                    added = true;
                    break;
                }
            }

            if (!added)
            {
                CurrentOrder.Lines.Insert(0, NewLine);
            }
            
            // Reset New Line
            NewLine = new OrderLineWrapper(new OrderLine { QuantityOrdered = 1 });
        }        
        /// <summary>
        /// Removes a line item from the order.
        /// </summary>
        [RelayCommand]
        public void RemoveLine(object item)
        {
            if (item is OrderLineWrapper line)
            {
                CurrentOrder.Lines.Remove(line);
                
                // If we removed the last row and now it's empty, add one back
                if (CurrentOrder.Lines.Count == 0)
                {
                    AddEmptyLine();
                }
            }
            RecalculateTotals();
        }

        /// <summary>
        /// Inserts an empty line above the specified line.
        /// </summary>
        [RelayCommand]
        public void InsertLine(object parameter)
        {
            if (parameter is OrderLineWrapper anchor && CurrentOrder?.Lines != null)
            {
                var index = CurrentOrder.Lines.IndexOf(anchor);
                if (index >= 0)
                {
                    var newLine = new OrderLineWrapper(new OrderLine { QuantityOrdered = 1, UnitOfMeasure = "ea" });
                    newLine.PropertyChanged += OnLineChanged;
                    CurrentOrder.Lines.Insert(index, newLine);
                }
            }
        }

        /// <summary>
        /// Adds an empty line to the grid for editing.
        /// </summary>
        [RelayCommand]
        public void AddEmptyLine()
        {
            CurrentOrder.Lines.Add(new OrderLineWrapper(new OrderLine { QuantityOrdered = 1, UnitOfMeasure = "ea" }));
        }

        /// <summary>
        /// Updates a specific order line with details from a selected inventory item.
        /// </summary>
        public void UpdateLineFromSelection(OrderLineWrapper line, InventoryItem item)
        {
            if (line == null || item == null) return;

            // Update model
            line.InventoryItemId = item.Id;
            line.ItemCode = item.Sku;
            line.Description = item.Description;
            line.UnitOfMeasure = string.IsNullOrEmpty(item.UnitOfMeasure) ? "ea" : item.UnitOfMeasure;
            line.UnitPrice = item.AverageCost; 
            
            // Auto-set quantity to 1 if empty/zero to populate the row
            if (line.QuantityOrdered <= 0) line.QuantityOrdered = 1;

            line.CalculateTotal(CurrentOrder.TaxRate);

            // Collection refresh not needed as OrderLine now implements INotifyPropertyChanged
            
            OnPropertyChanged(nameof(CurrentOrder.SubTotal));
            OnPropertyChanged(nameof(CurrentOrder.VatTotal));
            OnPropertyChanged(nameof(CurrentOrder.TotalAmount));
        }
        
        /// <summary>
        /// Toggles the quick add product overlay.
        /// </summary>
        [RelayCommand]
        public void ToggleQuickAddProduct()
        {
            IsAddingNewProduct = !IsAddingNewProduct;
            if (IsAddingNewProduct)
            {
                NewDescription = "";
                NewProductUOM = "ea";
                NewProductCategory = "General";
                IsInputtingNewCategory = false;
            }
        }

        /// <summary>
        /// Toggles between selecting an existing category or entering a new one.
        /// </summary>
        [RelayCommand]
        public void ToggleNewCategoryMode()
        {
            IsInputtingNewCategory = !IsInputtingNewCategory;
            NewProductCategory = IsInputtingNewCategory ? "" : "General";
        }
        
        /// <summary>
        /// Toggles the quick add supplier overlay.
        /// </summary>
        [RelayCommand]
        public void ToggleQuickAddSupplier()
        {
            IsAddingNewSupplier = !IsAddingNewSupplier;
            if (IsAddingNewSupplier) NewSupplierName = "";
        }

        /// <summary>
        /// Creates a new product directly from the order entry view.
        /// </summary>
        [RelayCommand]
        public async Task QuickCreateProduct()
        {
            if (string.IsNullOrWhiteSpace(NewDescription)) return;
            
            try
            {
                var created = await _orderManager.QuickCreateProductAsync(NewDescription, NewProductUOM, NewProductCategory, SelectedSupplier?.Name ?? string.Empty);
                InventoryItems.Add(created);
                if (!ProductCategories.Contains(created.Category)) ProductCategories.Add(created.Category);
                SelectedInventoryItem = created;
                IsAddingNewProduct = false;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to quick create product");
                await _dialogService.ShowAlertAsync("Error", "Failed to create product.");
            }
        }
        
        /// <summary>
        /// Creates a new supplier directly from the order entry view.
        /// </summary>
        [RelayCommand]
        public async Task QuickCreateSupplier()
        {
            if (string.IsNullOrWhiteSpace(NewSupplierName)) return;
            
            try
            {
                 var created = await _orderManager.QuickCreateSupplierAsync(NewSupplierName);
                 Suppliers.Add(created);
                 SelectedSupplier = created;
                 IsAddingNewSupplier = false;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to quick create supplier");
                await _dialogService.ShowAlertAsync("Error", "Failed to create supplier.");
            }
        }

        /// <summary>
        /// Generates and opens a PDF preview of the current order.
        /// </summary>
        [RelayCommand]
        public async Task PreviewOrder()
        {
             try
             {
                 if (SelectedSupplier == null && IsPurchaseOrder)
                 {
                      await _dialogService.ShowAlertAsync("Validation", "Please select a supplier first.");
                      return;
                 }
                 var orderToPrint = GetSanitizedOrder();
                 var path = await _pdfService.GenerateOrderPdfAsync(orderToPrint);
                 new System.Diagnostics.Process { StartInfo = new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true } }.Start();
             }
             catch(Exception ex)
             {
                 _logger.LogError(ex, "Failed to preview order");
                 await _dialogService.ShowAlertAsync("Error", "Failed to generate print preview.");
             }
        }

        /// <summary>
        /// Command to generate a grayscale print version of the order.
        /// </summary>
        [RelayCommand]
        public async Task DownloadPrint()
        {
             try
             {
                 if (SelectedSupplier == null && IsPurchaseOrder)
                 {
                      await _dialogService.ShowAlertAsync("Validation", "Please select a supplier first.");
                      return;
                 }
                 var orderToPrint = GetSanitizedOrder();
                 var path = await _pdfService.GenerateOrderPdfAsync(orderToPrint, isPrintVersion: true);
                 new System.Diagnostics.Process { StartInfo = new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true } }.Start();
             }
             catch(Exception ex)
             {
                 _logger.LogError(ex, "Failed to generate print version");
                  await _dialogService.ShowAlertAsync("Error", "Failed to generate grayscale version.");
             }
        }
        
        /// <summary>
        /// Command to generate a color version of the order for emailing.
        /// </summary>
        [RelayCommand]
        public async Task EmailOrder()
        {
             try
             {
                 if (SelectedSupplier == null && IsPurchaseOrder)
                 {
                      await _dialogService.ShowAlertAsync("Validation", "Please select a supplier first.");
                      return;
                 }
                 var orderToPrint = GetSanitizedOrder();
                 var path = await _pdfService.GenerateOrderPdfAsync(orderToPrint, isPrintVersion: false);
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
            // Create a shallow copy to avoid modifying the UI bound object
            var cleanOrder = new Order
            {
                Id = CurrentOrder.Id,
                OrderNumber = CurrentOrder.OrderNumber,
                OrderType = CurrentOrder.OrderType,
                Status = CurrentOrder.Status,
                OrderDate = CurrentOrder.OrderDate,
                ExpectedDeliveryDate = CurrentOrder.ExpectedDeliveryDate,
                SupplierId = CurrentOrder.SupplierId,
                SupplierName = CurrentOrder.SupplierName,
                CustomerId = CurrentOrder.CustomerId,
                // CustomerName = NewOrder.CustomerName, // Not on model
                ProjectId = CurrentOrder.ProjectId,
                ProjectName = CurrentOrder.ProjectName,
                EntityAddress = CurrentOrder.EntityAddress,
                EntityTel = CurrentOrder.EntityTel,
                EntityVatNo = CurrentOrder.EntityVatNo,
                Attention = CurrentOrder.Attention,
                DestinationType = CurrentOrder.DestinationType,
                DeliveryInstructions = CurrentOrder.DeliveryInstructions,
                ScopeOfWork = CurrentOrder.ScopeOfWork,
                Branch = CurrentOrder.Branch,
                TaxRate = CurrentOrder.TaxRate,
                Lines = new ObservableCollection<OrderLine>()
            };

            // Filter lines that have content
            var validLines = CurrentOrder.Lines
                .Where(l => !string.IsNullOrWhiteSpace(l.ItemCode) || !string.IsNullOrWhiteSpace(l.Description) || l.QuantityOrdered > 0 || l.UnitPrice > 0)
                .Where(l => l.QuantityOrdered > 0) // Typically we only want lines with qty
                .ToList();

            foreach(var lineWrapper in validLines)
            {
                lineWrapper.CommitToModel();
                cleanOrder.Lines.Add(lineWrapper.Model);
            }

            return cleanOrder;
        }

        /// <summary>
        /// Finalizes the order entry and persists it to the database.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSubmitOrder))]
        public async Task SubmitOrder()
        {
            if (IsBusy) return;

            // 1. Validate Basic Info
            if (CurrentOrder.OrderType == OrderType.PurchaseOrder && SelectedSupplier == null)
            {
                await _dialogService.ShowAlertAsync("Validation Error", "Please select a supplier.");
                return;
            }

            // Date Validation
            if (CurrentOrder.ExpectedDeliveryDate == null)
            {
                 await _dialogService.ShowAlertAsync("Validation Error", "Please select an Expected Delivery Date.");
                 return;
            }

            if (CurrentOrder.ExpectedDeliveryDate.Value.Date < DateTime.Today)
            {
                await _dialogService.ShowAlertAsync("Validation Error", "Expected delivery date cannot be in the past.");
                return;
            }

            if (CurrentOrder.OrderType == OrderType.SalesOrder && SelectedCustomer == null)
            {
                await _dialogService.ShowAlertAsync("Validation Error", "Please select a customer.");
                return;
            }
            if (IsSiteDelivery && SelectedProject == null)
            {
                 await _dialogService.ShowAlertAsync("Validation Error", "Please select a project for site delivery.");
                 return;
            }



            // 2. Validate Lines
            // Filter out empty placeholder lines before validation
            var meaningfulLines = CurrentOrder.Lines.Where(l => 
                l.InventoryItemId != null || 
                !string.IsNullOrWhiteSpace(l.ItemCode) || 
                !string.IsNullOrWhiteSpace(l.Description) || 
                l.QuantityOrdered > 0 || 
                l.UnitPrice > 0)
                .ToList();

            if (!meaningfulLines.Any())
            {
                await _dialogService.ShowAlertAsync("Validation Error", "Please add at least one line item.");
                return;
            }

            // Strict Field Validation for each meaningful line
            var invalidLines = meaningfulLines.Where(l => 
                l.InventoryItemId == null || 
                string.IsNullOrWhiteSpace(l.Description) || 
                l.QuantityOrdered <= 0 || 
                l.UnitPrice <= 0 || 
                string.IsNullOrWhiteSpace(l.UnitOfMeasure))
                .ToList();

            if (invalidLines.Any())
            {
                var firstInvalid = invalidLines.First();
                string missing = "";
                if (firstInvalid.InventoryItemId == null) missing = "Inventory Item";
                else if (string.IsNullOrWhiteSpace(firstInvalid.Description)) missing = "Description";
                else if (firstInvalid.QuantityOrdered <= 0) missing = "Quantity";
                else if (firstInvalid.UnitPrice <= 0) missing = "Unit Price";
                else if (string.IsNullOrWhiteSpace(firstInvalid.UnitOfMeasure)) missing = "Unit of Measure";

                await _dialogService.ShowAlertAsync("Validation Error", 
                    $"Some items are incomplete. Every line must have an Inventory Item, Description, Quantity, UOM and Unit Price.\n\nFirst issue found: Missing {missing}.");
                return;
            }

            try
            {
                IsBusy = true;
                BusyText = "Submitting Order...";

                // Commit Wrapper to Model
                CurrentOrder.CommitToModel();
                var orderToSubmit = CurrentOrder.Model;

                // Map Selections (redundant if bound directly, but keeping for safety if UI doesn't bind these properties to wrapper directly yet)
                if (SelectedSupplier != null) orderToSubmit.SupplierId = SelectedSupplier.Id;
                if (SelectedCustomer != null) orderToSubmit.CustomerId = SelectedCustomer.Id;
                if (SelectedProject != null) orderToSubmit.ProjectId = SelectedProject.Id;
                
                orderToSubmit.DestinationType = IsSiteDelivery ? OrderDestinationType.Site : OrderDestinationType.Stock;
                
                Order createdOrder;
                if (orderToSubmit.Id == Guid.Empty)
                {
                    createdOrder = await _orderManager.CreateOrderAsync(orderToSubmit);
                }
                else
                {
                    await _orderManager.UpdateOrderAsync(orderToSubmit);
                    createdOrder = orderToSubmit;
                }
                
                OrderCreated?.Invoke(this, EventArgs.Empty);

                if (ShouldEmailOrder)
                {
                    // Feature not yet implemented in Manager
                    // try { await _orderManager.EmailOrderAsync(createdOrder.Id); }
                    // catch(Exception ex) { _logger.LogError(ex, "Failed to email order"); }
                    await _dialogService.ShowAlertAsync("Info", "Email functionality is coming soon.");
                }

                if (ShouldPrintOrder)
                {
                    try
                    {
                        var path = await _pdfService.GenerateOrderPdfAsync(createdOrder, isPrintVersion: true);
                        new System.Diagnostics.Process { StartInfo = new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true } }.Start();
                    }
                    catch(Exception ex) { _logger.LogError(ex, "Failed to print order"); }
                }

                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting order");
                await _dialogService.ShowAlertAsync("Error", $"Failed to submit order: {ex.Message}");
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// Cancels the current operation and closes the view.
        /// </summary>
        [RelayCommand]
        public void Cancel() => CloseRequested?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Validates if the search text matches an existing item, and offers to create it if not.
        /// </summary>
        [RelayCommand]
        public async Task ValidateItemSearch(string searchText)
        {
             if (string.IsNullOrWhiteSpace(searchText)) return;
             
             var exists = InventoryItems.Any(i => i.Sku.Equals(searchText, StringComparison.OrdinalIgnoreCase) || i.Description.Equals(searchText, StringComparison.OrdinalIgnoreCase));
                  
             if (!exists && SelectedInventoryItem == null)
             {
                 bool create = await _dialogService.ShowConfirmationAsync("Item Not Found", $"'{searchText}' not found.\n\nCreate it now?");
                 if (create)
                 {
                     _orderStateService.SaveState(CurrentOrder.Model, NewLine.Model, searchText);
                     WeakReferenceMessenger.Default.Send(new OCC.Client.Messages.NavigationRequestMessage("InventoryDetail_Create", searchText));
                 }
             }
        }

        /// <summary>
        /// Toggles the add project address overlay.
        /// </summary>
        [RelayCommand]
        public void ToggleAddProjectAddress() => IsAddingProjectAddress = !IsAddingProjectAddress;

        /// <summary>
        /// Saves a new delivery address to the selected project.
        /// </summary>
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

        /// <summary>
        /// Initializes the view model by triggering validation.
        /// </summary>
        private void Initialize()
        {
            // Subscribe to validation changes
            ErrorsChanged -= OnErrorsChanged;
            ErrorsChanged += OnErrorsChanged;
            
            CurrentOrder.ErrorsChanged -= OnCurrentOrderErrorsChanged;
            CurrentOrder.ErrorsChanged += OnCurrentOrderErrorsChanged;
            
            // Force validation on all properties logic
            ValidateAllProperties();
            CurrentOrder.Validate();
            
            // Explicitly force Supplier validation if PO (sometimes Context validation is tricky on nulls if not Required)
            if (IsPurchaseOrder) ValidateProperty(SelectedSupplier, nameof(SelectedSupplier));
            
            // Notify command
            SubmitOrderCommand.NotifyCanExecuteChanged();
        }

        private void OnErrorsChanged(object? sender, System.ComponentModel.DataErrorsChangedEventArgs e)
        {
            SubmitOrderCommand.NotifyCanExecuteChanged();
        }

        private void OnCurrentOrderErrorsChanged(object? sender, System.ComponentModel.DataErrorsChangedEventArgs e)
        {
            SubmitOrderCommand.NotifyCanExecuteChanged();
        }

        public bool CanSubmitOrder => !HasErrors && !CurrentOrder.HasErrors;

        /// <summary>
        /// Asynchronously loads necessary lookup data from the Order Manager.
        /// </summary>
        public async Task LoadData()
        {
            try
            {
                BusyText = "Loading order entry data...";
                IsBusy = true;
                var data = await _orderManager.GetOrderEntryDataAsync();

                _allSuppliers.Clear();
                _allSuppliers.AddRange(data.Suppliers);
                FilterSuppliers(); // Apply filter based on current branch

                Customers.Clear();
                foreach(var i in data.Customers) Customers.Add(i);

                Projects.Clear();
                foreach(var i in data.Projects) Projects.Add(i);

                InventoryItems.Clear();
                FilteredInventoryItemsBySku.Clear();
                FilteredInventoryItemsByName.Clear();
                foreach(var i in data.Inventory) 
                {
                    InventoryItems.Add(i);
                    FilteredInventoryItemsBySku.Add(i);
                    FilteredInventoryItemsByName.Add(i);
                }

                var cats = data.Inventory.Select(x => x.Category).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c);
                ProductCategories.Clear();
                foreach(var c in cats) ProductCategories.Add(c);
                if (!ProductCategories.Contains("General")) ProductCategories.Add("General");

                if (_orderStateService.HasSavedState) 
                {
                    // Debug Toast
                    // Toast removed
                    await RestoreState();
                }
                else
                {
                     // Debug Toast
                     // Toast removed
                }
            }
            catch(Exception ex)
            {
                  _logger.LogError(ex, "Failed to load order entry data");
                  await _dialogService.ShowAlertAsync("Error", "Check connection and try again.");
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// Resets the ViewModel to a fresh state for a new order.
        /// </summary>
        public void Reset()
        {
            CurrentOrder = new OrderWrapper(_orderManager.CreateNewOrderTemplate());
            CurrentOrder.ExpectedDeliveryDate = DateTime.Today;

            // Default Branch to User's Branch if available (e.g. CPT)
            if (_authService?.CurrentUser?.Branch != null)
            {
                CurrentOrder.Branch = _authService.CurrentUser.Branch.Value;
            }

            IsOfficeDelivery = true;
            CurrentOrder.Attention = string.Empty;
            UpdateOrderTypeFlags();

            // Start with EXACTLY one row (TestView style)
            CurrentOrder.Lines.Clear();
            CurrentOrder.Lines.Add(new OrderLineWrapper(new OrderLine { UnitOfMeasure = "ea" }));
            
            SetupLineListeners();
            OrderMenu.ActiveTab = "New Order";
            Initialize();
        }

        /// <summary>
        /// Loads an existing order into the ViewModel for viewing or editing.
        /// </summary>
        public async Task LoadExistingOrder(Order order)
        {
            try
            {
                IsBusy = true;
                
                // Fetch full order to ensure all lines are included (List view might only provide summary)
                var fullOrder = await _orderManager.GetOrderByIdAsync(order.Id) ?? order;

                CurrentOrder = new OrderWrapper(fullOrder);
                
                if (CurrentOrder.SupplierId.HasValue)
                    SelectedSupplier = Suppliers.FirstOrDefault(s => s.Id == CurrentOrder.SupplierId.Value);

                if (CurrentOrder.CustomerId.HasValue)
                    SelectedCustomer = Customers.FirstOrDefault(c => c.Id == CurrentOrder.CustomerId.Value);

                if (CurrentOrder.ProjectId.HasValue)
                    SelectedProject = Projects.FirstOrDefault(p => p.Id == CurrentOrder.ProjectId.Value);

                IsSiteDelivery = CurrentOrder.DestinationType == OrderDestinationType.Site;
                IsReadOnly = CurrentOrder.Status == OrderStatus.Completed || CurrentOrder.Status == OrderStatus.Cancelled;
            
                SetupLineListeners();
            
                UpdateOrderTypeFlags();
            
                if (CurrentOrder.DestinationType == OrderDestinationType.Site) IsSiteDelivery = true;
                else IsOfficeDelivery = true;

                // CHECK: Allow editing as long as the order is NOT Completed or Cancelled.
                // User confirmed: "If the order says completed then we lock. Orders partial delivers we leave it unlocked."
                IsReadOnly = CurrentOrder.Status == OrderStatus.Completed || CurrentOrder.Status == OrderStatus.Cancelled;
            
                // If it is editable, ensure at least one row exists or add one at the end if needed
                if (!IsReadOnly && CurrentOrder.Lines != null)
                {
                    // Always add an empty row for editing convenience
                    CurrentOrder.Lines.Add(new OrderLineWrapper(new OrderLine { UnitOfMeasure = "ea" }));
                    SetupLineListeners(); // Hook up all lines including the new one
                }

                OrderMenu.ActiveTab = "New Order";
                Initialize();

                OnPropertyChanged(nameof(CurrentOrder));
                OnPropertyChanged(nameof(SubmitButtonText));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to load order details");
                await _dialogService.ShowAlertAsync("Error", "Failed to load order details.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Restores previous state when returning from a nested view.
        /// </summary>
        private async Task RestoreState()
        {
            var (savedOrder, pendingLine, searchTerm) = _orderStateService.RetrieveState();
            if (savedOrder != null)
            {
                 // Toast removed
                await LoadExistingOrder(savedOrder);
                if (pendingLine != null) NewLine = new OrderLineWrapper(pendingLine);
                if (!string.IsNullOrEmpty(searchTerm))
                {
                   // Try finding by exact match first (case-insensitive)
                   var match = InventoryItems.FirstOrDefault(i => i.Sku.Equals(searchTerm, StringComparison.OrdinalIgnoreCase) || i.Description.Equals(searchTerm, StringComparison.OrdinalIgnoreCase));
                   
                   // If not found, it might be the NEW item we just created.
                   // Since we just reloaded InventoryItems in LoadData, it SHOULD be there if it was saved.
                   
                   if (match != null) 
                   {
                       SelectedInventoryItem = match;
                       // User requested auto-add
                       if (SelectedInventoryItem != null)
                       {
                           await AddLine();
                       }
                   }
                }
            }
            _orderStateService.ClearState();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Coordinates order type flags for UI visibility logic.
        /// </summary>
        private void UpdateOrderTypeFlags()
        {
            IsPurchaseOrder = CurrentOrder.OrderType == OrderType.PurchaseOrder;
            IsSalesOrder = CurrentOrder.OrderType == OrderType.SalesOrder;
            IsReturnOrder = CurrentOrder.OrderType == OrderType.ReturnToInventory;
        }

        /// <summary>
        /// Event raised after an order has been successfully created.
        /// </summary>
        public event EventHandler? OrderCreated;

        /// <summary>
        /// Event raised when the view requests to close itself.
        /// </summary>
        public event EventHandler? CloseRequested;

        /// <summary>
        /// Static validator for supplier selection.
        /// </summary>
        public static ValidationResult? ValidateSupplierSelection(Supplier? supplier, ValidationContext context)
        {
            var vm = (CreateOrderViewModel)context.ObjectInstance;
            return (vm.IsPurchaseOrder && supplier == null) ? new ValidationResult("Supplier is required.") : ValidationResult.Success;
        }

        /// <summary>
        /// Static validator for project selection.
        /// </summary>
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
            FilterSuppliers();
        }

        private void OnOrderPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OrderWrapper.Branch))
            {
                FilterSuppliers();
            }
        }

        private void FilterSuppliers()
        {
            if (CurrentOrder == null) return;
            
            var targetBranch = CurrentOrder.Branch;
            
            var filtered = _allSuppliers.Where(s => s.Branch == null || s.Branch == targetBranch).ToList();
            
            // Retain selection if valid, else clear
            var currentSelection = SelectedSupplier;
            
            Suppliers.Clear();
            foreach(var s in filtered.OrderBy(x => x.Name)) Suppliers.Add(s);

            if (currentSelection != null && Suppliers.Contains(currentSelection))
            {
                SelectedSupplier = currentSelection;
            }
            else
            {
                // If the previously selected supplier is no longer valid for this branch, should we clear it?
                // Probably yes, to avoid invalid data.
                if (currentSelection != null && currentSelection.Branch != null && currentSelection.Branch != targetBranch)
                {
                    SelectedSupplier = null;
                }
                // If it was a global supplier (Branch == null), it should still be in the list, so we keep it.
                // If it's not in the list for some reason, SelectedSupplier becomes null automatically if bound to list? 
                // No, ObservableProperty holds the value.
            }
        }

        partial void OnIsOfficeDeliveryChanged(bool value) { if (value) { CurrentOrder.DestinationType = OrderDestinationType.Stock; IsSiteDelivery = false; } }
        partial void OnIsSiteDeliveryChanged(bool value) { if (value) { CurrentOrder.DestinationType = OrderDestinationType.Site; IsOfficeDelivery = false; if (SelectedProject != null) CurrentOrder.EntityAddress = SelectedProject.FullAddress; } }
        partial void OnSelectedProjectChanged(Project? value)
        {
             if (value != null)
             {
                 CurrentOrder.ProjectId = value.Id;
                 CurrentOrder.ProjectName = value.Name;
                 if (IsSalesOrder) { CurrentOrder.CustomerId = value.Id; CurrentOrder.EntityAddress = value.FullAddress; }
                 else if (IsPurchaseOrder && IsSiteDelivery) CurrentOrder.EntityAddress = value.FullAddress;
                 OnPropertyChanged(nameof(CurrentOrder));
             }
        }
        /// <summary>
        /// Attempts to automatically select the best match from the filtered list 
        /// if the user Tabs out or leaves the field without explicit selection.
        /// </summary>
        public void TryCommitAutoSelection(OrderLineWrapper line)
        {
            // If we already have a selection, or no search text, or no line, ignore.
            if (SelectedInventoryItem != null || line == null || string.IsNullOrWhiteSpace(ProductSearchText)) return;

            // Pick the best match (first item because we sorted by relevance in FilterInventory)
            var bestMatch = FilteredInventoryItemsByName.FirstOrDefault();
            if (bestMatch != null)
            {
                // Apply it
                SelectedInventoryItem = bestMatch;
                UpdateLineFromSelection(line, bestMatch);
                
                // Reset search for next interaction
                ProductSearchText = string.Empty;
                IsProductDropDownOpen = false;
            }
        }

        private void FilterInventory()
        {
             if (string.IsNullOrWhiteSpace(ProductSearchText))
             {
                 FilteredInventoryItemsByName.Clear();
                 foreach (var item in InventoryItems) FilteredInventoryItemsByName.Add(item);
                 return;
             }

             var search = ProductSearchText.Trim();
             
             // Weighted Search: Exact Match > StartsWith > Contains
             var filtered = InventoryItems
                 .Where(i => (i.Description != null && i.Description.Contains(search, StringComparison.OrdinalIgnoreCase)) || 
                             (i.Sku != null && i.Sku.Contains(search, StringComparison.OrdinalIgnoreCase)))
                 .GroupBy(i => (i.Sku ?? "").ToLower()).Select(g => g.First()) // Deduplicate by SKU (defensive against data dupes)
                 .OrderByDescending(i => i.Sku != null && i.Sku.Equals(search, StringComparison.OrdinalIgnoreCase)) // 1. Exact SKU
                 .ThenByDescending(i => i.Description != null && i.Description.Equals(search, StringComparison.OrdinalIgnoreCase)) // 2. Exact Name
                 .ThenByDescending(i => i.Sku != null && i.Sku.StartsWith(search, StringComparison.OrdinalIgnoreCase)) // 3. StartsWith SKU (New)
                 .ThenByDescending(i => i.Description != null && i.Description.StartsWith(search, StringComparison.OrdinalIgnoreCase)) // 4. StartsWith Name
                 .ThenBy(i => i.Description) // 5. Alphabetical
                 .ToList();

             FilteredInventoryItemsByName.Clear();
             foreach (var item in filtered) FilteredInventoryItemsByName.Add(item);

             // Auto-open if we have results and user is typing
             IsProductDropDownOpen = FilteredInventoryItemsByName.Any();
        }

        partial void OnSkuSearchTextChanged(string value)
        {
            if (_isUpdatingSearchText) return;
            Dispatcher.UIThread.Post(() => 
            {
                FilteredInventoryItemsBySku.Clear();
                var matches = string.IsNullOrWhiteSpace(value) ? InventoryItems : InventoryItems.Where(i => i.Sku != null && i.Sku.Contains(value, StringComparison.OrdinalIgnoreCase));
                foreach(var m in matches) FilteredInventoryItemsBySku.Add(m);
                IsSkuDropDownOpen = true;
            });
        }
        partial void OnProductSearchTextChanged(string value)
        {
            if (_isUpdatingSearchText) return;
            Dispatcher.UIThread.Post(() => 
            {
                FilterInventory();
            });
        }
        partial void OnSelectedInventoryItemChanged(InventoryItem? value)
        {
            if (value != null)
            {
                _isUpdatingSearchText = true;
                SkuSearchText = value.Sku ?? "";
                ProductSearchText = value.Description ?? "";
                IsSkuDropDownOpen = false;
                IsProductDropDownOpen = false;
                _isUpdatingSearchText = false;
                NewLine = new OrderLineWrapper(new OrderLine { InventoryItemId = value.Id, Description = value.Description ?? "", ItemCode = value.Sku ?? "", UnitOfMeasure = value.UnitOfMeasure ?? "", UnitPrice = value.AverageCost });
                OnPropertyChanged(nameof(NewLine));
                if (SelectedSupplier == null && !string.IsNullOrWhiteSpace(value.Supplier))
                {
                    var match = Suppliers.FirstOrDefault(s => s.Name.Equals(value.Supplier, StringComparison.OrdinalIgnoreCase));
                    if (match != null) SelectedSupplier = match;
                }
            }
        }
        partial void OnSelectedSupplierChanged(Supplier? value)
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
                
                // Force validation update
                ValidateProperty(SelectedSupplier, nameof(SelectedSupplier));
                SubmitOrderCommand.NotifyCanExecuteChanged();
            }
        }
        partial void OnSelectedCustomerChanged(Customer? value)
        {
            if (value != null)
            {
                CurrentOrder.CustomerId = value.Id;
                CurrentOrder.EntityAddress = value.Address;
                CurrentOrder.EntityTel = value.Phone;
                OnPropertyChanged(nameof(CurrentOrder));
            }
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
                    // We are already here, maybe reset?
                    Reset();
                    IsReadOnly = false;
                    break;
            }
        }



        #endregion
    }
}
