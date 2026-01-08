using System.Threading.Tasks;
using OCC.Shared.Models;
using OCC.Client.Services.Interfaces;

namespace OCC.Client.Services.ApiServices
{
    public class ApiEmployeeRepository : BaseApiService<Employee>
    {
        public ApiEmployeeRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "Employees";
    }
}
