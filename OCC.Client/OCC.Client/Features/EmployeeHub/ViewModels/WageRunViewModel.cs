using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.Features.EmployeeHub.ViewModels
{
    public partial class WageRunViewModel : ViewModelBase
    {
        private readonly IWageService _wageService;
        private readonly IDialogService _dialogService;

        public WageRunViewModel(IWageService wageService, IDialogService dialogService)
        {
            _wageService = wageService;
            _dialogService = dialogService;
            
            // Default period: Current Week's Monday
            // If today is Monday, use today. If today is Sunday, go back to previous Monday?
            // "Wage run will be run on a Wednesday... The run will be fortnight so it will start on a monday."
            // If we run on Wednesday of Week 2, the cycle started on Monday of Week 1.
            // So we take the current Monday, and subtract 7 days.
            var today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            StartDate = today.AddDays(-1 * diff).AddDays(-7).Date;
            
            // EndDate is calculated in OnStartDateChanged (StartDate + 13 days)
            
            // EndDate is calculated in OnStartDateChanged
        }

        [ObservableProperty]
        private DateTime _startDate;
        
        partial void OnStartDateChanged(DateTime value)
        {
            // Fortnight run: StartDate + 13 days (Total 14 days)
            EndDate = value.AddDays(13);
        }
        
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
        


        private Guid? _currentDraftId;

        [RelayCommand]
        private async Task GenerateDraft()
        {
            try
            {
                BusyText = "Generating draft run...";
                IsBusy = true;
                
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
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task FinalizeRun()
        {
            if (_currentDraftId == null) return;
            
            var confirm = await _dialogService.ShowConfirmationAsync("Finalize Wage Run", 
                "Are you sure you want to finalize this run? \n\nThis will lock the attendance records and variances for future runs.");
            
            if (!confirm) return;

            try
            {
                BusyText = "Finalizing run...";
                IsBusy = true;
                
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
                IsBusy = false;
            }
        }
    }
}
