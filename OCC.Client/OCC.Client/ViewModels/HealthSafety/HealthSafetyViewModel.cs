using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.HealthSafety
{
    public partial class HealthSafetyViewModel : ViewModelBase
    {
        [ObservableProperty]
        private HealthSafetyMenuViewModel _menuViewModel;

        [ObservableProperty]
        private ViewModelBase _currentView;

        [ObservableProperty]
        private HealthSafetyDashboardViewModel _dashboardView;

        [ObservableProperty]
        private PerformanceMonitoringViewModel _performanceView;

        [ObservableProperty]
        private IncidentsViewModel _incidentsView;

        [ObservableProperty]
        private TrainingViewModel _trainingView;

        [ObservableProperty]
        private AuditsViewModel _auditsView;

        [ObservableProperty]
        private DocumentsViewModel _documentsView;

        public HealthSafetyViewModel()
        {
            // Design-time
            _menuViewModel = null!;
            _dashboardView = null!;
            _performanceView = null!;
            _incidentsView = null!;
            _trainingView = null!;
            _auditsView = null!;
            _documentsView = null!;
            _currentView = null!;
        }

        public HealthSafetyViewModel(
            HealthSafetyMenuViewModel menuViewModel,
            HealthSafetyDashboardViewModel dashboardView,
            PerformanceMonitoringViewModel performanceView,
            IncidentsViewModel incidentsView,
            TrainingViewModel trainingView,
            AuditsViewModel auditsView,
            DocumentsViewModel documentsView)
        {
            _menuViewModel = menuViewModel;
            _dashboardView = dashboardView;
            _performanceView = performanceView;
            _incidentsView = incidentsView;
            _trainingView = trainingView;
            _auditsView = auditsView;
            _documentsView = documentsView;
            
            // Default view
            _currentView = _dashboardView;

            _menuViewModel.PropertyChanged += MenuViewModel_PropertyChanged;
        }

        private void MenuViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HealthSafetyMenuViewModel.ActiveTab))
            {
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            switch (_menuViewModel.ActiveTab)
            {
                case "Performance Monitoring":
                    CurrentView = _performanceView;
                    break;
                case "Incidents":
                    CurrentView = _incidentsView;
                    break;
                case "Training":
                    CurrentView = _trainingView;
                    break;
                case "Audits":
                    CurrentView = _auditsView;
                    break;
                case "Documents":
                    CurrentView = _documentsView;
                    break;
                case "Dashboard":
                default:
                    CurrentView = _dashboardView;
                    break;
            }
        }
    }
}
