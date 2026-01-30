using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using OCC.Client.ModelWrappers;
using OCC.Client.Services.Repositories.Interfaces;
using System.Collections.Generic;

namespace OCC.Client.ViewModels.HealthSafety
{
    public partial class AuditsViewModel : ViewModelBase
    {
        private readonly IHealthSafetyService _hseqService;
        private readonly IToastService _toastService;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IExportService _exportService;

        [ObservableProperty]
        private ObservableCollection<HseqAudit> _audits = new();

        [ObservableProperty]
        private HseqAudit? _selectedAudit;

        [ObservableProperty]
        private ObservableCollection<HseqAuditNonComplianceItemWrapper> _wrappedDeviations = new();

        [ObservableProperty]
        private ObservableCollection<Employee> _siteManagers = new();

        [ObservableProperty]
        private bool _isDeviationsOpen;

        [ObservableProperty]
        private ObservableCollection<HseqAuditAttachment> _auditAttachments = new();

        public AuditsViewModel(IHealthSafetyService hseqService, IToastService toastService, IRepository<Employee> employeeRepository, IExportService exportService)
        {
            _hseqService = hseqService;
            _toastService = toastService;
            _employeeRepository = employeeRepository;
            _exportService = exportService;
        }

        // Design-time constructor
        public AuditsViewModel()
        {
            _hseqService = null!;
            _toastService = null!;
            _employeeRepository = null!;
            _exportService = null!;
        }

        [RelayCommand]
        public async Task LoadAudits()
        {
            if (_hseqService == null) return;

            try
            {
                IsBusy = true;
                BusyText = "Loading audits...";
                
                var data = await _hseqService.GetAuditsAsync();
                if (data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[AuditsViewModel] Loaded {data.Count()} audits.");
                    foreach(var a in data) 
                    {
                        System.Diagnostics.Debug.WriteLine($" - Audit {a.AuditNumber}: {a.SiteName}, Score={a.ActualScore}, Status={a.Status}");
                    }
                    Audits = new ObservableCollection<HseqAudit>(data.OrderByDescending(a => a.Date));
                }
                else
                {
                     System.Diagnostics.Debug.WriteLine("[AuditsViewModel] LoadAudits returned NULL");
                }
            }
            catch (Exception ex)
            {
                _toastService?.ShowError("Error", "Failed to load audits.");
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ViewDeviations(HseqAudit audit)
        {
            if (audit == null) return;
            SelectedAudit = audit;

            try
            {
                IsBusy = true;
                BusyText = "Loading deviations...";
                
                // Load Site Managers for the dropdown
                if (!SiteManagers.Any())
                {
                    var allEmployees = await _employeeRepository.GetAllAsync();
                    if (allEmployees != null)
                    {
                        var managers = allEmployees.Where(e => e.Role == OCC.Shared.Models.EmployeeRole.SiteManager).ToList();
                        SiteManagers = new ObservableCollection<Employee>(managers);
                    }
                }

                // Fetch deviations for this audit
                var items = await _hseqService.GetAuditDeviationsAsync(audit.Id);
                
                WrappedDeviations.Clear();
                if (items != null)
                {
                    foreach (var item in items) 
                    {
                        WrappedDeviations.Add(new HseqAuditNonComplianceItemWrapper(item));
                    }
                }

                IsDeviationsOpen = true;
                IsEditorOpen = false; // Close main editor to show deviations sheet
            }
            catch (Exception ex)
            {
                _toastService?.ShowError("Error", "Failed to load deviations.");
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CloseDeviations()
        {
            IsDeviationsOpen = false;
            SelectedAudit = null;
            WrappedDeviations.Clear();
        }

        [ObservableProperty]
        private bool _isEditorOpen;

        [ObservableProperty]
        private HseqAudit _currentAudit = new();

        [ObservableProperty]
        private string _editorTitle = "New Audit";

        [RelayCommand]
        public void CreateNewAudit()
        {
            var newAudit = new HseqAudit
            {
                Date = DateTime.Today,
                Status = OCC.Shared.Enums.AuditStatus.InProgress,
                TargetScore = 100
            };
            
            // Seed default sections
            var categories = new[]
            {
                "Administrative Requirements", "Education Training & Promotion", "Public Safety",
                "Personal Protective Equipment (PPE)", "Housekeeping", "Elevated Work", "Electricity",
                "Fire Prevention and Protection", "Equipment", "Construction Vehicles and Mobile Plant"
            };

            newAudit.Sections = new System.Collections.Generic.List<HseqAuditSection>();
            foreach (var cat in categories)
            {
                newAudit.Sections.Add(new HseqAuditSection 
                { 
                    Name = cat, 
                    PossibleScore = 100, 
                    ActualScore = 0 
                });
            }

            CurrentAudit = newAudit;
            EditorTitle = "New Audit";
            IsEditorOpen = true;
        }

        [RelayCommand]
        public async Task EditAudit(HseqAudit audit)
        {
            if (audit == null) return;
            
            IsBusy = true;
            try
            {
                var fullAudit = await _hseqService.GetAuditAsync(audit.Id);
                var loadedAudit = fullAudit ?? audit;

                // Ensure sections exist (if legacy data)
                if (loadedAudit.Sections == null || !loadedAudit.Sections.Any())
                {
                    var categories = new[]
                    {
                        "Administrative Requirements", "Education Training & Promotion", "Public Safety",
                        "Personal Protective Equipment (PPE)", "Housekeeping", "Elevated Work", "Electricity",
                        "Fire Prevention and Protection", "Equipment", "Construction Vehicles and Mobile Plant"
                    };
                    loadedAudit.Sections = new System.Collections.Generic.List<HseqAuditSection>();
                    foreach (var cat in categories) 
                    {
                        loadedAudit.Sections.Add(new HseqAuditSection { Name = cat, PossibleScore = 100, ActualScore = 0 });
                    }
                }

                CurrentAudit = loadedAudit;
                AuditAttachments = new ObservableCollection<HseqAuditAttachment>(loadedAudit.Attachments ?? new List<HseqAuditAttachment>());
                EditorTitle = "Edit Audit Score";
                IsEditorOpen = true;
                IsDeviationsOpen = false; // Hide deviations if editor is opened
            }
            catch(Exception ex)
            {
                _toastService.ShowError("Error", "Failed to load audit details.");
            }
            finally 
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void CloseEditor()
        {
            IsEditorOpen = false;
        }

        [RelayCommand]
        public async Task SaveAudit()
        {
            // Calculate Total Score
            if (CurrentAudit.Sections.Any(s => s.PossibleScore > 0))
            {
                decimal totalActual = CurrentAudit.Sections.Sum(s => s.ActualScore);
                decimal totalPossible = CurrentAudit.Sections.Sum(s => s.PossibleScore);
                
                // If storing percentage in ActualScore property layout:
                if (totalPossible > 0)
                {
                    CurrentAudit.ActualScore = (totalActual / totalPossible) * 100m;
                    CurrentAudit.ActualScore = Math.Round(CurrentAudit.ActualScore, 2);
                }
            }
            
            // Usually we might want to calculate average of percentages if weights are equal? 
            // "Efficiency Rating" usually implies % of compliance.
            // (Sum(Actual) / Sum(Possible)) * 100 is standard weighted average.
            // However, chart shows "100%", "93%".
            
            IsBusy = true;
            try
            {
                // Determine if New or Update
                bool isNew = CurrentAudit.Id == Guid.Empty || !Audits.Any(a => a.Id == CurrentAudit.Id);
                // Wait, Id is initialized to NewGuid. We need to check existence in DB or list.
                // Better approach: check if it exists in _audits list OR rely on ID.
                // Usually CreateNewAudit creates a fresh ID.
                // We should likely try Update first, if Not Found then Create? Or use a flag.
                // I'll check if it's in the list.
                
                var existing = Audits.FirstOrDefault(a => a.Id == CurrentAudit.Id);
                if (existing != null)
                {
                     // Update
                     var success = await _hseqService.UpdateAuditAsync(CurrentAudit);
                     if (success)
                     {
                         _toastService.ShowSuccess("Saved", "Audit score updated.");
                         var index = Audits.IndexOf(existing);
                         if (index >= 0) Audits[index] = CurrentAudit;
                     }
                     else
                     {
                         _toastService.ShowError("Error", "Failed to update audit.");
                     }
                }
                else
                {
                    // Create
                    var created = await _hseqService.CreateAuditAsync(CurrentAudit);
                    if (created != null)
                    {
                        Audits.Insert(0, created);
                        _toastService.ShowSuccess("Created", "New audit created.");
                    }
                    else
                    {
                        _toastService.ShowError("Error", "Failed to create audit.");
                    }
                }
                
                IsEditorOpen = false;
            }
            catch (Exception ex)
            {
                _toastService.ShowError("Error", $"Failed to save: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void AddDeviation()
        {
            if (SelectedAudit == null) return;
            
            var newItem = new HseqAuditNonComplianceItem
            {
                Id = Guid.NewGuid(),
                AuditId = SelectedAudit.Id,
                Status = OCC.Shared.Enums.AuditItemStatus.Open,
                Description = "",
                RegulationReference = ""
            };
            
            var wrapper = new HseqAuditNonComplianceItemWrapper(newItem);
            WrappedDeviations.Insert(0, wrapper);
            
            // Sync with parent object
            if (SelectedAudit.NonComplianceItems == null) 
                SelectedAudit.NonComplianceItems = new System.Collections.Generic.List<HseqAuditNonComplianceItem>();
                
            SelectedAudit.NonComplianceItems.Add(newItem);
        }

        [RelayCommand]
        private async Task SaveDeviation(HseqAuditNonComplianceItemWrapper wrapper)
        {
            if (SelectedAudit == null || wrapper == null) return;
            
            IsBusy = true;
            try
            {
                wrapper.CommitToModel();
                var item = wrapper.Model;

                // Ensure synchronisation
                if (SelectedAudit.NonComplianceItems == null) 
                    SelectedAudit.NonComplianceItems = new System.Collections.Generic.List<HseqAuditNonComplianceItem>();

                var existing = SelectedAudit.NonComplianceItems.FirstOrDefault(i => i.Id == item.Id);
                if (existing == null && !SelectedAudit.NonComplianceItems.Contains(item))
                {
                    SelectedAudit.NonComplianceItems.Add(item);
                }
                
                // Save via parent Audit
                var success = await _hseqService.UpdateAuditAsync(SelectedAudit);
                if (success)
                {
                    _toastService.ShowSuccess("Saved", "Deviation action saved.");
                }
                else
                {
                    _toastService.ShowError("Error", "Failed to save deviation.");
                }
            }
            catch (Exception ex)
            {
                _toastService.ShowError("Error", "Failed to save.");
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task GenerateCloseOutReport()
        {
            if (SelectedAudit == null) return;

            IsBusy = true;
            BusyText = "Generating report...";
            try
            {
                // Ensure all items are committed
                foreach (var w in WrappedDeviations) w.CommitToModel();

                var filePath = await _exportService.GenerateAuditDeviationReportAsync(SelectedAudit, WrappedDeviations.Select(w => w.Model));
                if (!string.IsNullOrEmpty(filePath))
                {
                    await _exportService.OpenFileAsync(filePath);
                }
            }
            catch (Exception ex)
            {
                _toastService.ShowError("Error", "Failed to generate report.");
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task UploadFiles(object param)
        {
            if (CurrentAudit == null || param == null) return;

            object? files = null;
            HseqAuditNonComplianceItem? targetItem = null;

            if (param is IDataObject data)
            {
                files = data.GetFiles();
            }
            else if (param is IEnumerable<IStorageFile> || param is IStorageFile)
            {
                files = param;
            }
            else if (param is Tuple<object, HseqAuditNonComplianceItem> tuple)
            {
                files = tuple.Item1;
                targetItem = tuple.Item2;
            }

            if (files == null) return;

            IEnumerable<Avalonia.Platform.Storage.IStorageFile>? storageFiles = null;
            if (files is IEnumerable<Avalonia.Platform.Storage.IStorageFile> sFiles) storageFiles = sFiles;
            else if (files is Avalonia.Platform.Storage.IStorageFile sFile) storageFiles = new[] { sFile };

            if (storageFiles == null) return;

            IsBusy = true;
            try
            {
                foreach (var file in storageFiles)
                {
                    using var stream = await file.OpenReadAsync();
                    var metadata = new HseqAuditAttachment
                    {
                        AuditId = CurrentAudit.Id,
                        NonComplianceItemId = targetItem?.Id,
                        FileName = file.Name,
                        UploadedBy = "CurrentUser"
                    };

                    var result = await _hseqService.UploadAuditAttachmentAsync(metadata, stream, file.Name);
                    if (result != null)
                    {
                        if (targetItem != null)
                        {
                            if (targetItem.Attachments == null) targetItem.Attachments = new List<HseqAuditAttachment>();
                            targetItem.Attachments.Add(result);
                            
                            // To update UI, we might need to find the wrapper and notify.
                            // But for now, let's just add to the overall list too if it's the current audit.
                            AuditAttachments.Add(result);
                        }
                        else
                        {
                            AuditAttachments.Add(result);
                            if (CurrentAudit.Attachments == null) CurrentAudit.Attachments = new List<HseqAuditAttachment>();
                            CurrentAudit.Attachments.Add(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _toastService.ShowError("Error", "Failed to upload file(s).");
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task DeleteAttachment(HseqAuditAttachment attachment)
        {
            if (attachment == null) return;

            var result = await _hseqService.DeleteAuditAttachmentAsync(attachment.Id);
            if (result)
            {
                AuditAttachments.Remove(attachment);
                CurrentAudit?.Attachments?.Remove(attachment);
                _toastService.ShowSuccess("Deleted", "Attachment removed.");
            }
            else
            {
                _toastService.ShowError("Error", "Failed to delete attachment.");
            }
        }
    }
}
