using OCC.Shared.DTOs;
using OCC.Shared.Models;
using System.Net.Http.Json;

namespace OCC.Client.Services
{
    public class ApiProjectRepository : BaseApiService<Project>
    {
        public ApiProjectRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "Projects";
    }
}
