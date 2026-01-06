using OCC.Shared.DTOs;
using OCC.Shared.Models;
using System.Net.Http.Json;

namespace OCC.Client.Services
{
    public class ApiProjectTaskRepository : BaseApiService<ProjectTask>
    {
        public ApiProjectTaskRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "ProjectTasks";
    }
}
