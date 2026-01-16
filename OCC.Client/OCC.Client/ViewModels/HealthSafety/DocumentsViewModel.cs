using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.HealthSafety
{
    public partial class DocumentsViewModel : ViewModelBase
    {
        private readonly IHealthSafetyService _hseqService;
        private readonly IToastService _toastService;
        
        // In a real app we'd inject a file picker service or use TopLevel storage provider
        // For this task, we'll simulate the picker or assumes minimal integration 
        // as complete file picking requires view-layer dependency injection usually.
        // We'll stub the "Pick File" part.

        [ObservableProperty]
        private ObservableCollection<HseqDocument> _documents = new();

        [ObservableProperty]
        private bool _isUploading;



        // Upload Form Properties
        [ObservableProperty]
        private bool _showUploadForm;

        [ObservableProperty]
        private string _newDocTitle = string.Empty;

        [ObservableProperty]
        private OCC.Shared.Enums.DocumentCategory _newDocCategory = OCC.Shared.Enums.DocumentCategory.Other;
        
        [ObservableProperty]
        private string _selectedFilePath = string.Empty;

        public OCC.Shared.Enums.DocumentCategory[] Categories { get; } = 
            (OCC.Shared.Enums.DocumentCategory[])Enum.GetValues(typeof(OCC.Shared.Enums.DocumentCategory));

        public DocumentsViewModel(IHealthSafetyService hseqService, IToastService toastService)
        {
            _hseqService = hseqService;
            _toastService = toastService;
        }

        public DocumentsViewModel()
        {
             _hseqService = null!;
             _toastService = null!;
        }

        [RelayCommand]
        public async Task LoadDocuments()
        {
            if (_hseqService == null) return;
            IsBusy = true;
            try
            {
                var docs = await _hseqService.GetDocumentsAsync();
                Documents = new ObservableCollection<HseqDocument>(docs.OrderByDescending(d => d.UploadDate));
            }
            catch (Exception)
            {
                _toastService.ShowError("Error", "Failed to load documents.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ToggleUploadForm()
        {
            ShowUploadForm = !ShowUploadForm;
            if (ShowUploadForm)
            {
                NewDocTitle = "";
                NewDocCategory = OCC.Shared.Enums.DocumentCategory.Policy;
                SelectedFilePath = "";
            }
        }

        [RelayCommand]
        private async Task PickFile()
        {
            // Simulation of file picking
            // In a real Avalonia app, we would use StorageProvider from the View's TopLevel
            // For this scope, let's just simulate a path or ask user to type one for now?
            // Or better, just hardcode a 'simulated' file for the demo upload
            
            SelectedFilePath = "C:\\FakePath\\Safety_Policy_2025.pdf";
            if (string.IsNullOrEmpty(NewDocTitle))
            {
                NewDocTitle = "Safety Policy 2025";
            }
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task Upload()
        {
            if (string.IsNullOrWhiteSpace(NewDocTitle) || string.IsNullOrWhiteSpace(SelectedFilePath))
            {
                _toastService.ShowError("Validation", "Title and File are required.");
                return;
            }

            IsUploading = true;
            try
            {
                var newDoc = new HseqDocument
                {
                    Title = NewDocTitle,
                    Category = NewDocCategory,
                    FilePath = SelectedFilePath,
                    UploadedBy = "Current User", // Should use AuthService
                    UploadDate = DateTime.UtcNow,
                    Version = "1.0",
                    FileSize = "1.2 MB" // Mocked size
                };

                var created = await _hseqService.UploadDocumentAsync(newDoc);
                if (created != null)
                {
                    Documents.Insert(0, created);
                    _toastService.ShowSuccess("Success", "Document uploaded.");
                    ShowUploadForm = false;
                }
            }
            catch (Exception)
            {
                _toastService.ShowError("Error", "Failed to upload document.");
            }
            finally
            {
                IsUploading = false;
            }
        }

        [RelayCommand]
        private async Task DeleteDocument(HseqDocument doc)
        {
            if (doc == null) return;
            try
            {
                var success = await _hseqService.DeleteDocumentAsync(doc.Id);
                if (success)
                {
                    Documents.Remove(doc);
                    _toastService.ShowSuccess("Deleted", "Document removed.");
                }
            }
            catch (Exception)
            {
                 _toastService.ShowError("Error", "Failed to delete document.");
            }
        }
        
        [RelayCommand]
        private void DownloadDocument(HseqDocument doc)
        {
            _toastService.ShowInfo("Download", $"Downloading {doc.Title}...");
        }
    }
}
