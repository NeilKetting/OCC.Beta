using OCC.Client.Infrastructure;
using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;
using System;
using System.Linq;

namespace OCC.Client.Services
{
    /// <summary>
    /// Service responsible for determining user access and permissions based on their role.
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionService"/> class.
        /// </summary>
        /// <param name="authService">The authentication service to retrieve the current user.</param>
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

        /// <summary>
        /// Determines if the current user has access to a specific route or feature.
        /// </summary>
        /// <param name="route">The navigation route or feature key to check access for.</param>
        /// <returns>True if the user is authorized; otherwise, false.</returns>
        public bool CanAccess(string route)
        {
            var user = _authService.CurrentUser;
            if (user == null) return false;

            // 1. Dev & Admin & SiteManager (Temp) bypass everything
            if (IsDev || user.UserRole == UserRole.Admin || user.UserRole == UserRole.SiteManager) 
                return true;

            // 2. Core Features (Always Allowed for All Roles)
            if (route == NavigationRoutes.Feature_BugReports || 
                route == NavigationRoutes.Home || 
                route == NavigationRoutes.Calendar || 
                route == NavigationRoutes.Notifications || 
                route == NavigationRoutes.UserPreferences || 
                route == NavigationRoutes.Alerts)
            {
                return true;
            }

            // 3. Sensitive/Restricted Features (Always Restricted except for bypassed roles above)
            if (route == NavigationRoutes.AuditLog || 
                route == NavigationRoutes.CompanySettings || 
                route == NavigationRoutes.UserManagement || 
                route == NavigationRoutes.Feature_UserManagement || 
                route == NavigationRoutes.Feature_UserRegistration ||
                route == NavigationRoutes.Feature_WageViewing)
            {
                return false;
            }

            // 4. Toggleable Role-Based Access (Office)
            if (user.UserRole == UserRole.Office)
            {
                // These are the only things Office users can toggle
                var toggleableModules = new[] 
                { 
                    NavigationRoutes.Time, 
                    NavigationRoutes.StaffManagement, 
                    NavigationRoutes.Projects, 
                    NavigationRoutes.Feature_OrderManagement, 
                    NavigationRoutes.HealthSafety 
                };

                if (toggleableModules.Contains(route))
                {
                    if (string.IsNullOrEmpty(user.Permissions)) return false;
                    
                    var allowedRoutes = user.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    return allowedRoutes.Contains(route, StringComparer.OrdinalIgnoreCase);
                }

                // Default for Office for any other feature not explicitly handled above
                return true;
            }

            // 5. Contractor/Guest Access (Fallback)
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
    }
}
