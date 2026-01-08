using OCC.Shared.Models;
using OCC.Client.Services.Interfaces;

namespace OCC.Client.Services.ApiServices
{
    public class ApiAttendanceRecordRepository : BaseApiService<AttendanceRecord>
    {
        public ApiAttendanceRecordRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "AttendanceRecords";
    }
}
