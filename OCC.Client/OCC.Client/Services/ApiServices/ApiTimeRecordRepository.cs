using System.Threading.Tasks;
using OCC.Shared.Models;
using OCC.Client.Services.Interfaces;

namespace OCC.Client.Services.ApiServices
{
    public class ApiTimeRecordRepository : BaseApiService<TimeRecord>
    {
        public ApiTimeRecordRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "TimeRecords";
    }
}
