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
            
            if (Application.Current is App app && app.ServiceProvider != null)
            {
                var shellVm = app.ServiceProvider.GetRequiredService<Features.Shell.ViewModels.ShellViewModel>();
                DataContext = shellVm;
                
                // Set initial view
                shellVm.Navigation.NavigateTo<AuthViewModel>();
            }
        }
    }
}