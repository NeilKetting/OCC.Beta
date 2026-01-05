using Avalonia.Controls;
using Avalonia.Input;
using OCC.Client.ViewModels.Projects;

namespace OCC.Client.Views.Projects
{
    public partial class ProjectsView : UserControl
    {
        public ProjectsView()
        {
            InitializeComponent();
        }

        private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.Source is Grid grid && grid.Name == "OverlayGrid")
            {
                if (DataContext is ProjectsViewModel vm)
                {
                    vm.CloseTaskDetailCommand.Execute(null);
                }
            }
        }
    }
}
