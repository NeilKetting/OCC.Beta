using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Time
{
    public partial class WageRunViewModel : ViewModelBase
    {
        private readonly IWageService _wageService;
        private readonly IDialogService _dialogService;

        public WageRunViewModel(IWageService wageService, IDialogService dialogService)
        {
            _wageService = wageService;
            _dialogService = dialogService;
            
            // Default period: Current Fortnight? 
            // Logic to find "Next Pay Cycle" would be cool, but for now default to "This Week + Next Week" or something.
            // Or just default StartDate to Today.
            StartDate = DateTime.Today; // Likely should be a Monday
            EndDate = DateTime.Today.AddDays(13); 
        }

        [ObservableProperty]
        private DateTime _startDate;
        
        [ObservableProperty]
        private DateTime _endDate;
        
        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private ObservableCollection<WageRunLineViewModel> _lines = new();

        [ObservableProperty]
        private decimal _grandTotalWage;

        [ObservableProperty]
        private bool _isGenerated;
        
        [ObservableProperty]
        private bool _isLoading;

        private Guid? _currentDraftId;

        [RelayCommand]
        private async Task GenerateDraft()
        {
            IsLoading = true;
            try
            {
                var draft = await _wageService.GenerateDraftRunAsync(StartDate, EndDate, Notes);
                _currentDraftId = draft.Id;
                
                Lines.Clear();
                foreach (var line in draft.Lines.OrderBy(l => l.EmployeeName))
                {
                    Lines.Add(new WageRunLineViewModel(line));
                }
                
                GrandTotalWage = Lines.Sum(x => x.TotalWage);
                IsGenerated = true;
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"Failed to generate draft: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task FinalizeRun()
        {
            if (_currentDraftId == null) return;
            
            var confirm = await _dialogService.ShowConfirmationAsync("Finalize Wage Run", 
                "Are you sure you want to finalize this run? \n\nThis will lock the attendance records and variances for future runs.");
            
            if (!confirm) return;

            IsLoading = true;
            try
            {
                await _wageService.FinalizeRunAsync(_currentDraftId.Value);
                await _dialogService.ShowAlertAsync("Success", "Wage Run Finalized Successfully.");
                
                // Clear or Navigate away?
                Lines.Clear();
                IsGenerated = false;
                _currentDraftId = null;
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"Failed to finalize: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
