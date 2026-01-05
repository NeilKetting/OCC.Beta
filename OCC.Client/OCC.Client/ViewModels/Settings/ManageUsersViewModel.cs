using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services;
using OCC.Shared.Models;
using System.Linq;

namespace OCC.Client.ViewModels.Settings
{
    public partial class ManageUsersViewModel : ViewModelBase
    {
        private readonly IRepository<User> _userRepository;

        [ObservableProperty]
        private string _activeTab = "Manage Users";

        [ObservableProperty]
        private int _totalLicenses = 1;

        [ObservableProperty]
        private int _allocatedLicenses = 1;

        [ObservableProperty]
        private int _availableLicenses = 0;

        [ObservableProperty]
        private int _guests = 0;
        
        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<User> _users = new();

        public ManageUsersViewModel(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
            LoadData(); // In real app, might want to delay this until view shown but OK for now.
        }

        public async void LoadData()
        {
             // Simulate loading users or fetch from repository
             // For now, let's create some dummy data if repository is empty or just fetch
             var users = await _userRepository.GetAllAsync();
             Users = new ObservableCollection<User>(users);

             AllocatedLicenses = Users.Count;
             AvailableLicenses = TotalLicenses - AllocatedLicenses;
             if (AvailableLicenses < 0) AvailableLicenses = 0;
        }

        [RelayCommand]
        private void InvitePeople()
        {
            // Placeholder for invite functionality
        }
        
        [RelayCommand]
        private void SwitchTab(string tabName)
        {
            ActiveTab = tabName;
        }
    }
}
