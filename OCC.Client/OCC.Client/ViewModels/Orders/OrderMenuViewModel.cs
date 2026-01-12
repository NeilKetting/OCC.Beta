using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Client.ViewModels.Messages;
using OCC.Client.ViewModels.Notifications;
using System;

namespace OCC.Client.ViewModels.Orders
{
    /// <summary>
    /// ViewModel for the Order module's side menu, managing tab selection and notification access.
    /// </summary>
    public partial class OrderMenuViewModel : ViewModelBase, IRecipient<SwitchTabMessage>
    {
        #region Private Members
        #endregion

        #region Observables

        /// <summary>
        /// Gets or sets the currently active tab identifier.
        /// </summary>
        [ObservableProperty]
        private string _activeTab = "Dashboard";

        /// <summary>
        /// Gets or sets the display email of the currently authenticated user.
        /// </summary>
        [ObservableProperty]
        private string _userEmail = "origize63@gmail.com";

        /// <summary>
        /// Gets the ViewModel for managing and displaying notifications.
        /// </summary>
        public NotificationViewModel NotificationVM { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderMenuViewModel"/> class with required dependencies.
        /// </summary>
        /// <param name="notificationVM">ViewModel for the notification system.</param>
        /// <param name="authService">Service for retrieving current user information.</param>
        public OrderMenuViewModel(NotificationViewModel notificationVM, IAuthService authService)
        {
            NotificationVM = notificationVM;
            WeakReferenceMessenger.Default.RegisterAll(this);
            if (authService.CurrentUser != null)
            {
                UserEmail = authService.CurrentUser.Email;
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to update the active tab and notify the parent container.
        /// </summary>
        /// <param name="tabName">The name of the tab to activate.</param>
        [RelayCommand]
        private void SetActiveTab(string tabName)
        {
            ActiveTab = tabName;
            TabSelected?.Invoke(this, tabName);
        }

        /// <summary>
        /// Command to request opening the global notifications overlay.
        /// </summary>
        [RelayCommand]
        private void OpenNotifications()
        {
            WeakReferenceMessenger.Default.Send(new OpenNotificationsMessage());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Responds to external requests to switch tabs (e.g., from deep links).
        /// </summary>
        /// <param name="message">The tab switching request message.</param>
        public void Receive(SwitchTabMessage message)
        {
            ActiveTab = message.Value;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Event raised when a new tab is selected in the menu.
        /// </summary>
        public event EventHandler<string>? TabSelected;

        #endregion
    }
}
