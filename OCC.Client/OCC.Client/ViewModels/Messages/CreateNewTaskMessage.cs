using CommunityToolkit.Mvvm.Messaging.Messages;
using System;

namespace OCC.Client.ViewModels.Messages
{
    public class CreateNewTaskMessage
    {
        public Guid? ProjectId { get; }

        public CreateNewTaskMessage(Guid? projectId = null)
        {
            ProjectId = projectId;
        }
    }
}
