using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.ViewModels.Core;
using System;
using System.Collections.Generic;

namespace OCC.Client.ViewModels.Projects.Dashboard
{
    public partial class ProjectDashboardItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private int _progress;

        [ObservableProperty]
        private string _projectManagerInitials = string.Empty;

        [ObservableProperty]
        private List<string> _members = new();

        [ObservableProperty]
        private string _status = string.Empty;

        [ObservableProperty]
        private DateTime? _latestFinish;

        public string LatestFinishDisplay => LatestFinish?.ToString("dd MMM yyyy") ?? "";
    }
}
