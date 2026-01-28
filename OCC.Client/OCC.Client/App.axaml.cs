using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using System.Globalization;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using OCC.Client.Services;
using OCC.Client.Services.Infrastructure; // Added
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.ApiServices;
using OCC.Client.Services.Repositories.Interfaces; // Added
using OCC.Client.ViewModels.Core; // Added for ViewModelBase/Core VMs
using OCC.Client.ViewModels.EmployeeManagement;
using OCC.Client.ViewModels.HealthSafety;
using OCC.Client.ViewModels.Home;
using OCC.Client.ViewModels.Home.Calendar;
using OCC.Client.ViewModels.Home.Dashboard;
using OCC.Client.ViewModels.Home.Shared;
using OCC.Client.ViewModels.Login; // Added
using OCC.Client.ViewModels.Notifications; // Added
using OCC.Client.ViewModels.Orders;
using OCC.Client.ViewModels.Projects;
using OCC.Client.ViewModels.Projects.Shared;
using OCC.Client.ViewModels.Projects.Tasks;
using OCC.Client.ViewModels.Settings;
using OCC.Client.ViewModels.Shared;
using OCC.Client.ViewModels.Time;
using OCC.Client.Views.Core;
using OCC.Shared.Models;
using Serilog;
using System;
using System.Linq;
using LiveChartsCore; 
using LiveChartsCore.SkiaSharpView; 

namespace OCC.Client
{
    public partial class App : Application
    {
        public IServiceProvider? Services { get; private set; }

        public override void Initialize()
        {
            // Force dot decimal separator globally for consistency (e.g. for South African locale)
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ".";
            culture.NumberFormat.CurrencyDecimalSeparator = ".";
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            AvaloniaXamlLoader.Load(this);

            LiveCharts.Configure(config => 
                config
                    .AddSkiaSharp()
                    .AddDefaultMappers()
                    .AddLightTheme()
            );
        }

        public static void ChangeTheme(bool isDarkMode)
        {
            if (Current == null) 
            {
                System.Diagnostics.Debug.WriteLine("ChangeTheme: Current Application is null!");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"ChangeTheme: Switching to {(isDarkMode ? "Dark" : "Light")}");

            var merged = Current.Resources.MergedDictionaries;
            merged.Clear();
            
            var newTheme = isDarkMode 
                ? new Avalonia.Markup.Xaml.Styling.ResourceInclude(new Uri("avares://OCC.Client/Styles/Themes/DarkMode.axaml")) { Source = new Uri("avares://OCC.Client/Styles/Themes/DarkMode.axaml") }
                : new Avalonia.Markup.Xaml.Styling.ResourceInclude(new Uri("avares://OCC.Client/Styles/Themes/LightMode.axaml")) { Source = new Uri("avares://OCC.Client/Styles/Themes/LightMode.axaml") };
            
            merged.Add(newTheme);
            System.Diagnostics.Debug.WriteLine("ChangeTheme: Theme switched.");
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                
                // Resolve the MainViewModel
                var shellViewModel = Services.GetRequiredService<MainViewModel>(); 

                // Startup Logic: Splash -> Check Updates -> Main Window
                var updateService = Services.GetRequiredService<IUpdateService>();
                
                Views.SplashView? splashWindow = null;

                var splashVm = new ViewModels.SplashViewModel(updateService, () =>
                {
                    // On Splash Completed (No update or skipped)
                    var mainWindow = new MainWindow
                    {
                        DataContext = shellViewModel
                    };
                    
                    // Hook up user activity monitoring
                    var activityService = Services.GetRequiredService<UserActivityService>();
                    activityService.Monitor(mainWindow);

                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();
                    
                    // Close Splash
                    splashWindow?.Close();
                });

                splashWindow = new Views.SplashView
                {
                    DataContext = splashVm
                };
                
                desktop.MainWindow = splashWindow;
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = Services.GetRequiredService<MainViewModel>()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Database
            services.AddDbContext<Data.AppDbContext>(options => { }, ServiceLifetime.Transient); 

            // Repositories
            // repositories - Specific Repositories for API
            services.AddTransient<IRepository<User>, ApiUserRepository>();
            services.AddTransient<IRepository<Employee>, ApiEmployeeRepository>();
            services.AddTransient<IRepository<Project>, ApiProjectRepository>();
            services.AddTransient<IRepository<ProjectTask>, ApiProjectTaskRepository>();
            services.AddTransient<IProjectTaskRepository, ApiProjectTaskRepository>();
            services.AddTransient<IRepository<Customer>, ApiCustomerRepository>();
            services.AddTransient<IRepository<TaskAssignment>, ApiTaskAssignmentRepository>();
            services.AddTransient<IRepository<TaskComment>, ApiTaskCommentRepository>();
            services.AddTransient<IRepository<TimeRecord>, ApiTimeRecordRepository>();
            services.AddTransient<IRepository<AttendanceRecord>, ApiAttendanceRecordRepository>();
            services.AddTransient<IRepository<AppSetting>, ApiAppSettingRepository>();
            
            // Teams
            services.AddTransient<IRepository<Team>, ApiTeamRepository>();
            services.AddTransient<IRepository<TeamMember>, ApiTeamMemberRepository>();
            
            // Leave & Holidays
            services.AddTransient<IRepository<LeaveRequest>, ApiLeaveRequestRepository>();
            services.AddTransient<IRepository<PublicHoliday>, ApiPublicHolidayRepository>();
            services.AddTransient<IRepository<OvertimeRequest>, ApiOvertimeRequestRepository>();

            // Fallback for any other type not explicitly mapped (e.g. TimeRecord) - though unlikely to be used if we covered main ones
            // services.AddTransient(typeof(IRepository<>), typeof(SqlRepository<>));

             // services.AddSingleton<ITimeService, TimeService>();
             services.AddSingleton<ITimeService, TimeService>();
             
             // Auth Services
             services.AddSingleton<ApiAuthService>();
             services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<ApiAuthService>());
             services.AddSingleton<IAuditLogService, ApiAuditLogService>();

            services.AddSingleton<INotificationService, ApiNotificationService>();
            services.AddSingleton<IUpdateService, UpdateService>();
            services.AddSingleton<UserActivityService>();
            services.AddSingleton<OrderStateService>(); // New
            services.AddSingleton<SignalRNotificationService>();
            services.AddSingleton<UserActivityService>();
            services.AddSingleton<IPermissionService, PermissionService>();
            services.AddTransient<IHolidayService, HolidayService>();
            services.AddSingleton<LocalSettingsService>();
            services.AddSingleton<UserPreferencesService>(); // Local User Preferences (Timeout etc)
            services.AddSingleton(ConnectionSettings.Instance);
            services.AddTransient<ILeaveService, LeaveService>();
            services.AddHttpClient<IOrderService, OrderService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl));
            services.AddHttpClient<IProjectVariationOrderService, ProjectVariationOrderService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl));
            services.AddHttpClient<IInventoryService, InventoryService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl));
            services.AddHttpClient<ISupplierService, SupplierService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl));
            services.AddHttpClient<ISettingsService, SettingsService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl));
            services.AddHttpClient<IBugReportService, BugReportService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl));
            services.AddHttpClient<IHealthSafetyService, ApiHealthSafetyService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl));
            services.AddSingleton<IWageService, WageService>();
            services.AddSingleton<IDialogService, DialogService>();
            
            // ...

            services.AddTransient<TeamManagementViewModel>();
            services.AddTransient<TeamDetailViewModel>();
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<WageRunViewModel>();
            services.AddSingleton<IPdfService, PdfService>();
            services.AddSingleton<IToastService, ToastService>();
            services.AddHttpClient<OCC.Client.Services.External.IGoogleMapsService, OCC.Client.Services.External.GoogleMapsService>();
            services.AddSingleton<IProjectManager, ProjectManager>();
            services.AddSingleton<IExportService, ExportService>();
            services.AddSingleton<ISupplierImportService, SupplierImportService>();
            services.AddSingleton<IInventoryImportService, InventoryImportService>();
            services.AddSingleton<IEmployeeImportService, EmployeeImportService>();

            // Logging
            services.AddLogging(l => l.AddSerilog());

            // ViewModels

            // Core
            services.AddTransient<ShellViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddSingleton<SideMenuViewModel>();
            services.AddTransient<AccessDeniedViewModel>();

            // Login and Registration

            services.AddTransient<RegisterViewModel>();
            services.AddTransient<LoginViewModel>();
            
            // Home
            services.AddTransient<HomeMenuViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<SummaryViewModel>();
            services.AddTransient<TasksWidgetViewModel>();
            services.AddTransient<PulseViewModel>();
            services.AddSingleton<NotificationViewModel>();
            
            // Project
            
            services.AddTransient<ProjectsViewModel>();
            services.AddTransient<ProjectMainMenuViewModel>();



            services.AddTransient<TaskListViewModel>();
            services.AddTransient<ProjectListViewModel>();
            services.AddTransient<ProjectReportViewModel>();
            services.AddTransient<ProjectDetailViewModel>();
            services.AddTransient<CreateProjectViewModel>();
            services.AddTransient<EditProjectViewModel>();
            services.AddTransient<ProjectTopBarViewModel>();

            services.AddTransient<ProjectGanttViewModel>();
            services.AddTransient<ProjectCalendarViewModel>();
            services.AddTransient<ProjectVariationOrderListViewModel>();
            services.AddTransient<UserManagementViewModel>();
            services.AddTransient<ManageUsersViewModel>();
            services.AddTransient<AuditLogViewModel>();
            services.AddTransient<TaskDetailViewModel>(); // If needed
            services.AddTransient<EmployeeManagementViewModel>();
            services.AddTransient<ViewModels.Customers.CustomerManagementViewModel>();
            services.AddTransient<ViewModels.Customers.CustomerDetailViewModel>();
            services.AddTransient<EmployeeDetailViewModel>();
            services.AddTransient<TimeLiveViewModel>();
            services.AddTransient<TimeMenuViewModel>();
            services.AddTransient<TimeAttendanceViewModel>();

            services.AddTransient<DailyTimesheetViewModel>(); // Unified View

            services.AddTransient<AttendanceHistoryViewModel>();
            services.AddTransient<LeaveApplicationViewModel>();
            services.AddTransient<LeaveApprovalViewModel>();
            services.AddTransient<OvertimeViewModel>();
            services.AddTransient<OvertimeApprovalViewModel>();
            services.AddTransient<CalendarViewModel>();
            services.AddTransient<LeaveCalendarViewModel>();
            // services.AddTransient<TeamsViewModel>(); // Removed
            services.AddTransient<TeamManagementViewModel>();
            services.AddTransient<TeamDetailViewModel>();
            services.AddTransient<ProfileViewModel>();
            
            // Health Safety
            // Health Safety
            services.AddTransient<HealthSafetyViewModel>();
            services.AddTransient<HealthSafetyDashboardViewModel>();
            services.AddTransient<HealthSafetyMenuViewModel>();
            services.AddTransient<PerformanceMonitoringViewModel>();
            services.AddTransient<IncidentsViewModel>();
            services.AddTransient<TrainingViewModel>();
            services.AddTransient<AuditsViewModel>();
            services.AddTransient<DocumentsViewModel>();

            // Orders
            services.AddTransient<IOrderManager, OrderManager>();
            services.AddTransient<OrderMenuViewModel>();
            services.AddTransient<OrderViewModel>();
            services.AddTransient<InventoryViewModel>();
            services.AddTransient<ItemDetailViewModel>();
            services.AddTransient<ItemListViewModel>();
            services.AddTransient<CreateOrderViewModel>();
            services.AddTransient<OrderListViewModel>();
            services.AddTransient<SupplierListViewModel>();
            services.AddTransient<SupplierDetailViewModel>();
            services.AddTransient<ReceiveOrderViewModel>();
            services.AddTransient<OrderDashboardViewModel>();
            services.AddTransient<RestockReviewViewModel>();
            services.AddTransient<ViewModels.Bugs.BugListViewModel>();

            // Settings
            services.AddTransient<CompanySettingsViewModel>();
            services.AddTransient<UserPreferencesViewModel>(); // New
            services.AddTransient<ViewModels.Developer.DeveloperViewModel>();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}
