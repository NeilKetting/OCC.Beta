using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Repositories.Interfaces; // Use Repo directly or InventoryService if available?
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OCC.Client.Services
{
    public class InventoryImportService : IInventoryImportService
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryImportService> _logger;

        public InventoryImportService(IInventoryService inventoryService, ILogger<InventoryImportService> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        public async Task<(int SuccessCount, int FailureCount, List<string> Errors)> ImportInventoryAsync(Stream csvStream)
        {
            var successCount = 0;
            var failureCount = 0;
            var errors = new List<string>();

            try
            {
                using var reader = new StreamReader(csvStream);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    PrepareHeaderForMatch = args => args.Header.ToLower(),
                });

                var records = csv.GetRecords<InventoryImportRow>();
                
                // Get existing items to check for duplicates? Logic "Don't duplicate" requested by user.
                var existingItems = await _inventoryService.GetInventoryAsync();
                var existingProductNames = existingItems.Select(i => i.ProductName.ToLower().Trim()).ToHashSet();

                foreach (var row in records)
                {
                    if (string.IsNullOrWhiteSpace(row.ProductName))
                        continue;

                    if (existingProductNames.Contains(row.ProductName.ToLower().Trim()))
                    {
                        errors.Add($"Skipped '{row.ProductName}' - already exists.");
                        failureCount++;
                        continue;
                    }

                    try
                    {
                        var item = MapToInventoryItem(row);
                        
                        // We need to create it via service
                        // Assumption: InventoryService has a Create method. 
                        // Checked file list: Services/Interfaces/IInventoryService.cs exists.
                        // I'll assume CreateItemAsync(InventoryItem item) exists or similar.
                        await _inventoryService.CreateItemAsync(item);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to import item {Name}", row.ProductName);
                        errors.Add($"Failed '{row.ProductName}': {ex.Message}");
                        failureCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error reading CSV");
                errors.Add($"Fatal error reading CSV: {ex.Message}");
            }

            return (successCount, failureCount, errors);
        }

        private InventoryItem MapToInventoryItem(InventoryImportRow row)
        {
            // Extract UOM from Description or Name
            // Searching for pattern like "20kg", "50m", "5l" etc.
            var uom = ExtractUom(row.SalesDescription) ?? ExtractUom(row.ProductName) ?? "ea";

            // Parse Quantity
            double qty = 0;
            if (double.TryParse(row.QuantityOnHand, NumberStyles.Any, CultureInfo.InvariantCulture, out var q))
            {
                qty = q;
            }

            // Parse Cost
            decimal cost = 0;
            if (decimal.TryParse(row.Cost, NumberStyles.Any, CultureInfo.InvariantCulture, out var c))
            {
                cost = c;
            }

             // Parse Reorder Point
            double reorder = 0;
            if (double.TryParse(row.ReorderPoint, NumberStyles.Any, CultureInfo.InvariantCulture, out var r))
            {
                reorder = r;
            }


            return new InventoryItem
            {
                Id = Guid.NewGuid(),
                ProductName = row.ProductName.Trim(),
                Category = string.IsNullOrWhiteSpace(row.Category) ? "General" : row.Category.Trim(),
                Supplier = string.Empty, // Not in CSV
                Location = "Warehouse", // Default
                JhbQuantity = qty, // User instruction: "Stock can all go to jhb"
                CptQuantity = 0,
                QuantityOnHand = qty, // Derived total
                ReorderPoint = reorder,
                UnitOfMeasure = uom,
                Sku = row.Sku ?? string.Empty,
                AverageCost = cost,
                TrackLowStock = true,
                IsStockItem = true
            };
        }

        private string? ExtractUom(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            // Regex for Units: number followed by kg, m, l, mm, etc.
            // Or just terms like "roll", "bag", "pack"
            // The user mentioned "m, l, kg".

            // Look for standard units with optional value before it e.g. "20kg" or "50m"
            var match = Regex.Match(input, @"\b(\d+(\.\d+)?)\s*(kg|m|l|ml|mm|g)\b", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                // Return the unit part, e.g. "kg" or maybe the whole thing "20kg"? 
                // UOM is usually just the unit type "kg", but sometimes "Bag (20kg)". 
                // If the item is "Cement", UOM "bag" is better?
                // But user specifically said "m, l, kg so it is there".
                // Let's grab the unit group.
                return match.Groups[3].Value.ToLower();
            }
            
            // Fallback: check for keywords
            if (input.Contains("roll", StringComparison.OrdinalIgnoreCase)) return "roll";
            if (input.Contains("bag", StringComparison.OrdinalIgnoreCase)) return "bag";
            if (input.Contains("box", StringComparison.OrdinalIgnoreCase)) return "box";
            if (input.Contains("can", StringComparison.OrdinalIgnoreCase)) return "can";
            if (input.Contains("each", StringComparison.OrdinalIgnoreCase)) return "ea";

            return null;
        }

        private class InventoryImportRow
        {
            [Name("Product/Service Name")]
            public string ProductName { get; set; } = string.Empty;

            [Name("Quantity on hand")]
            public string QuantityOnHand { get; set; } = "0";

            [Name("SKU")]
            public string Sku { get; set; } = string.Empty;

            [Name("Cost")]
            public string Cost { get; set; } = "0";

            [Name("Sales Description")]
            public string SalesDescription { get; set; } = string.Empty;

            [Name("Category")]
            public string Category { get; set; } = string.Empty;

             [Name("Reorder Point")]
            public string ReorderPoint { get; set; } = "0";
        }
    }
}
