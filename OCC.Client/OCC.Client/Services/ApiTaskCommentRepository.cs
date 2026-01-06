using OCC.Shared.DTOs;
using OCC.Shared.Models;
using System.Net.Http.Json;

namespace OCC.Client.Services
{
    public class ApiTaskCommentRepository : BaseApiService<TaskComment>
    {
        public ApiTaskCommentRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "TaskComments";
    }
}
