using Microsoft.Extensions.DependencyInjection;
using OCC.WpfClient.Features.AuthHub.ViewModels;
using System.Windows;

namespace OCC.WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            StateChanged += MainWindow_StateChanged;

            if (Application.Current is App app && app.ServiceProvider != null)
            {
                var shellVm = app.ServiceProvider.GetRequiredService<Features.Shell.ViewModels.ShellViewModel>();
                DataContext = shellVm;
                
                // Set initial view
                shellVm.Navigation.NavigateTo<AuthViewModel>();

                // Monitor navigation for logout resizing
                if (shellVm.Navigation is System.ComponentModel.INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == "CurrentView")
                        {
                            if (shellVm.Navigation.CurrentView is AuthViewModel)
                            {
                                WindowState = WindowState.Normal;
                                Width = 1024;
                                Height = 700;
                            }
                        }
                    };
                }
            }
        }

        private void MainWindow_StateChanged(object? sender, System.EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                BtnMaximize.Content = ""; // Restore icon
            }
            else
            {
                BtnMaximize.Content = ""; // Maximize icon
            }
        }

        private void OnMinimizeClick(object? sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnMaximizeRestoreClick(object? sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}