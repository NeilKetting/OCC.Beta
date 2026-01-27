using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Customers
{
    public partial class CustomerManagementViewModel : ViewModelBase, IRecipient<ViewModels.Messages.EntityUpdatedMessage>
    {
        #region Private Members

        private readonly IRepository<Customer> _customerRepository;
        private readonly Services.Interfaces.IDialogService _dialogService;
        private readonly IServiceProvider _serviceProvider;
        private List<Customer> _allCustomers = new();

        #endregion

        #region Observables

        [ObservableProperty]
        private string _activeTab = "Customers";

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Customer> _customers = new();

        [ObservableProperty]
        private bool _isAddCustomerPopupVisible;

        [ObservableProperty]
        private CustomerDetailViewModel? _customerDetailPopup;

        [ObservableProperty]
        private Customer? _selectedCustomer;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _busyText = string.Empty;

        #endregion

        #region Constructors

        public CustomerManagementViewModel()
        {
            // Designer constructor
            _customerRepository = null!;
            _dialogService = null!;
            _serviceProvider = null!;
        }

        public CustomerManagementViewModel(
            IRepository<Customer> customerRepository,
            Services.Interfaces.IDialogService dialogService,
            IServiceProvider serviceProvider)
        {
            _customerRepository = customerRepository;
            _dialogService = dialogService;
            _serviceProvider = serviceProvider;

            LoadData();

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        public void Receive(ViewModels.Messages.EntityUpdatedMessage message)
        {
            if (message.Value.EntityType == "Customer")
            {
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(LoadData);
            }
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void AddCustomer()
        {
            OpenDetailPopup(null);
        }

        [RelayCommand]
        public void EditCustomer(Customer customer)
        {
            if (customer == null) return;
            OpenDetailPopup(customer);
        }

        [RelayCommand]
        public async Task DeleteCustomer(Customer customer)
        {
            if (customer == null) return;

            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Delete Customer", 
                $"Are you sure you want to delete '{customer.Name}'? This may affect linked projects.");

            if (confirmed)
            {
                try
                {
                    BusyText = $"Deleting {customer.Name}...";
                    IsBusy = true;
                    await _customerRepository.DeleteAsync(customer.Id);
                    LoadData();
                }
                catch (Exception ex)
                {
                   await _dialogService.ShowAlertAsync("Error", $"Failed to delete customer: {ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        [RelayCommand]
        private void CloseDetail()
        {
            IsAddCustomerPopupVisible = false;
            CustomerDetailPopup = null;
        }

        #endregion

        #region Methods

        private void OpenDetailPopup(Customer? customer)
        {
            var vm = new CustomerDetailViewModel(_customerRepository, _dialogService);
            if (customer != null)
            {
                vm.Load(customer);
            }
            else
            {
                vm.InitializeNew();
            }

            vm.CloseRequested += (s, e) => CloseDetail();
            vm.Saved += (s, e) => 
            {
                CloseDetail();
                LoadData();
            };

            CustomerDetailPopup = vm;
            IsAddCustomerPopupVisible = true;
        }

        public async void LoadData()
        {
            try
            {
                BusyText = "Loading customers...";
                IsBusy = true;

                var customers = await _customerRepository.GetAllAsync();
                _allCustomers = customers.OrderBy(c => c.Name).ToList();

                FilterCustomers();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSearchQueryChanged(string value)
        {
            FilterCustomers();
        }

        private void FilterCustomers()
        {
            if (_allCustomers == null) return;

            var filtered = _allCustomers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                filtered = filtered.Where(c => 
                    (c.Name?.ToLower().Contains(query) ?? false) ||
                    (c.Email?.ToLower().Contains(query) ?? false) ||
                    (c.Address?.ToLower().Contains(query) ?? false)
                );
            }

            var resultList = filtered.ToList();
            Customers = new ObservableCollection<Customer>(resultList);
            TotalCount = resultList.Count;
        }

        #endregion
    }
}
