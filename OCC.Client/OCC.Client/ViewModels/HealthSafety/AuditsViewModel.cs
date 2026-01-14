using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.HealthSafety
{
    public partial class AuditsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<OCC.Shared.Models.HseqAudit> _audits;

        public AuditsViewModel()
        {
            _audits = new System.Collections.ObjectModel.ObservableCollection<OCC.Shared.Models.HseqAudit>
            {
                new OCC.Shared.Models.HseqAudit
                {
                    Date = new System.DateTime(2025, 01, 10),
                    SiteName = "Site A - Warehouse",
                    AuditNumber = "AUD-001",
                    TargetScore = 95,
                    ActualScore = 92,
                    Findings = "Minor housekeeping issues.",
                    NonConformance = "Blocked fire extinguisher",
                    ImmediateAction = "Cleared blockage",
                    Status = "Closed",
                    HseqConsultant = "Sarah Conner"
                },
                new OCC.Shared.Models.HseqAudit
                {
                    Date = new System.DateTime(2025, 01, 12),
                    SiteName = "Site B - Office",
                    AuditNumber = "AUD-002",
                    TargetScore = 95,
                    ActualScore = 98,
                    Findings = "Excellent compliance.",
                    Status = "Closed",
                    HseqConsultant = "Sarah Conner"
                },
                new OCC.Shared.Models.HseqAudit
                {
                    Date = new System.DateTime(2025, 01, 14),
                    SiteName = "Site C - Construction",
                    AuditNumber = "AUD-003",
                    TargetScore = 95,
                    ActualScore = 85,
                    Findings = "Multiple PPE violations.",
                    NonConformance = "Workers without hard hats",
                    ImmediateAction = "Work stopped, briefing held",
                    Status = "Open",
                    HseqConsultant = "John Smith"
                }
            };
        }
    }
}
