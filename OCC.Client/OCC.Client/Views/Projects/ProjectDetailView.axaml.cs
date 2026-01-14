using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Projects;

namespace OCC.Client.Views.Projects;

public partial class ProjectDetailView : UserControl
{
    public ProjectDetailView()
    {
        InitializeComponent();
    }

    private void TaskGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is DataGrid && 
            e.Source is Visual source && 
            source.FindAncestorOfType<DataGridRow>() is DataGridRow row && 
            row.DataContext is ProjectTask task &&
            DataContext is ProjectDetailViewModel vm)
        {
            // Trigger Preview
            if (vm.PreviewTaskDetailCommand.CanExecute(task))
            {
                vm.PreviewTaskDetailCommand.Execute(task);
            }
        }
    }

    private void TaskGrid_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
         if (DataContext is ProjectDetailViewModel vm)
         {
             vm.EndPreviewCommand.Execute(null);
         }
    }

    private void TaskGrid_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is DataGrid grid && 
            grid.SelectedItem is ProjectTask task && 
            DataContext is ProjectDetailViewModel vm)
        {
            // Trigger Pin
            if (vm.PinTaskDetailCommand.CanExecute(task))
            {
                vm.PinTaskDetailCommand.Execute(task);
            }
        }
    }
}
