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

namespace OCC.Client.ViewModels.HealthSafety
{
    public partial class AuditsViewModel : ViewModelBase
    {
        private readonly IHealthSafetyService _hseqService;
        private readonly IToastService _toastService;

        [ObservableProperty]
        private ObservableCollection<HseqAudit> _audits = new();

        [ObservableProperty]
        private HseqAudit? _selectedAudit;

        [ObservableProperty]
        private ObservableCollection<HseqAuditNonComplianceItem> _deviations = new();

        [ObservableProperty]
        private bool _isDeviationsOpen;



        public AuditsViewModel(IHealthSafetyService hseqService, IToastService toastService)
        {
            _hseqService = hseqService;
            _toastService = toastService;
        }

        // Design-time constructor
        public AuditsViewModel()
        {
            _hseqService = null!;
            _toastService = null!;
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
                
                // Fetch deviations for this audit
                var items = await _hseqService.GetAuditDeviationsAsync(audit.Id);
                
                Deviations.Clear();
                if (items != null)
                {
                    foreach (var item in items) Deviations.Add(item);
                }

                IsDeviationsOpen = true;
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
            Deviations.Clear();
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
                EditorTitle = "Edit Audit Score";
                IsEditorOpen = true;
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
                bool isNew = CurrentAudit.Id == Guid.Empty || !_audits.Any(a => a.Id == CurrentAudit.Id);
                // Wait, Id is initialized to NewGuid. We need to check existence in DB or list.
                // Better approach: check if it exists in _audits list OR rely on ID.
                // Usually CreateNewAudit creates a fresh ID.
                // We should likely try Update first, if Not Found then Create? Or use a flag.
                // I'll check if it's in the list.
                
                var existing = _audits.FirstOrDefault(a => a.Id == CurrentAudit.Id);
                if (existing != null)
                {
                     // Update
                     var success = await _hseqService.UpdateAuditAsync(CurrentAudit);
                     if (success)
                     {
                         _toastService.ShowSuccess("Saved", "Audit score updated.");
                         var index = _audits.IndexOf(existing);
                         if (index >= 0) _audits[index] = CurrentAudit;
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
                        _audits.Insert(0, created);
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
                AuditId = SelectedAudit.Id,
                Status = OCC.Shared.Enums.AuditItemStatus.Open,
                Description = "",
                RegulationReference = ""
            };
            
            Deviations.Insert(0, newItem);
            
            // Sync with parent object
            if (SelectedAudit.NonComplianceItems == null) 
                SelectedAudit.NonComplianceItems = new System.Collections.Generic.List<HseqAuditNonComplianceItem>();
                
            SelectedAudit.NonComplianceItems.Add(newItem);
        }

        [RelayCommand]
        private async Task SaveDeviation(HseqAuditNonComplianceItem item)
        {
            if (SelectedAudit == null || item == null) return;
            
            IsBusy = true;
            try
            {
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
    }
}
