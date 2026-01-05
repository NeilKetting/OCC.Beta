using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace OCC.Client.ViewModels.Shared
{
    public partial class ChangeEmailPopupViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _newEmail = string.Empty;

        public event EventHandler? CloseRequested;
        public event EventHandler<string>? EmailChanged;

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void ChangeEmail()
        {
            if (!string.IsNullOrWhiteSpace(NewEmail))
            {
                EmailChanged?.Invoke(this, NewEmail);
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
