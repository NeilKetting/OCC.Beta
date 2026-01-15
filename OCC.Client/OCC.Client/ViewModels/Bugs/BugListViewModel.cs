using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.ViewModels.Core; // Added
using OCC.Shared.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace OCC.Client.ViewModels.Bugs
{
    public partial class BugListViewModel : ViewModelBase
    {
        private readonly IBugReportService _bugService;
        private readonly IAuthService _authService;
        private readonly IDialogService _dialogService;
        private readonly IPermissionService _permissionService;
        
        [ObservableProperty]
        private ObservableCollection<BugReport> _bugs = new();

        [ObservableProperty]
        private bool _isBusy;

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

        public BugListViewModel(IBugReportService bugService, IAuthService authService, IDialogService dialogService, IPermissionService permissionService)
        {
            _bugService = bugService;
            _authService = authService;
            _dialogService = dialogService;
            _permissionService = permissionService;
            
            IsDev = _permissionService.IsDev;
            LoadBugsCommand.Execute(null);
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
            IsBusy = true;
            try
            {
                var list = await _bugService.GetBugReportsAsync();
                Bugs = new ObservableCollection<BugReport>(list.OrderByDescending(x => x.ReportedDate));
                if (SelectedBug != null)
                {
                    SelectedBug = Bugs.FirstOrDefault(x => x.Id == SelectedBug.Id) ?? Bugs.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading bugs: {ex.Message}");
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
                if (!IsDev)
                {
                    await _dialogService.ShowAlertAsync("Access Denied", "Only the Developer can comment on bug reports.");
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
            var text = "Developer marked this issue as Resolved.";
            await _bugService.AddCommentAsync(SelectedBug.Id, text, "Resolved");
            await LoadBugs();
        }

        [RelayCommand]
        private async Task CloseBug()
        {
            if (SelectedBug == null || (!IsDev && !IsReporter)) return;
            var text = IsReporter ? "Reporter confirmed fix and closed the bug." : "Developer closed the bug.";
            await _bugService.AddCommentAsync(SelectedBug.Id, text, "Closed");
            await LoadBugs();
        }
    }
}
