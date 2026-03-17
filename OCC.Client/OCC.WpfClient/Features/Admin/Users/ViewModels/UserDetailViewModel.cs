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
    public partial class UserDetailViewModel : ViewModelBase
    {
        private readonly UserListViewModel _parent;
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly User _user;

        [ObservableProperty] private string _firstName;
        [ObservableProperty] private string _lastName;
        [ObservableProperty] private string _email;
        [ObservableProperty] private string? _phone;
        [ObservableProperty] private string? _location;
        [ObservableProperty] private UserRole _selectedRole;
        [ObservableProperty] private bool _isApproved;
        [ObservableProperty] private bool _isEmailUnknown;

        // Module Access
        [ObservableProperty] private bool _hasOrdersAccess;
        [ObservableProperty] private bool _hasInventoryAccess;
        [ObservableProperty] private bool _hasProjectsAccess;

        public List<UserRole> Roles => Enum.GetValues(typeof(UserRole)).Cast<UserRole>().ToList();

        public UserDetailViewModel(UserListViewModel parent, User user, IUserService userService, ILogger logger)
        {
            _parent = parent;
            _user = user;
            _userService = userService;
            _logger = logger;

            _firstName = user.FirstName;
            _lastName = user.LastName;
            _email = user.Email;
            _phone = user.Phone;
            _location = user.Location;
            _selectedRole = user.UserRole;
            _isApproved = user.IsApproved;
            _isEmailUnknown = !user.IsEmailVerified;

            LoadPermissions(user.Permissions);
        }

        private void LoadPermissions(string? permissions)
        {
            if (string.IsNullOrEmpty(permissions)) return;

            var current = permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            HasOrdersAccess = current.Contains(NavigationRoutes.Feature_OrderManagement, StringComparer.OrdinalIgnoreCase);
            HasInventoryAccess = current.Contains(NavigationRoutes.Feature_OrderInventoryOnly, StringComparer.OrdinalIgnoreCase);
            HasProjectsAccess = current.Contains(NavigationRoutes.Feature_ProjectCreation, StringComparer.OrdinalIgnoreCase);
        }

        private string GetPermissionsString()
        {
            var selected = new List<string>();
            if (HasOrdersAccess) selected.Add(NavigationRoutes.Feature_OrderManagement);
            if (HasInventoryAccess) selected.Add(NavigationRoutes.Feature_OrderInventoryOnly);
            if (HasProjectsAccess) selected.Add(NavigationRoutes.Feature_ProjectCreation);
            
            return string.Join(",", selected);
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                IsBusy = true;
                BusyText = "Saving user details...";

                _user.FirstName = FirstName;
                _user.LastName = LastName;
                _user.Email = Email;
                _user.Phone = Phone;
                _user.Location = Location;
                _user.UserRole = SelectedRole;
                _user.IsApproved = IsApproved;
                _user.IsEmailVerified = !IsEmailUnknown;
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

                if (success)
                {
                    await _parent.LoadData();
                    _parent.CloseDetailView();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            _parent.CloseDetailView();
        }

        [RelayCommand]
        private void ResetPassword()
        {
            // Placeholder for password reset logic
            _logger.LogInformation("Password reset requested for {Email}", Email);
        }
    }
}
