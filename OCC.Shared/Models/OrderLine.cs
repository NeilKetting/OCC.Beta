using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OCC.Shared.Models
{
    public class OrderLine : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _itemCode = string.Empty;
        private string _description = string.Empty;
        private double _quantityOrdered;
        private string _unitOfMeasure = "";
        private decimal _unitPrice;
        private decimal _lineTotal;

        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        
        public Guid? InventoryItemId { get; set; }
        
        public string ItemCode 
        { 
            get => _itemCode; 
            set { _itemCode = value; OnPropertyChanged(); } 
        }

        public string Description 
        { 
            get => _description; 
            set { _description = value; OnPropertyChanged(); } 
        }

        public string Category { get; set; } = "General";
        
        public double QuantityOrdered 
        { 
            get => _quantityOrdered; 
            set 
            { 
                if (_quantityOrdered != value)
                {
                    _quantityOrdered = value; 
                    OnPropertyChanged(); 
                    OnPropertyChanged(nameof(RemainingQuantity)); 
                    OnPropertyChanged(nameof(IsComplete));
                    CalculateTotal(0.15m); // Default VAT 15% for immediate UI update
                }
            } 
        }

        public double QuantityReceived { get; set; }
        public string UnitOfMeasure 
        { 
            get => _unitOfMeasure; 
            set { _unitOfMeasure = value; OnPropertyChanged(); } 
        }
        
        public decimal UnitPrice 
        { 
            get => _unitPrice; 
            set 
            {
                if (_unitPrice != value)
                {
                    _unitPrice = value; 
                    OnPropertyChanged();
                    CalculateTotal(0.15m); // Default VAT 15%
                }
            } 
        }
        
        public decimal VatAmount { get; set; } 
        
        public decimal LineTotal 
        { 
            get => _lineTotal; 
            set { _lineTotal = value; OnPropertyChanged(); } 
        }

        public double RemainingQuantity => Math.Max(0, QuantityOrdered - QuantityReceived);
        public bool IsComplete => QuantityReceived >= QuantityOrdered;
        
        public void CalculateTotal(decimal taxRate)
        {
            decimal qty = (decimal)QuantityOrdered;
            decimal price = UnitPrice;
            
            decimal sub = qty * price;
            VatAmount = sub * taxRate;
            LineTotal = sub; 
        }
    }
}
