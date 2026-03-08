using System;
using System.Linq;
using OCC.Shared.Models;
using OCC.WpfClient.Infrastructure;
using OCC.WpfClient.Services.Interfaces;

namespace OCC.WpfClient.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IAuthService _authService;

        public PermissionService(IAuthService authService)
        {
            _authService = authService;
        }

        public bool IsDev
        {
            get
            {
                var user = _authService.CurrentUser;
                if (user == null) return false;
                
                var email = user.Email?.ToLowerInvariant();
                return email == "neil@mdk.co.za" || email == "neil@origize63.co.za";
            }
        }

        public bool CanAccess(string route)
        {
            var user = _authService.CurrentUser;
            if (user == null) return false;

            // 1. System Overrides (Bypass)
            if (IsDev || user.UserRole == UserRole.Admin || user.UserRole == UserRole.SiteManager) 
                return true;

            // 2. Global Deny
            var restricted = new[] 
            { 
                NavigationRoutes.AuditLog, 
                NavigationRoutes.CompanySettings, 
                NavigationRoutes.CompanyProfile,
                NavigationRoutes.UserManagement, 
                NavigationRoutes.Feature_UserManagement, 
                NavigationRoutes.Feature_UserRegistration, 
                NavigationRoutes.Feature_WageViewing,
                NavigationRoutes.Developer 
            };
            if (restricted.Contains(route, StringComparer.OrdinalIgnoreCase)) return false;

            // 3. Global Allow
            var basic = new[] 
            { 
                NavigationRoutes.Home, 
                NavigationRoutes.Calendar, 
                NavigationRoutes.Notifications, 
                NavigationRoutes.UserPreferences, 
                NavigationRoutes.Feature_BugReports, 
                NavigationRoutes.Alerts, 
                NavigationRoutes.Reminders,
                NavigationRoutes.AccessDenied,
                "MyProfile",
                "Help",
                "SendLogs"
            };
            if (basic.Contains(route, StringComparer.OrdinalIgnoreCase)) return true;

            // 4. Role-Based Whitelisting (Office)
            if (user.UserRole == UserRole.Office)
            {
                if (route == "OrderList" || route == "Suppliers" || route == "CreateOrder")
                    return HasPermission(user, NavigationRoutes.Feature_OrderManagement);

                if (route == "Inventory" || route == "ItemList" || route == "RestockReview")
                    return HasPermission(user, NavigationRoutes.Feature_OrderManagement) || 
                           HasPermission(user, NavigationRoutes.Feature_OrderInventoryOnly);

                var toggleable = new[] 
                { 
                    NavigationRoutes.Time, 
                    NavigationRoutes.StaffManagement, 
                    NavigationRoutes.Projects, 
                    NavigationRoutes.Customers, 
                    NavigationRoutes.HealthSafety, 
                    NavigationRoutes.Feature_OrderManagement, 
                    NavigationRoutes.Feature_OrderInventoryOnly 
                };

                if (toggleable.Contains(route, StringComparer.OrdinalIgnoreCase))
                {
                    if (route == NavigationRoutes.Feature_OrderManagement)
                    {
                        return HasPermission(user, NavigationRoutes.Feature_OrderManagement) || 
                               HasPermission(user, NavigationRoutes.Feature_OrderInventoryOnly);
                    }

                    return HasPermission(user, route);
                }

                return false;
            }

            // 5. Contractor/Guest Access
            if (user.UserRole == UserRole.ExternalContractor || user.UserRole == UserRole.Guest)
            {
                return route switch
                {
                    NavigationRoutes.Projects => true, 
                    NavigationRoutes.Time => true, 
                    _ => false
                };
            }

            return false;
        }

        private bool HasPermission(User user, string route)
        {
            if (string.IsNullOrEmpty(user.Permissions)) return false;
            var allowed = user.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return allowed.Contains(route, StringComparer.OrdinalIgnoreCase);
        }
    }
}
