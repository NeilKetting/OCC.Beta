using CommunityToolkit.Mvvm.Messaging.Messages;
using OCC.Client.ViewModels.Shared;

namespace OCC.Client.ViewModels.Messages
{
    public class OpenWorkHoursMessage : ValueChangedMessage<WorkHoursPopupViewModel>
    {
        public OpenWorkHoursMessage(WorkHoursPopupViewModel value) : base(value)
        {
        }
    }
}
