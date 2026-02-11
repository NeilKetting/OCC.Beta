using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Enums;
using OCC.Shared.Models;
using OCC.Shared.DTOs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OCC.Client.Features.HseqHub.ViewModels
{
    public partial class IncidentsViewModel : ViewModelBase
    {
        private readonly IHealthSafetyService _hseqService;
        private readonly IToastService _toastService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private bool _isAdding;

        [ObservableProperty]
        private ObservableCollection<IncidentSummaryDto> _incidents = new();

        [ObservableProperty]
        private Incident _newIncident = new(); // Used for both new and editing in the side panel

        [ObservableProperty]
        private IncidentSummaryDto? _selectedSummary;

        [ObservableProperty]
        private ObservableCollection<IncidentPhotoDto> _currentIncidentPhotos = new();



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
                    Incidents = new ObservableCollection<IncidentSummaryDto>(data.OrderByDescending(i => i.Date));
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

        async partial void OnSelectedSummaryChanged(IncidentSummaryDto? value)
        {
            if (value != null)
            {
                try
                {
                    IsBusy = true;
                    BusyText = "Loading details...";
                    var detail = await _hseqService.GetIncidentAsync(value.Id);
                    if (detail != null)
                    {
                        NewIncident = ToEntity(detail);
                        CurrentIncidentPhotos = new ObservableCollection<IncidentPhotoDto>(detail.Photos);
                        IsAdding = true; 
                    }
                }
                catch (Exception ex)
                {
                    _toastService.ShowError("Error", "Failed to load incident details.");
                    System.Diagnostics.Debug.WriteLine(ex);
                }
                finally { IsBusy = false; }
            }
            else
            {
                CurrentIncidentPhotos.Clear();
            }
        }

        [RelayCommand]
        public async Task UploadPhotos(object param)
        {
            if (param == null) return;
            
            // Determine incident ID. If we are editing, use NewIncident (which holds detail).
            // If we are adding new, we MUST save it first (since photos belong to an ID).
            var incident = NewIncident;
            if (incident == null) return;

            // Handle file input (drag drop or picker)
            IEnumerable<IStorageFile>? storageFiles = null;
            
            if (param is IEnumerable<IStorageFile> sFiles)
            {
                storageFiles = sFiles;
            }
            else if (param is IEnumerable<IStorageItem> sItems)
            {
                storageFiles = sItems.OfType<IStorageFile>();
            }
            else if (param is IStorageFile sFile)
            {
                storageFiles = new[] { sFile };
            }

            if (storageFiles == null || !storageFiles.Any()) return;

            IsBusy = true;
            try
            {
                // If it's a NEW incident, we MUST create it first to get an ID for the photos
                if (incident.Id == Guid.Empty)
                {
                    BusyText = "Saving report first...";
                    var created = await _hseqService.CreateIncidentAsync(incident);
                    if (created == null)
                    {
                        _toastService.ShowError("Error", "Could not save incident to allow photo uploads.");
                        return;
                    }
                    incident.Id = created.Id;
                    NewIncident.Id = created.Id; 
                    // Add to list as summary
                    Incidents.Insert(0, new IncidentSummaryDto 
                    { 
                        Id = created.Id, 
                        Date = created.Date,
                        Location = created.Location,
                        Status = created.Status,
                        Severity = created.Severity,
                        Type = created.Type
                    });
                }

                int count = 0;
                foreach (var file in storageFiles)
                {
                    BusyText = $"Uploading {file.Name}...";
                    using var stream = await file.OpenReadAsync();
                    var metadata = new IncidentPhoto
                    {
                        IncidentId = incident.Id,
                        FileName = file.Name
                    };

                    var result = await _hseqService.UploadIncidentPhotoAsync(metadata, stream, file.Name);
                    if (result != null)
                    {
                        count++;
                        CurrentIncidentPhotos.Add(result);
                    }
                }

                if (count > 0) _toastService.ShowSuccess("Success", $"Uploaded {count} photo(s).");
            }
            catch (Exception ex)
            {
                _toastService.ShowError("Error", "Failed to upload photo(s).");
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task DeletePhoto(IncidentPhotoDto photo)
        {
            if (photo == null) return;

            try
            {
                var success = await _hseqService.DeleteIncidentPhotoAsync(photo.Id);
                if (success)
                {
                    CurrentIncidentPhotos.Remove(photo);
                    _toastService.ShowSuccess("Deleted", "Photo removed.");
                }
                else
                {
                    _toastService.ShowError("Error", "Failed to delete photo.");
                }
            }
            catch (Exception ex)
            {
                _toastService.ShowError("Error", "Exception deleting photo.");
                System.Diagnostics.Debug.WriteLine(ex);
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
            SelectedSummary = null;
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
        private async Task DeleteIncident(IncidentSummaryDto summary)
        {
            if (summary == null) return;
            
            try 
            {
                IsBusy = true;
                BusyText = "Deleting...";
                var success = await _hseqService.DeleteIncidentAsync(summary.Id);
                if (success)
                {
                    _toastService.ShowSuccess("Success", "Incident deleted.");
                    Incidents.Remove(summary);
                    if (SelectedSummary?.Id == summary.Id) SelectedSummary = null;
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
        private Incident ToEntity(IncidentDto dto)
        {
            return new Incident
            {
                Id = dto.Id,
                Date = dto.Date,
                Type = dto.Type,
                Severity = dto.Severity,
                Location = dto.Location,
                Description = dto.Description,
                ReportedByUserId = dto.ReportedByUserId,
                Status = dto.Status,
                InvestigatorId = dto.InvestigatorId,
                RootCause = dto.RootCause,
                CorrectiveAction = dto.CorrectiveAction
            };
        }
    }
}
