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
using OCC.Client.Features.EmployeeHub.ViewModels;
using OCC.Client.Features.TimeAttendanceHub.ViewModels;
using OCC.Client.Features.TimeAttendanceHub.Views;
using OCC.Client.Features.HseqHub.ViewModels;
using OCC.Client.Features.HomeHub.ViewModels;
using OCC.Client.Features.HomeHub.ViewModels.Dashboard;
using OCC.Client.Features.HomeHub.ViewModels.Shared;
using OCC.Client.Features.AuthHub.ViewModels;
using OCC.Client.Features.SettingsHub.ViewModels;
using OCC.Client.Features.BugHub.ViewModels;
using OCC.Client.Features.CustomerHub.ViewModels;
using OCC.Client.ViewModels.Notifications; // Added
using OCC.Client.Features.OrdersHub.ViewModels;
using OCC.Client.Features.OrdersHub.UseCases;
using OCC.Client.Features.ProjectsHub.ViewModels;
using OCC.Client.Features.TaskHub.ViewModels;
using OCC.Client.Features.CalendarHub.ViewModels;
using OCC.Client.ViewModels.Shared;
using OCC.Client.Views.Core;
using OCC.Client.Features.CoreHub.Views; // Added
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
            
            try
            {
                merged.Add(newTheme);
            }
            catch (ArgumentException ex) { System.Diagnostics.Debug.WriteLine($"Error changing theme: {ex.Message}"); }
            System.Diagnostics.Debug.WriteLine("ChangeTheme: Theme switched.");
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                
                // Resolve the MainViewModel
                var shellViewModel = Services.GetRequiredService<MainViewModel>(); 

                // Startup Logic: Splash -> Check Updates -> Main Window
                var updateService = Services.GetRequiredService<IUpdateService>();
                
                SplashView? splashWindow = null;

                var splashVm = new OCC.Client.Features.CoreHub.ViewModels.SplashViewModel(updateService, () =>
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

                splashWindow = new SplashView
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

        public void ConfigureServices(IServiceCollection services)
        {
            // --- Infrastructure & Core Services ---
            services.AddTransient<FailureLoggingHandler>();
            services.AddLogging(l => l.AddSerilog());
            services.AddSingleton(ConnectionSettings.Instance);
            services.AddSingleton<ITimeService, TimeService>();
            services.AddSingleton<IUpdateService, UpdateService>();
            services.AddSingleton<IPdfService, PdfService>();
            services.AddSingleton<IExportService, ExportService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IToastService, ToastService>();
            services.AddSingleton<UserActivityService>();
            services.AddSingleton<SignalRNotificationService>();
            services.AddSingleton<IPermissionService, PermissionService>();
            services.AddSingleton<LocalSettingsService>();
            services.AddSingleton<UserPreferencesService>();
            services.AddHttpClient<OCC.Client.Services.External.IGoogleMapsService, OCC.Client.Services.External.GoogleMapsService>()
                .AddHttpMessageHandler<FailureLoggingHandler>();
            services.AddSingleton<ILogUploadService, LogUploadService>();

            // --- Database & Repositories ---
            services.AddDbContext<Data.AppDbContext>(options => { }, ServiceLifetime.Transient); 
            
            // Repositories - API
            services.AddTransient<IRepository<User>, ApiUserRepository>();

            // Instrumented Repositories (Logging)
            services.AddHttpClient<IRepository<Employee>, ApiEmployeeRepository>(c => c.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();

            services.AddTransient<IRepository<Project>, ApiProjectRepository>();
            services.AddTransient<IRepository<ProjectTask>, ApiProjectTaskRepository>();
            services.AddTransient<IProjectTaskRepository, ApiProjectTaskRepository>();
            services.AddTransient<IRepository<Customer>, ApiCustomerRepository>();
            services.AddTransient<IRepository<TaskAssignment>, ApiTaskAssignmentRepository>();
            services.AddTransient<IRepository<TaskComment>, ApiTaskCommentRepository>();

            services.AddHttpClient<IRepository<TimeRecord>, ApiTimeRecordRepository>(c => c.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();

            services.AddHttpClient<IRepository<AttendanceRecord>, ApiAttendanceRecordRepository>(c => c.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();

            services.AddTransient<IRepository<AppSetting>, ApiAppSettingRepository>();
            services.AddTransient<IRepository<Team>, ApiTeamRepository>();
            services.AddTransient<IRepository<TeamMember>, ApiTeamMemberRepository>();

            services.AddHttpClient<IRepository<LeaveRequest>, ApiLeaveRequestRepository>(c => c.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();

            services.AddTransient<IRepository<PublicHoliday>, ApiPublicHolidayRepository>();

            services.AddHttpClient<IRepository<OvertimeRequest>, ApiOvertimeRequestRepository>(c => c.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();

            // --- Core Hub (Shell & Main) ---
            services.AddTransient<ShellViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddSingleton<SideMenuViewModel>();
            services.AddTransient<AccessDeniedViewModel>();

            // --- Auth Hub ---
            services.AddSingleton<ApiAuthService>();
            services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<ApiAuthService>());
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();

            // --- Home Hub ---
            services.AddTransient<HomeViewModel>();
            services.AddTransient<HomeMenuViewModel>();
            services.AddTransient<SummaryViewModel>();
            services.AddTransient<TasksWidgetViewModel>();
            services.AddTransient<PulseViewModel>();
            services.AddSingleton<NotificationViewModel>();
            services.AddSingleton<INotificationService, ApiNotificationService>();

            // --- Projects & Tasks Hub ---
            services.AddSingleton<IProjectManager, ProjectManager>();
            services.AddTransient<ProjectsViewModel>();
            services.AddTransient<ProjectMainMenuViewModel>();
            services.AddTransient<ProjectListViewModel>();
            services.AddTransient<ProjectDetailViewModel>();
            services.AddTransient<CreateProjectViewModel>();
            services.AddTransient<EditProjectViewModel>();
            services.AddTransient<ProjectReportViewModel>();
            services.AddTransient<ProjectTopBarViewModel>();
            services.AddTransient<ProjectGanttViewModel>();
            services.AddTransient<ProjectVariationOrderListViewModel>();
            services.AddTransient<ProjectFilesViewModel>();
            services.AddHttpClient<IProjectService, ProjectService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();
            services.AddHttpClient<IProjectVariationOrderService, ProjectVariationOrderService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();
            
            services.AddTransient<TaskListViewModel>();
            services.AddTransient<TaskDetailViewModel>();
            services.AddHttpClient<ITaskAttachmentService, ApiTaskAttachmentService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();

            // --- Employee Hub ---
            services.AddHttpClient<IEmployeeService, ApiEmployeeService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();
            services.AddTransient<EmployeeManagementViewModel>();
            services.AddTransient<EmployeeDetailViewModel>();
            services.AddSingleton<IEmployeeImportService, EmployeeImportService>();
            
            services.AddTransient<TeamManagementViewModel>();
            services.AddTransient<TeamDetailViewModel>();
            
            services.AddTransient<IWageService, WageService>();
            services.AddTransient<WageRunViewModel>();

            services.AddHttpClient<IEmployeeLoanService, ApiEmployeeLoanService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();
            services.AddTransient<LoansManagementViewModel>();
            services.AddTransient<AddLoanDialogViewModel>();
            
            services.AddTransient<TimeAttendanceViewModel>();
            services.AddTransient<TimeLiveViewModel>();
            services.AddTransient<TimeMenuViewModel>();
            services.AddTransient<DailyTimesheetViewModel>();
            services.AddTransient<AttendanceHistoryViewModel>();

            services.AddTransient<ILeaveService, LeaveService>();
            services.AddTransient<LeaveApplicationViewModel>();
            services.AddTransient<LeaveApprovalViewModel>();
            
            services.AddTransient<IHolidayService, HolidayService>();
            services.AddTransient<OvertimeViewModel>();
            services.AddTransient<OvertimeApprovalViewModel>();
            
            services.AddSingleton<IReminderService, ReminderService>();

            // --- Calendar Hub ---
            services.AddTransient<ICalendarService, CalendarService>();
            services.AddTransient<CalendarHubViewModel>();

            // --- Customer Hub ---
            services.AddHttpClient<ICustomerService, CustomerService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();
            services.AddTransient<CustomerManagementViewModel>();
            services.AddTransient<CustomerDetailViewModel>();

            // --- Orders Hub ---
            services.AddSingleton<OrderStateService>();
            services.AddTransient<IOrderLifecycleService, OrderLifecycleService>();
            services.AddTransient<IOrderManager, OrderManager>();
            services.AddHttpClient<IOrderService, OrderService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();
            services.AddHttpClient<IInventoryService, InventoryService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();
            services.AddHttpClient<ISupplierService, SupplierService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();
            services.AddSingleton<IInventoryImportService, InventoryImportService>();
            services.AddSingleton<ISupplierImportService, SupplierImportService>();
            
            services.AddTransient<OrderViewModel>();
            services.AddTransient<OrderMenuViewModel>();
            services.AddTransient<OrderDashboardViewModel>();
            services.AddTransient<OrderListViewModel>();
            services.AddSingleton<IOrderCalculationService, OrderCalculationService>();
            services.AddTransient<OrderLinesViewModel>();
            services.AddTransient<InventoryLookupViewModel>();
            services.AddTransient<SupplierSelectorViewModel>();
            services.AddTransient<OrderSubmissionUseCase>();
            services.AddTransient<CreateOrderViewModel>();
            services.AddTransient<ReceiveOrderViewModel>();
            services.AddTransient<InventoryViewModel>();
            services.AddTransient<ItemDetailViewModel>();
            services.AddTransient<ItemListViewModel>();
            services.AddTransient<SupplierListViewModel>();
            services.AddTransient<SupplierDetailViewModel>();
            services.AddTransient<RestockReviewViewModel>();

            // --- HSEQ Hub ---
            services.AddHttpClient<IHealthSafetyService, ApiHealthSafetyService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();
            services.AddTransient<HealthSafetyViewModel>();
            services.AddTransient<HealthSafetyDashboardViewModel>();
            services.AddTransient<HealthSafetyMenuViewModel>();
            services.AddTransient<PerformanceMonitoringViewModel>();
            services.AddTransient<IncidentsViewModel>();
            services.AddTransient<IncidentEditorViewModel>();
            services.AddTransient<TrainingViewModel>();
            services.AddTransient<TrainingEditorViewModel>();
            services.AddTransient<AuditsViewModel>();
            services.AddTransient<AuditEditorViewModel>();
            services.AddTransient<AuditDeviationsViewModel>();
            services.AddTransient<DocumentsViewModel>();

            // --- Bug Hub ---
            services.AddHttpClient<IBugReportService, BugReportService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();
            services.AddTransient<BugListViewModel>();
            services.AddTransient<SupportCenterViewModel>();

            // --- Settings & Admin Hub ---
            services.AddHttpClient<ISettingsService, SettingsService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl))
                .AddHttpMessageHandler<FailureLoggingHandler>();
            services.AddSingleton<IAuditLogService, ApiAuditLogService>();
            services.AddTransient<AuditLogViewModel>();
            services.AddTransient<CompanySettingsViewModel>();
            services.AddTransient<UserManagementViewModel>();
            services.AddTransient<ManageUsersViewModel>();
            services.AddTransient<UserPreferencesViewModel>();
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<OCC.Client.ViewModels.Developer.DeveloperViewModel>();
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
