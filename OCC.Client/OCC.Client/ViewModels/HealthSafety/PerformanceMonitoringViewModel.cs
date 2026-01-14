using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.HealthSafety
{
    public partial class PerformanceMonitoringViewModel : ViewModelBase
    {
        [ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<OCC.Shared.Models.HseqSafeHourRecord> _safeHours;

        public PerformanceMonitoringViewModel()
        {
            _safeHours = new System.Collections.ObjectModel.ObservableCollection<OCC.Shared.Models.HseqSafeHourRecord>
            {
                new OCC.Shared.Models.HseqSafeHourRecord 
                { 
                    Month = new System.DateTime(2025, 1, 1), 
                    SafeWorkHours = 1200, 
                    IncidentReported = "No", 
                    NearMisses = 0, 
                    Status = "Closed",
                    ReportedBy = "John Doe"
                },
                new OCC.Shared.Models.HseqSafeHourRecord 
                { 
                    Month = new System.DateTime(2025, 2, 1), 
                    SafeWorkHours = 1150, 
                    IncidentReported = "Yes", 
                    NearMisses = 2, 
                    RootCause = "Slippery floor",
                    CorrectiveActions = "Installed anti-slip mats",
                    Status = "Open",
                    ReportedBy = "Jane Smith"
                }
            };
        }
    }
}
