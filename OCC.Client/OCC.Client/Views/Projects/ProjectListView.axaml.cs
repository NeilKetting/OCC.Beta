using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Projects;

    public partial class ProjectListView : UserControl
    {
        public ProjectListView()
        {
            InitializeComponent();
        }

        private void DataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            if (sender is DataGrid dg && dg.SelectedItem is ViewModels.Projects.Dashboard.ProjectDashboardItemViewModel project && 
                DataContext is ViewModels.Projects.ProjectListViewModel vm)
            {
                vm.OpenProjectCommand.Execute(project);
            }
        }
    }
