using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Linq;
using System;
using OCC.Client.Features.HseqHub.ViewModels;

namespace OCC.Client.Features.HseqHub.Views
{
    public partial class IncidentsView : UserControl
    {
        public IncidentsView()
        {
            InitializeComponent();
            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DragOverEvent, DragOver);
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
            if (DataContext is IncidentsViewModel vm && e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.Get(DataFormats.Files);
                if (files != null)
                {
                    vm.UploadPhotosCommand.Execute(files);
                }
            }
        }

        private async void Browse_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not IncidentsViewModel vm) return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Photos to Upload",
                AllowMultiple = true,
                FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
            });

            if (files != null && files.Any())
            {
                vm.UploadPhotosCommand.Execute(files);
            }
        }
    }
}
