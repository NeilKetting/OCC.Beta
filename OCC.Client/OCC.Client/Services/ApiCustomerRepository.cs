using OCC.Shared.DTOs;
using OCC.Shared.Models;
using System.Net.Http.Json;

namespace OCC.Client.Services
{
    public class ApiCustomerRepository : BaseApiService<Customer>
    {
        public ApiCustomerRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "Customers";
    }
}
