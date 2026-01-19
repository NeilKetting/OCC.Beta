using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace OCC.Client.ViewModels.Bugs
{
    public partial class BugListViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IBugReportService _bugService;
        private readonly IAuthService _authService;
        private readonly IDialogService _dialogService;
        private readonly IPermissionService _permissionService;
        private readonly Microsoft.Extensions.Logging.ILogger<BugListViewModel> _logger;

        #endregion

        #region Observables

        [ObservableProperty]
        private ObservableCollection<BugReport> _bugs = new();



        [ObservableProperty]
        private BugReport? _selectedBug;

        [ObservableProperty]
        private string _newCommentText = string.Empty;

        [ObservableProperty]
        private bool _isDev;

        [ObservableProperty]
        private bool _isReporter;

        [ObservableProperty]
        private Avalonia.Media.Imaging.Bitmap? _selectedBugScreenshot;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _statusFilter = "All"; // All, Open, Fixed, Resolved, Closed, Waiting for Client

        [ObservableProperty]
        private bool _isImageZoomed;

        public ObservableCollection<string> StatusFilters { get; } = new() 
        { 
            "All", "Open", "Fixed", "Resolved", "Closed", "Waiting for Client" 
        };

        #endregion

        #region Constructor

        public BugListViewModel(IBugReportService bugService, IAuthService authService, IDialogService dialogService, IPermissionService permissionService, Microsoft.Extensions.Logging.ILogger<BugListViewModel> logger)
        {
            _bugService = bugService;
            _authService = authService;
            _dialogService = dialogService;
            _permissionService = permissionService;
            _logger = logger;
            
            // Permissions: "Dev" is specifically Neil for commenting purposes now
            // We use the email check directly here or rely on PermissionService if updated.
            // Requirement: "Me (neil@mdk.co.za) is the only person who can reply"
            var email = _authService.CurrentUser?.Email?.ToLowerInvariant();
            IsDev = email == "neil@mdk.co.za";

            LoadBugsCommand.Execute(null);
        }

        #endregion

        #region Methods

        partial void OnSearchTextChanged(string value) => ApplyFilters();
        partial void OnStatusFilterChanged(string value) => ApplyFilters();

        private void ApplyFilters()
        {
            if (Bugs == null) return;
            
            // We'll reload from service to get fresh data or just filter locally if we have all data.
            // Requirement says "Refresh" shouldn't close the view, so let's filter locally if possible
            // but LoadBugs already fetches from service. Let's make LoadBugs smarter.
            _ = LoadBugs();
        }

        partial void OnSelectedBugChanged(BugReport? value)
        {
            IsReporter = value?.ReporterId == _authService.CurrentUser?.Id;
            
            if (!string.IsNullOrEmpty(value?.ScreenshotBase64))
            {
                try
                {
                    var bytes = Convert.FromBase64String(value.ScreenshotBase64);
                    using (var ms = new System.IO.MemoryStream(bytes))
                    {
                        SelectedBugScreenshot = new Avalonia.Media.Imaging.Bitmap(ms);
                    }
                }
                catch
                {
                    SelectedBugScreenshot = null;
                }
            }
            else
            {
                SelectedBugScreenshot = null;
            }
        }

        [RelayCommand]
        private async Task LoadBugs()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var previousId = SelectedBug?.Id;
                var list = await _bugService.GetBugReportsAsync();
                
                // Sort Priority
                Func<string, int> getPriority = (s) => s switch {
                    "Open" => 0,
                    "Fixed" => 1,
                    "Waiting for Client" => 2,
                    "Resolved" => 3,
                    "Closed" => 4,
                    _ => 5
                };

                var filteredList = list.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filteredList = filteredList.Where(x => 
                        x.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || 
                        x.ViewName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        x.ReporterName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }

                if (StatusFilter != "All")
                {
                    filteredList = filteredList.Where(x => x.Status == StatusFilter);
                }

                var sortedList = filteredList
                    .OrderBy(x => getPriority(x.Status))
                    .ThenByDescending(x => x.ReportedDate);

                Bugs = new ObservableCollection<BugReport>(sortedList);
                
                if (previousId.HasValue)
                {
                    SelectedBug = Bugs.FirstOrDefault(x => x.Id == previousId.Value);
                }
                
                if (SelectedBug == null && Bugs.Any() && !previousId.HasValue)
                {
                    SelectedBug = Bugs.First();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bugs");
                await _dialogService.ShowAlertAsync("Error", $"Failed to load bug reports: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SendComment()
        {
            if (SelectedBug == null || string.IsNullOrWhiteSpace(NewCommentText)) return;

            try
            {
                if (!IsDev && !IsReporter)
                {
                    await _dialogService.ShowAlertAsync("Access Denied", "Only the Developer (Neil) or the Reporter can comment on this bug.");
                    return;
                }

                await _bugService.AddCommentAsync(SelectedBug.Id, NewCommentText, null);
                NewCommentText = string.Empty;
                await LoadBugs();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"Failed to send comment: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task RequestInfo()
        {
            if (SelectedBug == null || !IsDev) return;
            var text = "Developer requested more information.";
            await _bugService.AddCommentAsync(SelectedBug.Id, text, "Waiting for Client");
            await LoadBugs();
        }

        [RelayCommand]
        private async Task MarkFixed()
        {
            if (SelectedBug == null || !IsDev) return;
            var text = "Developer marked this issue as Fixed. Please verify and mark as Resolved.";
            await _bugService.AddCommentAsync(SelectedBug.Id, text, "Fixed");
            await LoadBugs();
        }

        [RelayCommand]
        private async Task CloseBug()
        {
            // Only Dev (Neil) can close/reply as per strict instructions
            if (SelectedBug == null || !IsDev) return;
            
            var text = "Developer closed the bug.";
            await _bugService.AddCommentAsync(SelectedBug.Id, text, "Closed");
            await LoadBugs();
        }

        [RelayCommand]
        private async Task DeleteBug()
        {
             if (SelectedBug == null || !IsDev) return;
             
             var result = await _dialogService.ShowConfirmationAsync("Delete Bug Report", "Are you sure you want to permanently delete this bug report?");
             if (!result) return;
             
             try
             {
                 await _bugService.DeleteBugAsync(SelectedBug.Id);
                 await LoadBugs();
                 SelectedBug = null;
             }
             catch(Exception ex)
             {
                 await _dialogService.ShowAlertAsync("Error", $"Delete failed: {ex.Message}");
             }
        }

        [RelayCommand]
        private async Task MarkResolved()
        {
            if (SelectedBug == null) return;
            var text = $"{_authService.CurrentUser?.FirstName ?? "Reporter"} marked this issue as Resolved.";
            await _bugService.AddCommentAsync(SelectedBug.Id, text, "Resolved");
            await LoadBugs();
        }

        [RelayCommand]
        private async Task MarkNotResolved()
        {
            if (SelectedBug == null) return;
            var text = $"{_authService.CurrentUser?.FirstName ?? "Reporter"} marked this issue as Still Broken.";
            await _bugService.AddCommentAsync(SelectedBug.Id, text, "Open");
            await LoadBugs();
        }

        [RelayCommand]
        private void ToggleImageZoom()
        {
            IsImageZoomed = !IsImageZoomed;
        }

        #endregion
    }
}
