using Microsoft.Extensions.DependencyInjection;
using OCC.WpfClient.Features.ProcurementHub.ViewModels;
using OCC.WpfClient.Infrastructure;
using OCC.WpfClient.Services.Interfaces;
using OCC.WpfClient.Services;
using System.Collections.Generic;

namespace OCC.WpfClient.Features.ProcurementHub
{
    public class ProcurementFeature : IFeature
    {
        public string Name => "Procurement";
        public string Description => "Supply Chain, Inventory and Supplier Management";
        public string Icon => "IconSummary";
        public int Order => 40;

        public void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<ProcurementViewModel>();
            services.AddTransient<InventoryViewModel>();
            services.AddTransient<PurchaseOrderViewModel>();

            services.AddTransient<ISupplierService, SupplierService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IProjectService, ProjectService>();
        }

        public void RegisterRoutes(INavigationService navigationService)
        {
            navigationService.RegisterRoute(NavigationRoutes.Procurement, typeof(ProcurementViewModel));
            navigationService.RegisterRoute(NavigationRoutes.Inventory, typeof(InventoryViewModel));
            navigationService.RegisterRoute(NavigationRoutes.PurchaseOrder, typeof(PurchaseOrderViewModel));
        }

        public IEnumerable<NavItem> GetNavigationItems()
        {
            yield return new NavItem(
                "Procurement Dashboard",
                "IconSummary",
                NavigationRoutes.Procurement,
                "Operations");

            yield return new NavItem(
                "Inventory Management",
                "IconDatabase",
                NavigationRoutes.Inventory,
                "Operations");

            yield return new NavItem(
                "Create Purchase Order",
                "IconAdd",
                NavigationRoutes.PurchaseOrder,
                "Actions");
        }
    }
}
