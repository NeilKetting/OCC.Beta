using Avalonia.Controls;
using vm = OCC.Client.ViewModels.Home.Dashboard;

namespace OCC.Client.Views.Home.MySummary.Widgets
{
    public partial class TasksWidgetView : UserControl
    {
        public TasksWidgetView()
        {
            InitializeComponent();
        }

        private void OnTaskDoubleTapping(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Border border && border.Tag is ViewModels.Home.Shared.HomeTaskItem task && DataContext is vm.TasksWidgetViewModel vmModel)
            {
                vmModel.OpenTaskCommand.Execute(task);
            }
        }

        private void OnOpenClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.DataContext is ViewModels.Home.Shared.HomeTaskItem item && DataContext is vm.TasksWidgetViewModel vm)
            {
                vm.OpenTaskCommand.Execute(item);
            }
        }

        private void OnCompleteClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.DataContext is ViewModels.Home.Shared.HomeTaskItem item && DataContext is vm.TasksWidgetViewModel vm)
            {
                vm.CompleteTaskCommand.Execute(item);
            }
        }

        private void OnDeleteClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.DataContext is ViewModels.Home.Shared.HomeTaskItem item && DataContext is vm.TasksWidgetViewModel vm)
            {
                vm.DeleteTaskCommand.Execute(item);
            }
        }
    }
}
