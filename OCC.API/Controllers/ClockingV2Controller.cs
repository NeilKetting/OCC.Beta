using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.API.Hubs;
using OCC.Shared.Models;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClockingV2Controller : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<ClockingV2Controller> _logger;

        public ClockingV2Controller(AppDbContext context, IHubContext<NotificationHub> hubContext, ILogger<ClockingV2Controller> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpPost("clock-in")]
        public async Task<IActionResult> ClockIn([FromBody] ClockingEventRequest request)
        {
            if (request == null || request.EmployeeId == Guid.Empty)
                return BadRequest("Invalid clock-in request.");

            var now = DateTime.Now;

            // 1. Create the immutable V2 Clocking Event
            var clockingEvent = new ClockingEvent
            {
                Id = Guid.NewGuid(),
                EmployeeId = request.EmployeeId,
                Timestamp = request.Timestamp ?? now,
                EventType = ClockEventType.ClockIn,
                Source = request.Source ?? "WebPortal"
            };

            _context.ClockingEvents.Add(clockingEvent);

            // 2. Dual Write: Create or Update the V2 Daily Timesheet
            var today = clockingEvent.Timestamp.Date;
            var timesheet = await _context.DailyTimesheets
                .FirstOrDefaultAsync(t => t.EmployeeId == request.EmployeeId && t.Date == today);

            if (timesheet == null)
            {
                timesheet = new DailyTimesheet
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = request.EmployeeId,
                    Date = today,
                    FirstInTime = clockingEvent.Timestamp,
                    Status = TimesheetStatus.Present,
                    CalculatedHours = 0,
                    WageEstimated = 0
                };
                _context.DailyTimesheets.Add(timesheet);
            }
            else if (timesheet.FirstInTime == null)
            {
                 timesheet.FirstInTime = clockingEvent.Timestamp;
                 timesheet.Status = TimesheetStatus.Present;
            }

            // 3. Dual Write: Create the V1 AttendanceRecord (Legacy compatibility)
            // We must create an open shift if one doesn't exist to not break the old Live Board
            var openLegacyRecord = await _context.AttendanceRecords
                .FirstOrDefaultAsync(r => r.EmployeeId == request.EmployeeId && r.Date.Date == today && r.CheckOutTime == null);

            if (openLegacyRecord == null)
            {
                var employee = await _context.Employees.FindAsync(request.EmployeeId);
                var legacyRecord = new AttendanceRecord
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = request.EmployeeId,
                    Date = today,
                    CheckInTime = clockingEvent.Timestamp,
                    ClockInTime = clockingEvent.Timestamp.TimeOfDay,
                    Status = AttendanceStatus.Present,
                    Branch = employee?.Branch ?? "Unknown",
                    CachedHourlyRate = (decimal?)(employee?.HourlyRate ?? 0)
                };
                _context.AttendanceRecords.Add(legacyRecord);
            }

            try
            {
                await _context.SaveChangesAsync();
                
                // SignalR updates for both systems
                await _hubContext.Clients.All.SendAsync("EntityUpdate", "ClockingEvent", "Create", clockingEvent.Id);
                await _hubContext.Clients.All.SendAsync("EntityUpdate", "AttendanceRecord", "Create", Guid.Empty); 

                return Ok(clockingEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing V2 clock in for Employee {EmployeeId}", request.EmployeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("clock-out")]
        public async Task<IActionResult> ClockOut([FromBody] ClockingEventRequest request)
        {
            if (request == null || request.EmployeeId == Guid.Empty)
                return BadRequest("Invalid clock-out request.");

            var now = DateTime.Now;

            // 1. Create the immutable V2 Clocking Event
            var clockingEvent = new ClockingEvent
            {
                Id = Guid.NewGuid(),
                EmployeeId = request.EmployeeId,
                Timestamp = request.Timestamp ?? now,
                EventType = ClockEventType.ClockOut,
                Source = request.Source ?? "WebPortal"
            };

            _context.ClockingEvents.Add(clockingEvent);

            // 2. Dual Write: Update the V2 Daily Timesheet
            var today = clockingEvent.Timestamp.Date;
            var timesheet = await _context.DailyTimesheets
                .OrderByDescending(t => t.Date)
                .FirstOrDefaultAsync(t => t.EmployeeId == request.EmployeeId && t.Date <= today && t.LastOutTime == null);

            if (timesheet != null)
            {
                timesheet.LastOutTime = clockingEvent.Timestamp;
                
                // Very basic hours calc - real system might subtract lunch
                if (timesheet.FirstInTime.HasValue)
                {
                     var hours = (decimal)(timesheet.LastOutTime.Value - timesheet.FirstInTime.Value).TotalHours;
                     if (hours > 5) hours -= 0.75m; // Deduct lunch if working full day
                     timesheet.CalculatedHours = Math.Max(0, Math.Round(hours, 2));
                     
                     var employee = await _context.Employees.FindAsync(request.EmployeeId);
                     if (employee != null)
                     {
                         timesheet.WageEstimated = timesheet.CalculatedHours * (decimal)employee.HourlyRate;
                     }
                }
            }

            // 3. Dual Write: Update the V1 AttendanceRecord (Legacy compatibility)
            var openLegacyRecord = await _context.AttendanceRecords
                .OrderByDescending(r => r.CheckInTime)
                .FirstOrDefaultAsync(r => r.EmployeeId == request.EmployeeId && r.CheckOutTime == null);

            if (openLegacyRecord != null)
            {
                openLegacyRecord.CheckOutTime = clockingEvent.Timestamp;
                
                if (openLegacyRecord.CheckInTime.HasValue)
                {
                    var hours = (openLegacyRecord.CheckOutTime.Value - openLegacyRecord.CheckInTime.Value).TotalHours;
                    if (hours > 5) hours -= 0.75;
                    openLegacyRecord.HoursWorked = Math.Max(0, Math.Round(hours, 2));
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                
                await _hubContext.Clients.All.SendAsync("EntityUpdate", "ClockingEvent", "Create", clockingEvent.Id);
                await _hubContext.Clients.All.SendAsync("EntityUpdate", "AttendanceRecord", "Update", openLegacyRecord?.Id ?? Guid.Empty);

                return Ok(clockingEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing V2 clock out for Employee {EmployeeId}", request.EmployeeId);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<ClockingEvent>>> GetActivePhysicalPresence()
        {
            try
            {
                var today = DateTime.Today;
                var latestEventsToday = await _context.ClockingEvents
                    .Where(e => e.Timestamp >= today)
                    .GroupBy(e => e.EmployeeId)
                    .Select(g => g.OrderByDescending(e => e.Timestamp).FirstOrDefault())
                    .ToListAsync();
                    
                var activeEvents = latestEventsToday
                    .Where(e => e != null && e.EventType == ClockEventType.ClockIn)
                    .Cast<ClockingEvent>()
                    .ToList();
                    
                return Ok(activeEvents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active presence.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("timesheets")]
        public async Task<ActionResult<IEnumerable<DailyTimesheet>>> GetDailyTimesheets([FromQuery] DateTime date)
        {
            try
            {
                var timesheets = await _context.DailyTimesheets
                    .Where(t => t.Date == date.Date)
                    .ToListAsync();

                return Ok(timesheets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily timesheets.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("timesheets/range")]
        public async Task<ActionResult<IEnumerable<DailyTimesheet>>> GetTimesheetsByRange([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            try
            {
                var timesheets = await _context.DailyTimesheets
                    .Where(t => t.Date >= start.Date && t.Date <= end.Date)
                    .ToListAsync();

                return Ok(timesheets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets by range.");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class ClockingEventRequest
    {
        public Guid EmployeeId { get; set; }
        public DateTime? Timestamp { get; set; }
        public string? Source { get; set; }
    }
}

