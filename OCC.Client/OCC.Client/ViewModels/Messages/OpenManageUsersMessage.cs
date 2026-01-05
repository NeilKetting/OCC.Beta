using CommunityToolkit.Mvvm.Messaging.Messages;

namespace OCC.Client.ViewModels.Messages
{
    public class OpenManageUsersMessage : ValueChangedMessage<bool>
    {
        public OpenManageUsersMessage(bool value = true) : base(value)
        {
        }
    }
}
