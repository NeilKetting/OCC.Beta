using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Shared.Models;
using OCC.Client.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq;
using OCC.Client.Services.Infrastructure;
using OCC.Client.ViewModels.Core;
using OCC.Client.ViewModels.Messages;

namespace OCC.Client.ViewModels.EmployeeManagement
{
    public partial class TeamManagementViewModel : ViewModelBase, CommunityToolkit.Mvvm.Messaging.IRecipient<EntityUpdatedMessage>
    {
        private readonly IRepository<Team> _teamRepository;
        private readonly IServiceProvider _serviceProvider; 

        public ObservableCollection<Team> Teams { get; } = new();

        [ObservableProperty]
        private Team? _selectedTeam;
        
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string? _errorMessage;

        public event EventHandler<Team>? EditTeamRequested;

        public TeamManagementViewModel(IRepository<Team> teamRepository, IServiceProvider serviceProvider)
        {
            _teamRepository = teamRepository;
            _serviceProvider = serviceProvider;
            
            LoadData(); 
            CommunityToolkit.Mvvm.Messaging.IMessengerExtensions.RegisterAll(CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default, this);
        }

        private async void LoadData()
        {
            IsBusy = true;
            ErrorMessage = null;
            try 
            {
                var teams = await _teamRepository.GetAllAsync();
                Teams.Clear();
                foreach(var team in teams) Teams.Add(team);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void Receive(EntityUpdatedMessage message)
        {
             if (message.Value.EntityType == "Team") 
             {
                 Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(LoadData);
             }
        }

        [RelayCommand]
        private void AddTeam()
        {
            EditTeamRequested?.Invoke(this, new Team());
        }

        [RelayCommand]
        private void EditTeam(Team team)
        {
             EditTeamRequested?.Invoke(this, team);
        }

        [RelayCommand]
        private async Task DeleteTeam(Team team)
        {
             if (team == null) return;
             
             ErrorMessage = null;
             
             try
             {
                 await _teamRepository.DeleteAsync(team.Id);
             }
             catch (System.Net.Http.HttpRequestException ex)
             {
                 // Handle specific status codes if needed
                 if (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
                 {
                     ErrorMessage = "Cannot delete: Team has existing members. Please remove members first.";
                     // Ideally show a popup here
                     System.Diagnostics.Debug.WriteLine($"[TeamManagementViewModel] Delete Conflict: {ex.Message}");
                     CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new ViewModels.Messages.UpdateStatusMessage(ErrorMessage));
                 }
                 else
                 {
                     ErrorMessage = $"Error deleting team: {ex.Message}";
                     System.Diagnostics.Debug.WriteLine($"[TeamManagementViewModel] Delete Error: {ex.Message}");
                 }
             }
             catch (Exception ex)
             {
                 ErrorMessage = "An unexpected error occurred.";
                 System.Diagnostics.Debug.WriteLine($"[TeamManagementViewModel] General Error: {ex.Message}");
             }
             // SignalR will trigger reload on success
        }
    }
}
