using System.Windows.Controls;
using OCC.Shared.DTOs;
using OCC.WpfClient.Features.Chat.ViewModels;

namespace OCC.WpfClient.Features.Chat.Views
{
    public partial class ChatView : UserControl
    {
        public ChatView()
        {
            InitializeComponent();
        }

        private void MenuButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void Contact_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is ChatUserDto contact && DataContext is ChatViewModel vm)
            {
                vm.ToggleContactSelectionCommand.Execute(contact);
            }
        }

        private void Contact_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is ChatUserDto contact && DataContext is ChatViewModel vm)
            {
                vm.ToggleContactSelectionCommand.Execute(contact);
            }
        }
    }
}
