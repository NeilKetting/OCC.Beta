-- SQL Script to seed InventoryItems table with 20 sample items
-- Run this script in your SQL Server database to populate data for testing.

INSERT INTO InventoryItems (Id, Description, Supplier, Category, Location, JhbQuantity, CptQuantity, QuantityOnHand, ReorderPoint, UnitOfMeasure, Sku, AverageCost, Price, TrackLowStock, IsStockItem)
VALUES
(NEWID(), 'Portland Cement 42.5N', 'Builders Warehouse', 'Material', 'Warehouse A', 50, 30, 80, 20, 'bag', 'MAT-CEM-001', 95.50, 135.00, 1, 1),
(NEWID(), 'Red Clay Bricks (Pallet)', 'Cashbuild', 'Material', 'Yard 1', 10, 5, 15, 5, 'pallet', 'MAT-BRK-050', 1200.00, 1650.00, 1, 1),
(NEWID(), 'Safety Helmet (White)', 'Safety First', 'Safety', 'Office Store', 15, 20, 35, 10, 'ea', 'SAF-HLM-WHT', 45.00, 85.00, 1, 0),
(NEWID(), 'Safety Vest (Reflective)', 'Safety First', 'Safety', 'Office Store', 40, 40, 80, 20, 'ea', 'SAF-VST-ORG', 35.00, 65.00, 1, 0),
(NEWID(), 'Angle Grinder 115mm', 'Makro', 'Tool', 'Secure Cage', 3, 2, 5, 2, 'ea', 'TOL-AGR-115', 850.00, 1200.00, 1, 1),
(NEWID(), 'Extension Cord 20m', 'Voltex', 'Electrical', 'Warehouse B', 8, 4, 12, 5, 'ea', 'ELE-EXT-020', 250.00, 400.00, 1, 1),
(NEWID(), 'Copper Pipe 15mm (5m)', 'Plumblink', 'Plumbing', 'Rack 3', 30, 20, 50, 15, 'length', 'PLU-COP-015', 180.00, 280.00, 1, 1),
(NEWID(), 'PVC Elbow 110mm', 'Plumblink', 'Plumbing', 'Rack 3', 60, 40, 100, 30, 'ea', 'PLU-PVC-110', 45.00, 75.00, 1, 1),
(NEWID(), 'Circuit Breaker 20A', 'Voltex', 'Electrical', 'Shelf E2', 25, 15, 40, 10, 'ea', 'ELE-CBK-020', 65.00, 110.00, 1, 1),
(NEWID(), 'LED Downlight 5W', 'Voltex', 'Electrical', 'Shelf E3', 100, 50, 150, 40, 'ea', 'ELE-LED-005', 35.00, 60.00, 1, 1),
(NEWID(), 'Spade (Round Nose)', 'Builders Warehouse', 'Tool', 'Yard Store', 12, 8, 20, 5, 'ea', 'TOL-SPD-RND', 120.00, 180.00, 1, 1),
(NEWID(), 'Wheelbarrow (Heavy Duty)', 'Builders Warehouse', 'Tool', 'Yard Store', 5, 3, 8, 3, 'ea', 'TOL-WHL-HDY', 650.00, 950.00, 1, 1),
(NEWID(), 'Pine Timber 38x38 (3m)', 'Cashbuild', 'Material', 'Warehouse A', 200, 100, 300, 50, 'length', 'MAT-TIM-038', 45.00, 70.00, 1, 1),
(NEWID(), 'Plaster Primer 20L', 'Dulux', 'Paint', 'Warehouse B', 10, 5, 15, 5, 'drum', 'PNT-PRI-020', 890.00, 1350.00, 1, 1),
(NEWID(), 'Roof Paint Charcoal 20L', 'Dulux', 'Paint', 'Warehouse B', 8, 4, 12, 4, 'drum', 'PNT-ROF-CHR', 1100.00, 1600.00, 1, 1),
(NEWID(), 'Drill Bit Set (Masonry)', 'Makro', 'Tool', 'Secure Cage', 10, 10, 20, 5, 'set', 'TOL-DRL-MAS', 150.00, 250.00, 1, 1),
(NEWID(), 'Hammer 500g', 'Makro', 'Tool', 'Rack 1', 15, 10, 25, 5, 'ea', 'TOL-HAM-500', 95.00, 150.00, 1, 1),
(NEWID(), 'Tape Measure 5m', 'Makro', 'Tool', 'Rack 1', 20, 15, 35, 10, 'ea', 'TOL-TAP-005', 45.00, 80.00, 1, 0),
(NEWID(), 'Work Gloves (Leather)', 'Safety First', 'Safety', 'Office Store', 50, 30, 80, 20, 'pair', 'SAF-GLV-LTH', 30.00, 55.00, 1, 0),
(NEWID(), 'Safety Boots Size 9', 'Safety First', 'Safety', 'Office Store', 5, 5, 10, 3, 'pair', 'SAF-BOT-009', 450.00, 750.00, 1, 0);

-- Query to verify insertion
-- SELECT * FROM InventoryItems;
