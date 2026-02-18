using Avalonia.Data.Converters;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using OCC.Shared.DTOs;

namespace OCC.Client.Converters
{
    public class BranchQuantityConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2 || values[1] is not Branch branch)
                return "0";

            if (values[0] is InventoryItem item)
            {
                return branch switch
                {
                    Branch.JHB => item.JhbQuantity.ToString(),
                    Branch.CPT => item.CptQuantity.ToString(),
                    _ => item.QuantityOnHand.ToString()
                };
            }
            
            if (values[0] is InventorySummaryDto summary)
            {
                return branch switch
                {
                    Branch.JHB => summary.JhbQuantity.ToString(),
                    Branch.CPT => summary.CptQuantity.ToString(),
                    _ => summary.QuantityOnHand.ToString()
                };
            }

            return "0";
        }
    }
}
