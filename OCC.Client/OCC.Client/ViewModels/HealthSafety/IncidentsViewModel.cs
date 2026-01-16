using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Enums;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.HealthSafety
{
    public partial class IncidentsViewModel : ViewModelBase
    {
        private readonly IHealthSafetyService _hseqService;
        private readonly IToastService _toastService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private ObservableCollection<Incident> _incidents = new();

        [ObservableProperty]
        private Incident _newIncident = new();

        [ObservableProperty]
        private bool _isAdding;



        // Enums for ComboBoxes
        public IncidentType[] IncidentTypes => Enum.GetValues<IncidentType>();
        public IncidentSeverity[] IncidentSeverities => Enum.GetValues<IncidentSeverity>();
        public IncidentStatus[] IncidentStatuses => Enum.GetValues<IncidentStatus>();

        public IncidentsViewModel(IHealthSafetyService hseqService, IToastService toastService, IAuthService authService)
        {
            _hseqService = hseqService;
            _toastService = toastService;
            _authService = authService;
            
            // Initialize with default date
            NewIncident.Date = DateTime.Now;
        }

        [RelayCommand]
        public async Task LoadIncidents()
        {
            try
            {
                IsBusy = true;
                BusyText = "Loading incidents...";
                var data = await _hseqService.GetIncidentsAsync();
                if (data != null)
                {
                    Incidents = new ObservableCollection<Incident>(data.OrderByDescending(i => i.Date));
                }
            }
            catch (Exception ex)
            {
                _toastService.ShowError("Error", "Failed to load incidents.");
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ToggleAdd()
        {
            IsAdding = !IsAdding;
            if (IsAdding)
            {
                NewIncident = new Incident { Date = DateTime.Now }; // Reset form
            }
        }

        [RelayCommand]
        private void CancelAdd()
        {
            IsAdding = false;
            NewIncident = new Incident { Date = DateTime.Now };
        }

        [RelayCommand]
        private async Task SaveIncident()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(NewIncident.Description) || string.IsNullOrWhiteSpace(NewIncident.Location))
            {
                _toastService.ShowWarning("Validation", "Description and Location are required.");
                return;
            }

            try
            {
                IsBusy = true;
                BusyText = "Saving incident...";
                
                // Auto-fill reported by if available
                if (_authService.CurrentUser != null)
                {
                    NewIncident.ReportedByUserId = _authService.CurrentUser.Id.ToString();
                }

                var created = await _hseqService.CreateIncidentAsync(NewIncident);
                if (created != null)
                {
                    _toastService.ShowSuccess("Success", "Incident reported successfully.");
                    IsAdding = false;
                    await LoadIncidents(); // Refresh list
                }
                else
                {
                    _toastService.ShowError("Error", "Failed to report incident.");
                }
            }
            catch (Exception ex)
            {
                _toastService.ShowError("Error", "An error occurred while saving.");
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteIncident(Incident incident)
        {
            if (incident == null) return;
            
            try 
            {
                IsBusy = true;
                BusyText = "Deleting...";
                var success = await _hseqService.DeleteIncidentAsync(incident.Id);
                if (success)
                {
                    _toastService.ShowSuccess("Success", "Incident deleted.");
                    Incidents.Remove(incident);
                }
                else
                {
                    _toastService.ShowError("Error", "Failed to delete incident.");
                }
            }
            catch (Exception ex)
            {
                 _toastService.ShowError("Error", "Exception deleting incident.");
                 System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
