using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OCC.WpfClient.Services;
using OCC.WpfClient.Services.Interfaces;
using OCC.WpfClient.Features.AuthHub.ViewModels;
using OCC.WpfClient.Features.Splash.Views;
using OCC.WpfClient.Features.Splash.ViewModels;
using OCC.WpfClient.Features.Shell.ViewModels;
using OCC.WpfClient.Features.Main.ViewModels;
using OCC.WpfClient.Services.Infrastructure;
using OCC.WpfClient.Services.Infrastructure.Logging;
using OCC.WpfClient.Features.Employees.ViewModels;

namespace OCC.WpfClient
{
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            // Initialize Velopack
            Velopack.VelopackApp.Build().Run();

            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Show Splash Screen
            var splashViewModel = ServiceProvider.GetRequiredService<SplashViewModel>();
            var splashView = new SplashView { DataContext = splashViewModel };
            
            this.MainWindow = splashView;
            splashView.Show();

            // Perform Initialization
            await splashViewModel.InitializeAsync();

            // Show Main Window
            var mainWindow = new MainWindow();
            this.MainWindow = mainWindow;
            mainWindow.Show();
            
            splashView.Close();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Logging
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
                builder.AddWpfFileLogger();
            });

            // Infrastructure
            services.AddSingleton<ConnectionSettings>();
            services.AddSingleton<LocalSettingsService>();
            services.AddSingleton<ILocalEncryptionService, LocalEncryptionService>();

            // Services
            services.AddHttpClient();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IPermissionService, PermissionService>();
            services.AddSingleton<IUpdateService, UpdateService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IEmployeeService, EmployeeService>();
            services.AddSingleton<IUserService, UserService>();

            // ViewModels
            services.AddTransient<ShellViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<DashboardViewModel>(); // Registered DashboardViewModel
            services.AddTransient<OCC.WpfClient.Features.Chat.ViewModels.ChatViewModel>();
            services.AddTransient<EmployeeListViewModel>();
            services.AddTransient<SplashViewModel>();
            services.AddTransient<AuthViewModel>();
        }
    }
}
