using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using OCC.Shared.Models;
using OCC.WpfClient.Infrastructure;
using OCC.WpfClient.Services.Interfaces;

namespace OCC.WpfClient.Features.Admin.Users.ViewModels
{
    public partial class UserDetailViewModel : DetailViewModelBase
    {
        private readonly UserListViewModel _parent;
        private readonly IUserService _userService;
        private readonly User _user;

        [ObservableProperty] private string _firstName;
        [ObservableProperty] private string _lastName;
        [ObservableProperty] private string _email;
        [ObservableProperty] private string? _phone;
        [ObservableProperty] private string? _location;
        [ObservableProperty] private UserRole _selectedRole;
        [ObservableProperty] private bool _isApproved;
        [ObservableProperty] private bool _isEmailVerified;
        [ObservableProperty] private string? _password;

        // Module Access
        [ObservableProperty] private bool _hasChatAccess;
        [ObservableProperty] private bool _hasUserManagementAccess;
        [ObservableProperty] private bool _hasEmployeeManagementAccess;
        [ObservableProperty] private bool _hasProcurementAccess;
        [ObservableProperty] private bool _hasInventoryAccess;
        [ObservableProperty] private bool _hasPurchaseOrderAccess;

        [ObservableProperty] private bool _showModuleAccess;

        public List<UserRole> Roles => new List<UserRole>
        {
            UserRole.Admin,
            UserRole.Office,
            UserRole.ExternalContractor,
            UserRole.HSEQ
        };

        public UserDetailViewModel(UserListViewModel parent, User user, IUserService userService, IDialogService dialogService, ILogger logger) : base(dialogService, logger)
        {
            _parent = parent;
            _user = user;
            _userService = userService;

            _firstName = user.FirstName;
            _lastName = user.LastName;
            _email = user.Email;
            _phone = user.Phone;
            _location = user.Location;
            _selectedRole = user.UserRole;
            _isApproved = user.IsApproved;
            _isEmailVerified = user.IsEmailVerified;

            _showModuleAccess = _selectedRole == UserRole.Office;
            LoadPermissions(user.Permissions);
        }

        partial void OnSelectedRoleChanged(UserRole value)
        {
            ShowModuleAccess = value == UserRole.Office;
            if (value == UserRole.Admin)
            {
                HasChatAccess = true;
                HasUserManagementAccess = true;
                HasEmployeeManagementAccess = true;
                HasProcurementAccess = true;
                HasInventoryAccess = true;
                HasPurchaseOrderAccess = true;
            }
        }

        private void LoadPermissions(string? permissions)
        {
            if (string.IsNullOrEmpty(permissions)) return;

            var current = permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            HasChatAccess = current.Contains(NavigationRoutes.Chat, StringComparer.OrdinalIgnoreCase);
            HasUserManagementAccess = current.Contains(NavigationRoutes.UserManagement, StringComparer.OrdinalIgnoreCase);
            HasEmployeeManagementAccess = current.Contains(NavigationRoutes.StaffManagement, StringComparer.OrdinalIgnoreCase);
            HasProcurementAccess = current.Contains(NavigationRoutes.Procurement, StringComparer.OrdinalIgnoreCase);
            HasInventoryAccess = current.Contains(NavigationRoutes.Inventory, StringComparer.OrdinalIgnoreCase);
            HasPurchaseOrderAccess = current.Contains(NavigationRoutes.PurchaseOrder, StringComparer.OrdinalIgnoreCase);
        }

        private string GetPermissionsString()
        {
            var selected = new List<string>();
            if (HasChatAccess) selected.Add(NavigationRoutes.Chat);
            if (HasUserManagementAccess) selected.Add(NavigationRoutes.UserManagement);
            if (HasEmployeeManagementAccess) selected.Add(NavigationRoutes.StaffManagement);
            if (HasProcurementAccess) selected.Add(NavigationRoutes.Procurement);
            if (HasInventoryAccess) selected.Add(NavigationRoutes.Inventory);
            if (HasPurchaseOrderAccess) selected.Add(NavigationRoutes.PurchaseOrder);
            
            return string.Join(",", selected);
        }

        protected override async Task ExecuteSaveAsync()
        {
            _user.FirstName = FirstName;
            _user.LastName = LastName;
            _user.Email = Email;
            _user.Phone = Phone;
            _user.Location = Location;
            _user.UserRole = SelectedRole;
            _user.IsApproved = IsApproved;
            _user.IsEmailVerified = IsEmailVerified;
            
            if (!string.IsNullOrWhiteSpace(Password))
            {
                _user.Password = Password;
            }
            
            _user.Permissions = GetPermissionsString();

            bool success;
            if (_user.Id == Guid.Empty)
            {
                success = await _userService.CreateUserAsync(_user);
            }
            else
            {
                success = await _userService.UpdateUserAsync(_user);
            }

            if (!success)
            {
                throw new Exception("Failed to save user. Please check your connection.");
            }
        }

        protected override void OnSaveSuccess()
        {
            _parent.LoadData().ConfigureAwait(false);
            _parent.CloseDetailView();
        }

        protected override async Task ExecuteReloadAsync()
        {
            var latest = await _userService.GetUserAsync(_user.Id);
            if (latest != null)
            {
                _user.FirstName = latest.FirstName;
                _user.LastName = latest.LastName;
                _user.Email = latest.Email;
                _user.Phone = latest.Phone;
                _user.Location = latest.Location;
                _user.UserRole = latest.UserRole;
                _user.IsApproved = latest.IsApproved;
                _user.IsEmailVerified = latest.IsEmailVerified;
                _user.Permissions = latest.Permissions;
                _user.RowVersion = latest.RowVersion;

                FirstName = _user.FirstName;
                LastName = _user.LastName;
                Email = _user.Email;
                Phone = _user.Phone;
                Location = _user.Location;
                SelectedRole = _user.UserRole;
                IsApproved = _user.IsApproved;
                IsEmailVerified = _user.IsEmailVerified;
                
                LoadPermissions(_user.Permissions);
                
                Title = $"Edit {FirstName} {LastName} (Reloaded)";
            }
        }

        protected override void OnCancel()
        {
            _parent.CloseDetailView();
        }

    }
}
