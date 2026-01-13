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
using CommunityToolkit.Mvvm.Messaging; // NEW
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
        private readonly OrderStateService _orderStateService;
        private readonly ILogger<CreateOrderViewModel> _logger;
        private readonly IPdfService _pdfService;
        private bool _isUpdatingSearchText;

        #endregion

        #region Observables

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _busyText = "Please wait...";

        [ObservableProperty]
        private Order _newOrder = new();

        [ObservableProperty]
        private bool _isReadOnly;

        /// <summary>
        /// Gets the current order subtotal from the model.
        /// </summary>
        public decimal OrderSubTotal => NewOrder?.SubTotal ?? 0;
        
        /// <summary>
        /// Gets the current order VAT total from the model.
        /// </summary>
        public decimal OrderVat => NewOrder?.VatTotal ?? 0;
        
        /// <summary>
        /// Gets the current order total amount from the model.
        /// </summary>
        public decimal OrderTotal => NewOrder?.TotalAmount ?? 0;

        [ObservableProperty]
        private OrderLine _newLine = new();
        
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
        private string _newProductName = string.Empty;

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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateOrderViewModel"/> class with required dependencies.
        /// </summary>
        public CreateOrderViewModel(
            IOrderManager orderManager,
            IDialogService dialogService,
            ILogger<CreateOrderViewModel> logger,
            IPdfService pdfService,
            OrderStateService orderStateService)
        {
            _orderManager = orderManager;
            _dialogService = dialogService;
            _logger = logger;
            _pdfService = pdfService;
            _orderStateService = orderStateService;
            
            Reset();
        }

        /// <summary>
        /// Design-time constructor.
        /// </summary>
        public CreateOrderViewModel() 
        {
            _orderManager = null!;
            _dialogService = null!;
            _logger = null!;
            _pdfService = null!;
            _orderStateService = null!;
            NewOrder.OrderNumber = "DEMO-0000";
        }

        private void RecalculateTotals()
        {
            // Model properties are computed (read-only), so we just notify the view to re-read them.
            OnPropertyChanged(nameof(OrderSubTotal));
            OnPropertyChanged(nameof(OrderVat));
            OnPropertyChanged(nameof(OrderTotal));
        }

        private void OnLineChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OrderLine.LineTotal) || e.PropertyName == nameof(OrderLine.VatAmount))
            {
                RecalculateTotals();
            }
        }

        private void SetupLineListeners()
        {
             // Remove old if any (naive approach, assume fresh start or handle in logic)
             NewOrder.Lines.CollectionChanged -= Lines_CollectionChanged;
             NewOrder.Lines.CollectionChanged += Lines_CollectionChanged;

             foreach(var line in NewOrder.Lines)
             {
                 line.PropertyChanged -= OnLineChanged;
                 line.PropertyChanged += OnLineChanged;
             }
        }

        private void Lines_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach(OrderLine item in e.NewItems) item.PropertyChanged += OnLineChanged;

            if (e.OldItems != null)
                foreach(OrderLine item in e.OldItems) item.PropertyChanged -= OnLineChanged;

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
             NewOrder.OrderType = type;
             var temp = _orderManager.CreateNewOrderTemplate(type);
             NewOrder.OrderType = temp.OrderType;
             NewOrder.OrderNumber = temp.OrderNumber;
             UpdateOrderTypeFlags();
             OnPropertyChanged(nameof(NewOrder));
        }

        /// <summary>
        /// Adds a new line item to the order based on the current selection.
        /// </summary>
        [RelayCommand]
        public async Task AddLine()
        {
            if (string.IsNullOrWhiteSpace(NewLine.Description) && SelectedInventoryItem == null) return;

            NewLine.CalculateTotal(NewOrder.TaxRate);
            
            var line = new OrderLine
            {
                InventoryItemId = NewLine.InventoryItemId,
                ItemCode = NewLine.ItemCode,
                Description = NewLine.Description,
                QuantityOrdered = NewLine.QuantityOrdered,
                QuantityReceived = NewLine.QuantityReceived,
                UnitOfMeasure = NewLine.UnitOfMeasure,
                UnitPrice = NewLine.UnitPrice,
                VatAmount = NewLine.VatAmount,
                LineTotal = NewLine.LineTotal
            };
            
            if (line.LineTotal == 0 && line.QuantityOrdered > 0 && line.UnitPrice > 0)
            {
                 line.CalculateTotal(NewOrder.TaxRate);
            }

            if (SelectedInventoryItem != null && SelectedInventoryItem.AverageCost > 0)
            {
                decimal threshold = SelectedInventoryItem.AverageCost * 1.10m;
                if (line.UnitPrice > threshold)
                {
                    decimal pctIncrease = ((line.UnitPrice - SelectedInventoryItem.AverageCost) / SelectedInventoryItem.AverageCost) * 100m;
                    bool confirm = await _dialogService.ShowConfirmationAsync(
                        "Price Increase Warning", 
                        $"The price of R{line.UnitPrice:F2} is {pctIncrease:F0}% higher than average.\n\nAccept and update average cost?");
                    
                    if (confirm)
                    {
                        SelectedInventoryItem.AverageCost = line.UnitPrice;
                        try { await _orderManager.UpdateInventoryItemAsync(SelectedInventoryItem); }
                        catch(Exception ex) { _logger.LogError(ex, "Failed to update avg cost"); }
                    }
                }
            }

            NewOrder.Lines.Add(line);
            OnPropertyChanged(nameof(NewOrder));
            OnPropertyChanged(nameof(OrderSubTotal));
            OnPropertyChanged(nameof(OrderVat));
            OnPropertyChanged(nameof(OrderTotal));

            NewLine = new OrderLine();
            SelectedInventoryItem = null;
        }
        
        /// <summary>
        /// Removes a line item from the order.
        /// </summary>
        [RelayCommand]
        public void RemoveLine(OrderLine line)
        {
            if (line == null) return;
            NewOrder.Lines.Remove(line);
            OnPropertyChanged(nameof(NewOrder));
            OnPropertyChanged(nameof(OrderSubTotal));
            OnPropertyChanged(nameof(OrderVat));
            OnPropertyChanged(nameof(OrderTotal));
        }

        /// <summary>
        /// Adds an empty line to the grid for editing.
        /// </summary>
        [RelayCommand]
        public void AddEmptyLine()
        {
            NewOrder.Lines.Add(new OrderLine { QuantityOrdered = 1, UnitOfMeasure = "ea" });
        }

        /// <summary>
        /// Updates a specific order line with details from a selected inventory item.
        /// </summary>
        public void UpdateLineFromSelection(OrderLine line, InventoryItem item)
        {
            if (line == null || item == null) return;

            // Update model
            line.InventoryItemId = item.Id;
            line.ItemCode = item.Sku;
            line.Description = item.ProductName;
            line.UnitOfMeasure = string.IsNullOrEmpty(item.UnitOfMeasure) ? "ea" : item.UnitOfMeasure;
            line.UnitPrice = item.AverageCost; 
            
            // Auto-set quantity to 1 if empty/zero to populate the row
            if (line.QuantityOrdered <= 0) line.QuantityOrdered = 1;

            line.CalculateTotal(NewOrder.TaxRate);

            // Collection refresh not needed as OrderLine now implements INotifyPropertyChanged
            
            OnPropertyChanged(nameof(OrderSubTotal));
            OnPropertyChanged(nameof(OrderVat));
            OnPropertyChanged(nameof(OrderTotal));
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
                NewProductName = "";
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
            if (string.IsNullOrWhiteSpace(NewProductName)) return;
            
            try
            {
                var created = await _orderManager.QuickCreateProductAsync(NewProductName, NewProductUOM, NewProductCategory, SelectedSupplier?.Name ?? string.Empty);
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
                Id = NewOrder.Id,
                OrderNumber = NewOrder.OrderNumber,
                OrderType = NewOrder.OrderType,
                Status = NewOrder.Status,
                OrderDate = NewOrder.OrderDate,
                ExpectedDeliveryDate = NewOrder.ExpectedDeliveryDate,
                SupplierId = NewOrder.SupplierId,
                SupplierName = NewOrder.SupplierName,
                CustomerId = NewOrder.CustomerId,
                // CustomerName = NewOrder.CustomerName, // Not on model
                ProjectId = NewOrder.ProjectId,
                ProjectName = NewOrder.ProjectName,
                EntityAddress = NewOrder.EntityAddress,
                EntityTel = NewOrder.EntityTel,
                EntityVatNo = NewOrder.EntityVatNo,
                Attention = NewOrder.Attention,
                DestinationType = NewOrder.DestinationType,
                TaxRate = NewOrder.TaxRate,
                Lines = new ObservableCollection<OrderLine>()
            };

            // Filter lines that have content
            var validLines = NewOrder.Lines
                .Where(l => !string.IsNullOrWhiteSpace(l.ItemCode) || !string.IsNullOrWhiteSpace(l.Description) || l.QuantityOrdered > 0 || l.UnitPrice > 0)
                .Where(l => l.QuantityOrdered > 0) // Typically we only want lines with qty
                .ToList();

            foreach(var line in validLines)
            {
                cleanOrder.Lines.Add(line);
            }

            return cleanOrder;
        }

        /// <summary>
        /// Finalizes the order entry and persists it to the database.
        /// </summary>
        [RelayCommand]
        public async Task SubmitOrder()
        {
            if (IsBusy) return;

            // 1. Validate Basic Info
            if (NewOrder.OrderType == OrderType.PurchaseOrder && SelectedSupplier == null)
            {
                await _dialogService.ShowAlertAsync("Validation Error", "Please select a supplier.");
                return;
            }

            // Date Validation
            if (NewOrder.ExpectedDeliveryDate == null)
            {
                 await _dialogService.ShowAlertAsync("Validation Error", "Please select an Expected Delivery Date.");
                 return;
            }

            if (NewOrder.ExpectedDeliveryDate.Value.Date < DateTime.Today)
            {
                await _dialogService.ShowAlertAsync("Validation Error", "Expected delivery date cannot be in the past.");
                return;
            }

            if (NewOrder.OrderType == OrderType.SalesOrder && SelectedCustomer == null)
            {
                await _dialogService.ShowAlertAsync("Validation Error", "Please select a customer.");
                return;
            }
            if (IsSiteDelivery && SelectedProject == null)
            {
                 await _dialogService.ShowAlertAsync("Validation Error", "Please select a project for site delivery.");
                 return;
            }



            // 2. Filter Empty Rows
            var validLines = NewOrder.Lines
                .Where(l => !string.IsNullOrWhiteSpace(l.ItemCode) || !string.IsNullOrWhiteSpace(l.Description))
                .ToList();

            if (!validLines.Any())
            {
                await _dialogService.ShowAlertAsync("Validation Error", "Please add at least one line item.");
                return;
            }

            // Temporarily swap lines for saving
            var originalLines = NewOrder.Lines.ToList();
            NewOrder.Lines.Clear();
            foreach(var line in validLines) NewOrder.Lines.Add(line);

            try
            {
                IsBusy = true;
                BusyText = "Submitting Order...";

                // Map Selections
                if (SelectedSupplier != null) NewOrder.SupplierId = SelectedSupplier.Id;
                if (SelectedCustomer != null) NewOrder.CustomerId = SelectedCustomer.Id;
                if (SelectedProject != null) NewOrder.ProjectId = SelectedProject.Id;
                
                NewOrder.DestinationType = IsSiteDelivery ? OrderDestinationType.Site : OrderDestinationType.Stock;
                
                Order createdOrder;
                if (NewOrder.Id == Guid.Empty)
                {
                    createdOrder = await _orderManager.CreateOrderAsync(NewOrder);
                }
                else
                {
                    await _orderManager.UpdateOrderAsync(NewOrder);
                    createdOrder = NewOrder;
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
                
                // Restore lines if failed, so user doesn't lose empty rows if they want to keep editing
                NewOrder.Lines.Clear();
                foreach(var line in originalLines) NewOrder.Lines.Add(line);
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
             
             var exists = InventoryItems.Any(i => i.Sku.Equals(searchText, StringComparison.OrdinalIgnoreCase) || i.ProductName.Equals(searchText, StringComparison.OrdinalIgnoreCase));
                  
             if (!exists && SelectedInventoryItem == null)
             {
                 bool create = await _dialogService.ShowConfirmationAsync("Item Not Found", $"'{searchText}' not found.\n\nCreate it now?");
                 if (create)
                 {
                     _orderStateService.SaveState(NewOrder, NewLine, searchText);
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
                     project.Location = NewProjectAddress;
                     await _orderManager.UpdateProjectAsync(project);
                     SelectedProject.Location = NewProjectAddress;
                     NewOrder.EntityAddress = NewProjectAddress;
                     OnPropertyChanged(nameof(NewOrder));
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
        /// Asynchronously loads necessary lookup data from the Order Manager.
        /// </summary>
        public async Task LoadData()
        {
            try
            {
                BusyText = "Loading order entry data...";
                IsBusy = true;
                var data = await _orderManager.GetOrderEntryDataAsync();

                Suppliers.Clear();
                foreach(var i in data.Suppliers) Suppliers.Add(i);

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

                if (_orderStateService.IsReturningFromItemCreation) await RestoreState();
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
            NewOrder = _orderManager.CreateNewOrderTemplate();
            IsOfficeDelivery = true;
            NewOrder.Attention = string.Empty;
            UpdateOrderTypeFlags();

            // Pre-fill with empty rows for spreadsheet-style entry
            for (int i = 0; i < 20; i++)
            {
                NewOrder.Lines.Add(new OrderLine { UnitOfMeasure = "" }); 
            }
            SetupLineListeners();
        }

        /// <summary>
        /// Loads an existing order into the ViewModel for viewing or editing.
        /// </summary>
        public void LoadExistingOrder(Order order)
        {
            NewOrder = order;
            if (Suppliers.Any()) SelectedSupplier = Suppliers.FirstOrDefault(s => s.Id == order.SupplierId);
            if (Customers.Any()) SelectedCustomer = Customers.FirstOrDefault(c => c.Id == order.CustomerId);
            if (Projects.Any()) SelectedProject = Projects.FirstOrDefault(p => p.Id == order.ProjectId);
            UpdateOrderTypeFlags();
            if (order.DestinationType == OrderDestinationType.Site) IsSiteDelivery = true;
            else IsOfficeDelivery = true;
            OnPropertyChanged(nameof(NewOrder));
            OnPropertyChanged(nameof(OrderSubTotal));
            OnPropertyChanged(nameof(OrderVat));
            OnPropertyChanged(nameof(OrderVat));
            OnPropertyChanged(nameof(OrderTotal));
        }

        /// <summary>
        /// Restores previous state when returning from a nested view.
        /// </summary>
        private async Task RestoreState()
        {
            var (savedOrder, pendingLine, searchTerm) = _orderStateService.RetrieveState();
            if (savedOrder != null)
            {
                LoadExistingOrder(savedOrder);
                if (pendingLine != null) NewLine = pendingLine;
                if (!string.IsNullOrEmpty(searchTerm))
                {
                   // Try finding by exact match first (case-insensitive)
                   var match = InventoryItems.FirstOrDefault(i => i.Sku.Equals(searchTerm, StringComparison.OrdinalIgnoreCase) || i.ProductName.Equals(searchTerm, StringComparison.OrdinalIgnoreCase));
                   
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
            IsPurchaseOrder = NewOrder.OrderType == OrderType.PurchaseOrder;
            IsSalesOrder = NewOrder.OrderType == OrderType.SalesOrder;
            IsReturnOrder = NewOrder.OrderType == OrderType.ReturnToInventory;
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



        partial void OnNewOrderChanged(Order value) => UpdateOrderTypeFlags();
        partial void OnIsOfficeDeliveryChanged(bool value) { if (value) { NewOrder.DestinationType = OrderDestinationType.Stock; IsSiteDelivery = false; } }
        partial void OnIsSiteDeliveryChanged(bool value) { if (value) { NewOrder.DestinationType = OrderDestinationType.Site; IsOfficeDelivery = false; if (SelectedProject != null) NewOrder.EntityAddress = SelectedProject.Location; } }
        partial void OnSelectedProjectChanged(Project? value)
        {
             if (value != null)
             {
                 NewOrder.ProjectId = value.Id;
                 NewOrder.ProjectName = value.Name;
                 if (IsSalesOrder) { NewOrder.CustomerId = value.Id; NewOrder.EntityAddress = value.Location; }
                 else if (IsPurchaseOrder && IsSiteDelivery) NewOrder.EntityAddress = value.Location;
                 OnPropertyChanged(nameof(NewOrder));
             }
        }
        /// <summary>
        /// Attempts to automatically select the best match from the filtered list 
        /// if the user Tabs out or leaves the field without explicit selection.
        /// </summary>
        public void TryCommitAutoSelection(OrderLine line)
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
                 .Where(i => (i.ProductName != null && i.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase)) || 
                             (i.Sku != null && i.Sku.Contains(search, StringComparison.OrdinalIgnoreCase)))
                 .GroupBy(i => (i.Sku ?? "").ToLower()).Select(g => g.First()) // Deduplicate by SKU (defensive against data dupes)
                 .OrderByDescending(i => i.Sku != null && i.Sku.Equals(search, StringComparison.OrdinalIgnoreCase)) // 1. Exact SKU
                 .ThenByDescending(i => i.ProductName != null && i.ProductName.Equals(search, StringComparison.OrdinalIgnoreCase)) // 2. Exact Name
                 .ThenByDescending(i => i.Sku != null && i.Sku.StartsWith(search, StringComparison.OrdinalIgnoreCase)) // 3. StartsWith SKU (New)
                 .ThenByDescending(i => i.ProductName != null && i.ProductName.StartsWith(search, StringComparison.OrdinalIgnoreCase)) // 4. StartsWith Name
                 .ThenBy(i => i.ProductName) // 5. Alphabetical
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
        partial void OnProductSearchTextChanged(string? value)
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
                ProductSearchText = value.ProductName ?? "";
                IsSkuDropDownOpen = false;
                IsProductDropDownOpen = false;
                _isUpdatingSearchText = false;
                NewLine = new OrderLine { InventoryItemId = value.Id, Description = value.ProductName ?? "", ItemCode = value.Sku ?? "", UnitOfMeasure = value.UnitOfMeasure ?? "", UnitPrice = value.AverageCost };
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
                NewOrder.SupplierId = value.Id;
                NewOrder.SupplierName = value.Name;
                NewOrder.EntityAddress = value.Address;
                NewOrder.EntityTel = value.Phone;
                NewOrder.EntityVatNo = value.VatNumber;
                NewOrder.Attention = value.ContactPerson; 
                OnPropertyChanged(nameof(NewOrder));
            }
        }
        partial void OnSelectedCustomerChanged(Customer? value)
        {
            if (value != null)
            {
                NewOrder.CustomerId = value.Id;
                NewOrder.EntityAddress = value.Address;
                NewOrder.EntityTel = value.Phone;
                OnPropertyChanged(nameof(NewOrder));
            }
        }



        #endregion
    }
}
