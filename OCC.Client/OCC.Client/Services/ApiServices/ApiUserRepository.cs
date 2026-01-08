using System.Threading.Tasks;
using OCC.Shared.Models;
using OCC.Client.Services.Interfaces;

namespace OCC.Client.Services.ApiServices
{
    public class ApiUserRepository : BaseApiService<User>
    {
        public ApiUserRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "Users";
    }
}
