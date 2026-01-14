using System;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.EmployeeManagement
{
    public partial class EmployeePermissionsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _userName = string.Empty;

        [ObservableProperty]
        private Guid? _selectedUserId;

        [ObservableProperty]
        private List<User> _availableUsers = new();

        public partial class PermissionItem : ObservableObject
        {
            public string Key { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            
            [ObservableProperty]
            private bool _isSelected;
        }

        [ObservableProperty]
        private List<PermissionItem> _permissions = new();

        public System.Action<string?, Guid?>? OnSaved;

        public EmployeePermissionsViewModel(string userName, string? currentPermissions, List<User> availableUsers, Guid? currentUserId)
        {
            UserName = userName;
            AvailableUsers = availableUsers;
            SelectedUserId = currentUserId;
            
            // Define all possible permissions
            var availablePermissions = new List<(string Key, string DisplayName)>
            {
                ("Home", "Home / Dashboard"),
                ("Time", "Time & Attendance"),
                ("Team", "Employee Management"),
                ("Portfolio", "Projects Portfolio"),
                ("Orders", "Orders & Restocking"),
                ("HealthSafety", "Health & Safety"),
                ("Calendar", "Calendar"),
                ("AuditLog", "Audit Logs"),
                ("BugList", "Bug Reports"),
                ("CompanySettings", "Company Settings"),
                ("UserPreferences", "User Preferences")
            };

            var current = (currentPermissions ?? string.Empty)
                .Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

            Permissions = availablePermissions.Select(p => new PermissionItem
            {
                Key = p.Key,
                DisplayName = p.DisplayName,
                IsSelected = current.Contains(p.Key, System.StringComparer.OrdinalIgnoreCase)
            }).ToList();
        }

        [RelayCommand]
        private void Save()
        {
            var selected = Permissions.Where(p => p.IsSelected).Select(p => p.Key);
            var permissionsString = string.Join(",", selected);
            OnSaved?.Invoke(permissionsString, SelectedUserId);
        }
    }
}
