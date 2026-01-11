using OCC.Shared.Models;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.Services.Interfaces;

namespace OCC.Client.Services.Repositories.ApiServices
{
    public class ApiAttendanceRecordRepository : BaseApiService<AttendanceRecord>
    {
        public ApiAttendanceRecordRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "AttendanceRecords";
    }
}
