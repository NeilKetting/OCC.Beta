using System;

namespace OCC.Shared.Models
{
    public class InventoryItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string Supplier { get; set; } = string.Empty;
        public string Location { get; set; } = "Warehouse"; // Rack A, Shelf 1
        
        public double JhbQuantity { get; set; }
        public double CptQuantity { get; set; }
        public double QuantityOnHand { get; set; } // Aggregate or total stock
        public double ReorderPoint { get; set; }
        public string UnitOfMeasure { get; set; } = "ea";
        
        public string Sku { get; set; } = string.Empty;
        public decimal AverageCost { get; set; }
        public bool TrackLowStock { get; set; } = true;
        public bool IsStockItem { get; set; } = true;

        // Status
        public InventoryStatus Status => TrackLowStock && QuantityOnHand <= ReorderPoint ? InventoryStatus.Low : InventoryStatus.OK;
        
        // Alias for View Binding compatibility
        public InventoryStatus InventoryStatus => Status;
    }

    public enum InventoryStatus
    {
        OK,
        Low,
        Critical
    }
}
