using Avalonia.Data.Converters;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace OCC.Client.Converters
{
    public class BranchQuantityConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2 || values[0] is not InventoryItem item || values[1] is not Branch branch)
            {
                // Fallback or total if something is missing, or 0
                if (values.Count > 0 && values[0] is InventoryItem i) return i.QuantityOnHand.ToString();
                return "0";
            }

            return branch switch
            {
                Branch.JHB => item.JhbQuantity.ToString(),
                Branch.CPT => item.CptQuantity.ToString(),
                _ => item.QuantityOnHand.ToString()
            };
        }
    }
}
