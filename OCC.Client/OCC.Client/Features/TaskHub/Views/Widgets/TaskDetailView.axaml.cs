using OCC.Client.Features.TaskHub.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

using System.Linq;

namespace OCC.Client.Features.TaskHub.Views.Widgets
{
    public partial class TaskDetailView : UserControl
    {
        public TaskDetailView()
        {
            InitializeComponent();
            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DragOverEvent, DragOver);
        }

        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
        }

        public void FocusInput()
        {
            this.Focus();
        }

        private void Duration_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is TaskDetailViewModel vm)
            {
                vm.CommitDurationsCommand.Execute(null);
            }
        }

        private void DragOver(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Files))
            {
               e.DragEffects = DragDropEffects.Copy;
            }
            else
            {
               e.DragEffects = DragDropEffects.None;
            }
        }

        private void Drop(object? sender, DragEventArgs e)
        {
            if (DataContext is TaskDetailViewModel vm && e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.Get(DataFormats.Files);
                if (files != null)
                {
                    vm.UploadFilesCommand.Execute(files);
                }
            }
        }

        private async void Browse_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not TaskDetailViewModel vm) return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Files to Upload",
                AllowMultiple = true
            });

            if (files != null && files.Any())
            {
                vm.UploadFilesCommand.Execute(files);
            }
        }
    }
}




