using System;
using OCC.Shared.Models;

namespace OCC.Client.Models
{
    public class RestockCandidate
    {
        public InventoryItem Item { get; set; } = new();
        public double QuantityOnOrder { get; set; } // Approved/Ordered POs
        
        public double Gap => Math.Max(0, Item.ReorderPoint - (Item.QuantityOnHand + QuantityOnOrder));
    }
}
