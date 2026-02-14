using Moq;
using OCC.Client.Features.OrdersHub.ViewModels;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OCC.Tests.ViewModels
{
    public class InventoryLookupViewModelTests
    {
        private readonly Mock<IOrderManager> _mockOrderManager;
        private readonly Mock<IDialogService> _mockDialogService;
        private readonly InventoryLookupViewModel _viewModel;

        public InventoryLookupViewModelTests()
        {
            _mockOrderManager = new Mock<IOrderManager>();
            _mockDialogService = new Mock<IDialogService>();
            
            _viewModel = new InventoryLookupViewModel(
                _mockOrderManager.Object,
                _mockDialogService.Object,
                NullLogger<InventoryLookupViewModel>.Instance);
        }

        [Fact]
        public void Initialize_PopulatesCategories()
        {
            // Arrange
            var inventory = new List<InventoryItem>
            {
                new InventoryItem { Category = "Tools" },
                new InventoryItem { Category = "Materials" }
            };

            // Act
            _viewModel.Initialize(inventory);

            // Assert
            Assert.Contains("Tools", _viewModel.Categories);
            Assert.Contains("Materials", _viewModel.Categories);
            Assert.Contains("General", _viewModel.Categories);
        }

        [Fact]
        public void Filter_WeightedSearch_OrdersCorrectly()
        {
            // Arrange
            var inventory = new List<InventoryItem>
            {
                new InventoryItem { Sku = "BEER", Description = "Cold Beer" },
                new InventoryItem { Sku = "B123", Description = "Beer Bottle" },
                new InventoryItem { Sku = "C1", Description = "Cheese and Beer" }
            };
            _viewModel.Initialize(inventory);

            // Act
            _viewModel.SearchText = "Beer";
            _viewModel.Filter();

            // Assert
            // SKU exact match (BEER) should be first
            Assert.Equal("BEER", _viewModel.FilteredItems[0].Sku);
            // Description starts with (Beer Bottle) should be before "Cheese and Beer"
            Assert.Equal("B123", _viewModel.FilteredItems[1].Sku);
        }

        [Fact]
        public async Task QuickCreateProduct_AddsToMaster_AndRefreshesFilter()
        {
            // Arrange
            _viewModel.Initialize(new List<InventoryItem>());
            _viewModel.NewDescription = "New Gadget";
            _viewModel.NewCategory = "Gadgets";
            var created = new InventoryItem { Description = "New Gadget", Category = "Gadgets" };
            
            _mockOrderManager.Setup(m => m.QuickCreateProductAsync("New Gadget", It.IsAny<string>(), "Gadgets", It.IsAny<string>()))
                .ReturnsAsync(created);

            // Act
            await _viewModel.QuickCreateProduct();

            // Assert
            Assert.Contains(created, _viewModel.FilteredItems);
            Assert.Contains("Gadgets", _viewModel.Categories);
            Assert.Equal(created, _viewModel.SelectedItem);
        }
    }
}
