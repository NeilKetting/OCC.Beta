using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Shared.Models;

namespace OCC.Client.Services.Managers
{
    /// <summary>
    /// Implementation of <see cref="IOrderManager"/> that coordinates order operations.
    /// This class acts as a facade, aggregating <see cref="IOrderService"/>, <see cref="IInventoryService"/>, 
    /// <see cref="ISupplierService"/>, and repositories to provide a clean API for ViewModels.
    /// </summary>
    public class OrderManager : IOrderManager
    {
        #region Private Members

        private readonly IOrderService _orderService;
        private readonly IInventoryService _inventoryService;
        private readonly ISupplierService _supplierService;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Project> _projectRepository;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderManager"/> class.
        /// </summary>
        /// <param name="orderService">Service for core order operations.</param>
        /// <param name="inventoryService">Service for inventory management.</param>
        /// <param name="supplierService">Service for supplier management.</param>
        /// <param name="customerRepository">Repository for customer data.</param>
        /// <param name="projectRepository">Repository for project data.</param>
        public OrderManager(
            IOrderService orderService,
            IInventoryService inventoryService,
            ISupplierService supplierService,
            IRepository<Customer> customerRepository,
            IRepository<Project> projectRepository)
        {
            _orderService = orderService;
            _inventoryService = inventoryService;
            _supplierService = supplierService;
            _customerRepository = customerRepository;
            _projectRepository = projectRepository;
        }

        #endregion

        #region Methods

        #region Order Operations

        /// <inheritdoc/>
        public async Task<IEnumerable<Order>> GetOrdersAsync() => await _orderService.GetOrdersAsync();

        /// <inheritdoc/>
        public async Task<Order?> GetOrderByIdAsync(Guid id) => await _orderService.GetOrderAsync(id);

        /// <inheritdoc/>
        public async Task<Order> CreateOrderAsync(Order order) => await _orderService.CreateOrderAsync(order);

        /// <inheritdoc/>
        public async Task UpdateOrderAsync(Order order) => await _orderService.UpdateOrderAsync(order);

        /// <inheritdoc/>
        public async Task DeleteOrderAsync(Guid orderId) => await _orderService.DeleteOrderAsync(orderId);

        /// <inheritdoc/>
        public async Task<OrderEntryData> GetOrderEntryDataAsync()
        {
            var tSuppliers = GetSuppliersAsync();
            var tCustomers = GetCustomersAsync();
            var tProjects = GetProjectsAsync();
            var tInventory = GetInventoryAsync();

            await Task.WhenAll(tSuppliers, tCustomers, tProjects, tInventory);

            return new OrderEntryData(
                await tSuppliers, 
                await tCustomers, 
                await tProjects, 
                await tInventory);
        }

        /// <inheritdoc/>
        public async Task ReceiveOrderAsync(Order order, List<OrderLine> receipts) => await _orderService.ReceiveOrderAsync(order, receipts);

        #endregion

        #region Supplier Operations

        /// <inheritdoc/>
        public async Task<IEnumerable<Supplier>> GetSuppliersAsync() => await _supplierService.GetSuppliersAsync();

        /// <inheritdoc/>
        public async Task DeleteSupplierAsync(Guid supplierId) => await _supplierService.DeleteSupplierAsync(supplierId);

        /// <inheritdoc/>
        public async Task UpdateSupplierAsync(Supplier supplier) => await _supplierService.UpdateSupplierAsync(supplier);

        /// <inheritdoc/>
        public async Task<Supplier> CreateSupplierAsync(Supplier supplier) => await _supplierService.CreateSupplierAsync(supplier);

        /// <inheritdoc/>
        public async Task<Supplier> QuickCreateSupplierAsync(string name)
        {
            var supplier = new Supplier { Name = name };
            return await _supplierService.CreateSupplierAsync(supplier);
        }

        #endregion

        #region Inventory Operations

        /// <inheritdoc/>
        public async Task<IEnumerable<InventoryItem>> GetInventoryAsync() => await _inventoryService.GetInventoryAsync();

        /// <inheritdoc/>
        public async Task<InventoryItem> CreateItemAsync(InventoryItem item) => await _inventoryService.CreateItemAsync(item);

        /// <inheritdoc/>
        public async Task DeleteItemAsync(Guid itemId) => await _inventoryService.DeleteItemAsync(itemId);

        /// <inheritdoc/>
        public async Task UpdateItemAsync(InventoryItem item) => await _inventoryService.UpdateItemAsync(item);

        /// <inheritdoc/>
        public async Task UpdateInventoryItemAsync(InventoryItem item) => await _inventoryService.UpdateItemAsync(item);

        /// <inheritdoc/>
        public async Task<InventoryItem> QuickCreateProductAsync(string name, string uom, string category, string supplierName)
        {
            var item = new InventoryItem
            {
                ProductName = name,
                UnitOfMeasure = uom,
                Category = string.IsNullOrWhiteSpace(category) ? "General" : category,
                Supplier = supplierName
            };
            return await _inventoryService.CreateItemAsync(item);
        }

        #endregion

        #region Project & Customer Operations

        /// <inheritdoc/>
        public async Task<IEnumerable<Customer>> GetCustomersAsync() => await _customerRepository.GetAllAsync();

        /// <inheritdoc/>
        public async Task<IEnumerable<Project>> GetProjectsAsync() => await _projectRepository.GetAllAsync();
        
        /// <inheritdoc/>
        public async Task<Project?> GetProjectByIdAsync(Guid id) => await _projectRepository.GetByIdAsync(id);

        /// <inheritdoc/>
        public async Task UpdateProjectAsync(Project project) => await _projectRepository.UpdateAsync(project);

        #endregion

        #region Dashboard & Analytics

        /// <inheritdoc/>
        public async Task<OrderDashboardStats> GetDashboardStatsAsync()
        {
            var allOrders = (await _orderService.GetOrdersAsync()).ToList();
            var inventory = (await _inventoryService.GetInventoryAsync()).ToList();

            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);

            var thisMonthOrders = allOrders.Where(o => o.OrderDate >= startOfMonth).ToList();
            var lastMonthOrders = allOrders.Where(o => o.OrderDate >= startOfLastMonth && o.OrderDate <= endOfLastMonth).ToList();

            int ordersThisMonthCount = thisMonthOrders.Count;
            double growth = 0;
            if (lastMonthOrders.Count > 0)
            {
                growth = ((double)(ordersThisMonthCount - lastMonthOrders.Count) / lastMonthOrders.Count) * 100;
            }
            else if (ordersThisMonthCount > 0)
            {
                growth = 100;
            }

            string growthSign = growth >= 0 ? "+" : "";
            string growthText = $"{growthSign}{growth:0}% from last month";
            string growthColor = growth >= 0 ? "#10B981" : "#EF4444";

            var pendingOrders = allOrders.Where(o => o.Status == OrderStatus.Ordered || o.Status == OrderStatus.PartialDelivery).ToList();
            int pendingCount = pendingOrders.Count;
            string pendingText;
            string pendingColor;

            var today = DateTime.Today;
            var arrivingTodayCount = pendingOrders.Count(o => o.ExpectedDeliveryDate?.Date == today);

            if (arrivingTodayCount > 0)
            {
                pendingText = $"{arrivingTodayCount} Arriving Today";
                pendingColor = "#F59E0B";
            }
            else
            {
                var nextOrder = pendingOrders.Where(o => o.ExpectedDeliveryDate?.Date > today)
                                             .OrderBy(o => o.ExpectedDeliveryDate)
                                             .FirstOrDefault();

                if (nextOrder != null && nextOrder.ExpectedDeliveryDate.HasValue)
                {
                    pendingText = $"Next order will arrive {nextOrder.ExpectedDeliveryDate.Value:dd MMM}";
                    pendingColor = "#64748B";
                }
                else
                {
                    pendingText = "No upcoming deliveries";
                    pendingColor = "#94A3B8";
                }
            }

            var recentOrders = allOrders.OrderByDescending(o => o.OrderDate).Take(5).ToList();
            var lowStockItems = inventory.Where(i => i.TrackLowStock && i.QuantityOnHand <= i.ReorderPoint).ToList();

            return new OrderDashboardStats(
                ordersThisMonthCount,
                growthText,
                growthColor,
                pendingCount,
                pendingText,
                pendingColor,
                recentOrders,
                lowStockItems.Take(5).ToList(),
                lowStockItems.Count);
        }

        /// <inheritdoc/>
        public async Task<Order> GetRestockOrderTemplateAsync()
        {
            var inventory = await _inventoryService.GetInventoryAsync();
            var lowStockItems = inventory.Where(i => i.TrackLowStock && i.QuantityOnHand <= i.ReorderPoint).ToList();

            if (!lowStockItems.Any()) return CreateNewOrderTemplate(OrderType.PurchaseOrder);

            // Group by Supplier and pick the one with most items for now (Single PO constraint)
            var supplierGroups = lowStockItems.GroupBy(i => i.Supplier).OrderByDescending(g => g.Count());
            var topSupplierGroup = supplierGroups.First();
            var supplierName = topSupplierGroup.Key;

            var itemsToOrder = topSupplierGroup.ToList();

            var order = CreateNewOrderTemplate(OrderType.PurchaseOrder);
            order.ExpectedDeliveryDate = DateTime.Today.AddDays(7);
            order.Notes = $"Auto-generated restock order for {supplierName}.";
            
            // Try to find supplier details to pre-fill
            var supplier = (await _supplierService.GetSuppliersAsync()).FirstOrDefault(s => s.Name == supplierName);
            if (supplier != null)
            {
                order.SupplierId = supplier.Id;
                order.SupplierName = supplier.Name;
                // Pre-fill email, address etc if Order model supports it directly or via Id
            }

            // Fetch all orders to determine last purchase price
            var allOrders = await _orderService.GetOrdersAsync();
            var lastPrices = new Dictionary<Guid, decimal>();

            // Helper to get last price
            foreach(var item in itemsToOrder)
            {
                var lastLine = allOrders
                    .Where(o => o.OrderType == OrderType.PurchaseOrder && o.Status != OrderStatus.Cancelled)
                    .OrderByDescending(o => o.OrderDate)
                    .SelectMany(o => o.Lines)
                    .FirstOrDefault(l => l.InventoryItemId == item.Id);

                if (lastLine != null && lastLine.UnitPrice > 0)
                {
                    lastPrices[item.Id] = lastLine.UnitPrice;
                }
                else
                {
                    lastPrices[item.Id] = item.AverageCost;
                }
            }

            foreach (var item in itemsToOrder)
            {
                // Calculate restock quantity: e.g. bring up to 2x ReorderPoint or just 1.5x.
                // For now, let's just order enough to double the ReorderPoint as a safe buffer, minus what we have.
                // Target = ReorderPoint * 2. 
                // QtyToOrder = Target - QtyOnHand.
                // Min Qty = 1.
                
                double target = item.ReorderPoint * 2; 
                double needed = target - item.QuantityOnHand;
                if (needed < 1) needed = 1;

                decimal unitPrice = lastPrices[item.Id];

                order.Lines.Add(new OrderLine
                {
                    OrderId = order.Id,
                    InventoryItemId = item.Id,
                    ItemCode = item.Sku,
                    Description = item.ProductName,
                    Category = item.Category,
                    UnitOfMeasure = item.UnitOfMeasure,
                    UnitPrice = unitPrice,
                    QuantityOrdered = needed,
                    LineTotal = (decimal)needed * unitPrice,
                    VatAmount = ((decimal)needed * unitPrice) * 0.15m // Approx
                });
            }

            return order;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OCC.Client.Models.RestockCandidate>> GetRestockCandidatesAsync()
        {
            var inventory = await _inventoryService.GetInventoryAsync();
            var orders = await _orderService.GetOrdersAsync();

            // Filter for active POs
            var activePOs = orders.Where(o => o.OrderType == OrderType.PurchaseOrder && 
                                              (o.Status == OrderStatus.Ordered || o.Status == OrderStatus.PartialDelivery));

            // Flatten lines to map: InventoryId -> QuantityRemaining
            // Note: OrderLine has RemainingQuantity property but it is calculated.
            // We need to match by InventoryItemId.
            var pendingQuantities = new Dictionary<Guid, double>();
            
            foreach (var order in activePOs)
            {
                foreach (var line in order.Lines)
                {
                    if (line.InventoryItemId.HasValue)
                    {
                        if (!pendingQuantities.ContainsKey(line.InventoryItemId.Value))
                            pendingQuantities[line.InventoryItemId.Value] = 0;
                        
                        pendingQuantities[line.InventoryItemId.Value] += line.RemainingQuantity;
                    }
                }
            }

            var candidates = new List<OCC.Client.Models.RestockCandidate>();
            
            foreach (var item in inventory)
            {
                if (!item.TrackLowStock) continue;

                double onOrder = 0;
                if (pendingQuantities.ContainsKey(item.Id))
                {
                    onOrder = pendingQuantities[item.Id];
                }

                // Rule: Show if Physical Stock is Low.
                // We display the 'On Order' part visually so the user knows if it's covered.
                // If we filtered by (Hand + Order) >= Reorder, we would never show the "Blue Bar" case the user described.
                if (item.QuantityOnHand >= item.ReorderPoint) continue;

                candidates.Add(new OCC.Client.Models.RestockCandidate
                {
                    Item = item,
                    QuantityOnOrder = onOrder
                });
            }

            return candidates;
        }

        #endregion

        #endregion

        #region Helper Methods

        /// <inheritdoc/>
        public Order CreateNewOrderTemplate(OrderType type = OrderType.PurchaseOrder)
        {
            string prefix = type switch
            {
                OrderType.PurchaseOrder => "PO",
                OrderType.SalesOrder => "SO",
                OrderType.ReturnToInventory => "RET",
                _ => "ORD"
            };

            return new Order
            {
                OrderDate = DateTime.Now,
                OrderNumber = $"{prefix}-{DateTime.Now:yyMM}-{new Random().Next(1000, 9999)}",
                OrderType = type,
                TaxRate = 0.15m,
                DestinationType = OrderDestinationType.Stock,
                Attention = string.Empty
            };
        }

        #endregion
    }
}
