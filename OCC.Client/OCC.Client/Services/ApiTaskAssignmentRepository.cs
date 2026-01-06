using OCC.Shared.DTOs;
using OCC.Shared.Models;
using System.Net.Http.Json;

namespace OCC.Client.Services
{
    // TaskAssignments are handled via ProjectTasks usually but might need direct access
    public class ApiTaskAssignmentRepository : BaseApiService<TaskAssignment>
    {
        public ApiTaskAssignmentRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "TaskAssignments";
    }
}
