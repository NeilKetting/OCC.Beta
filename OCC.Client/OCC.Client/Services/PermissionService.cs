using OCC.Client.Infrastructure;
using System;
using System.Linq;
using OCC.Shared.Models;

using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.Services.Infrastructure; // If needed

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

            // Dev has access to everything
            if (IsDev) return true;

            // Admin access
            if (user.UserRole == UserRole.Admin) return true;

            // Special Check for Wage Visibility
            if (route == "WageViewing")
            {
                // Only Admin can see wages
                return false;
            }

            // Dynamic Permission Check for Office and SiteManager
            if (user.UserRole == UserRole.Office || user.UserRole == UserRole.SiteManager)
            {
                // If permissions are explicitly set (not null), we enforce STRICT access control.
                // Even if the string is empty (user.Permissions == ""), it means the user has 0 permissions.
                // We do NOT fall back to default role permissions in this case.
                if (user.Permissions != null)
                {
                    var allowedRoutes = user.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    return allowedRoutes.Contains(route, StringComparer.OrdinalIgnoreCase);
                }

                // Fallback to defaults ONLY if user.Permissions is NULL (i.e., permissions have never been configured for this user)
                if (user.UserRole == UserRole.Office)
                {
                    return route switch
                    {
                        "Home" => true,
                        "Time" => true,
                        "HealthSafety" => true,
                        "LeaveApproval" => true,
                        "OvertimeRequest" => true,
                        "OvertimeApproval" => true,
                        "Orders" => true,
                        _ => false
                    };
                }

                if (user.UserRole == UserRole.SiteManager)
                {
                    return route switch
                    {
                        "Home" => true,
                        "Time" => true,
                        "HealthSafety" => true,
                        "RollCall" => true,
                        "ClockOut" => true,
                        "LeaveApproval" => true,
                        "OvertimeRequest" => true,
                        "OvertimeApproval" => true,
                        "Teams" => true,
                        _ => false
                    };
                }
            }

            // Contractor/Guest Access
            // ... (No change needed, valid logic)
            if (user.UserRole == UserRole.ExternalContractor || user.UserRole == UserRole.Guest)
            {
                 return route switch
                {
                    NavigationRoutes.Home => true,
                    NavigationRoutes.Projects => true, 
                    NavigationRoutes.Time => true, 
                    NavigationRoutes.Calendar => true,
                    NavigationRoutes.Notifications => true,
                    _ => false
                };
            }

            return false;
        }
    }
}
