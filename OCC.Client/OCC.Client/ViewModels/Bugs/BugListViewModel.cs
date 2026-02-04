using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
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
        private List<BugReport> _allBugsCache = new();

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
        private string _sortOption = "Priority"; // Priority, Date

        [ObservableProperty]
        private bool _isImageZoomed;

        public ObservableCollection<string> StatusFilters { get; } = new() 
        { 
            "All", "Open", "Fixed", "Resolved", "Closed", "Waiting for Client" 
        };
        
        public ObservableCollection<string> SortOptions { get; } = new()
        {
            "Priority", "Date"
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
            var email = _authService.CurrentUser?.Email?.ToLowerInvariant();
            IsDev = email == "neil@mdk.co.za";

            LoadBugsCommand.Execute(null);
        }

        #endregion

        #region Methods

        partial void OnSearchTextChanged(string value) => ApplyFilters();
        partial void OnStatusFilterChanged(string value) => ApplyFilters();
        partial void OnSortOptionChanged(string value) => ApplyFilters();

        private void ApplyFilters()
        {
            if (_allBugsCache == null) return;
            
            var filteredList = _allBugsCache.AsEnumerable();

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

            // sort logic
            if (SortOption == "Date")
            {
                filteredList = filteredList.OrderByDescending(x => x.ReportedDate);
            }
            else
            {
                // Priority Sort
                Func<string, int> getPriority = (s) => s switch {
                    "Open" => 0,
                    "Fixed" => 1,
                    "Waiting for Client" => 2,
                    "Resolved" => 3,
                    "Closed" => 4,
                    _ => 5
                };
                filteredList = filteredList
                    .OrderBy(x => getPriority(x.Status))
                    .ThenByDescending(x => x.ReportedDate);
            }

            Bugs = new ObservableCollection<BugReport>(filteredList);
        }

        async partial void OnSelectedBugChanged(BugReport? value)
        {
            IsReporter = value?.ReporterId == _authService.CurrentUser?.Id;
            SelectedBugScreenshot = null; // Clear previous first

            if (value != null)
            {
                // Optimization: If base64 is null, fetch full details seamlessly
                // Or if we just want to ensure we have the latest comments etc.
                // We'll check if we need to fetch. Ideally, the list view items don't have ScreenshotBase64 now.
                
                BugReport fullBug = value;

                // If screenshot is missing but we expect one, or just to get comments freshly
                // The list view didn't include comments either in my API optimization? 
                // Wait, I only removed ScreenshotBase64. Comments were Included. 
                // But let's be safe and fetch fresh details to be sure.
                
                // Only fetch if we suspect missing data or want fresh data without reloading list
                // For now, let's always fetch detail to support the "Light List" architecture
                try 
                {
                    var fresh = await _bugService.GetBugReportAsync(value.Id);
                    if (fresh != null)
                    {
                        // Update the object in the list OR just use it for display?
                        // If we update the object in the list, it might trigger this again if we replace the reference.
                        // Better to just update properties of the reference in the list, or keep a separate "DetailedBug" property.
                        // But existing UI binds to SelectedBug. Let's update its properties.
                        value.Comments = fresh.Comments;
                        value.ScreenshotBase64 = fresh.ScreenshotBase64;
                        value.Status = fresh.Status;
                        // Don't replace 'value' reference, just update content.
                        fullBug = value; // Update reference for local logic
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error fetching bug details: {ex.Message}");
                }

                if (!string.IsNullOrEmpty(fullBug.ScreenshotBase64))
                {
                    try
                    {
                        var bytes = Convert.FromBase64String(fullBug.ScreenshotBase64);
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
                
                _allBugsCache = list;
                ApplyFilters();
                
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

        private async Task RefreshSelectedBug()
        {
            if (SelectedBug == null) return;
            try 
            {
                var fresh = await _bugService.GetBugReportAsync(SelectedBug.Id);
                if (fresh != null)
                {
                    // Update properties that might have changed
                    SelectedBug.Comments = fresh.Comments;
                    SelectedBug.Status = fresh.Status;
                    SelectedBug.ScreenshotBase64 = fresh.ScreenshotBase64; // Restore if needed
                    
                    // Force refresh of binding if needed (PropertyChanged?)
                    // Since SelectedBug is an ObservableObject ideally, these would notify. 
                    // But BugReport is just a POCO BaseEntity.
                    // We might need to manually notify or replace the item in the collection if we want the List row to update status color.
                    
                    var index = _allBugsCache.FindIndex(b => b.Id == fresh.Id);
                    if (index >= 0)
                    {
                         _allBugsCache[index] = fresh; // Update cache
                    }
                    
                    // Update ObservableCollection
                    var obsItem = Bugs.FirstOrDefault(b => b.Id == fresh.Id);
                    if (obsItem != null)
                    {
                         // Manually update properties for UI if binding to object directly
                         obsItem.Status = fresh.Status;
                         obsItem.Comments = fresh.Comments;
                         // Triggers? Collections don't observe property changes of items unless items implement INotifyPropertyChanged
                         // Simple hack: Replace the item in the collection to force UI update
                         var obsIndex = Bugs.IndexOf(obsItem);
                         Bugs[obsIndex] = fresh;
                         SelectedBug = fresh; // Reselect to keep detail view happy
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Refesh error: {ex.Message}");
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
                await RefreshSelectedBug(); // OPTIMIZATION: Only refresh this bug
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
            await RefreshSelectedBug();
        }

        [RelayCommand]
        private async Task MarkFixed()
        {
            if (SelectedBug == null || !IsDev) return;
            var text = "Developer marked this issue as Fixed. Please verify and mark as Resolved.";
            await _bugService.AddCommentAsync(SelectedBug.Id, text, "Fixed");
            await RefreshSelectedBug();
        }

        [RelayCommand]
        private async Task CloseBug()
        {
            if (SelectedBug == null || !IsDev) return;
            var text = "Developer closed the bug.";
            await _bugService.AddCommentAsync(SelectedBug.Id, text, "Closed");
            await RefreshSelectedBug();
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
                 await LoadBugs(); // Deletion still needs full reload to remove from list correctly/safely
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
            await RefreshSelectedBug();
        }

        [RelayCommand]
        private async Task MarkNotResolved()
        {
            if (SelectedBug == null) return;
            var text = $"{_authService.CurrentUser?.FirstName ?? "Reporter"} marked this issue as Still Broken.";
            await _bugService.AddCommentAsync(SelectedBug.Id, text, "Open");
            await RefreshSelectedBug();
        }

        [RelayCommand]
        private void ToggleImageZoom()
        {
            IsImageZoomed = !IsImageZoomed;
        }

        #endregion
    }
}
