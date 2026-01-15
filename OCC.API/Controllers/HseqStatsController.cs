using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.Shared.Models;
using OCC.Shared.DTOs; // We might need a DTO for stats

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HseqStatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HseqStatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<object>> GetDashboardStats()
        {
            // 1. Total Safe Man Hours (From Attendance)
            // Naive calc: Sum of hoursWorked in AttendanceRecords for current month/year?
            // Or total cumulative? Usually cumulative for Safe Hours.
            var totalHours = await _context.AttendanceRecords
                .Where(a => !a.IsDeleted)
                .SumAsync(a => a.HoursWorked);

            // 2. Incident Counts
            var incidents = await _context.Incidents
                .Where(i => !i.IsDeleted)
                .ToListAsync(); // Pull into mem for grouping

            var incidentsCount = incidents.Count;
            var nearMisses = incidents.Count(i => i.Type == Shared.Enums.IncidentType.NearMiss);
            var injuries = incidents.Count(i => i.Type == Shared.Enums.IncidentType.Injury);
            
            // 3. Audits
            var audits = await _context.HseqAudits
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.Date)
                .Take(5)
                .ToListAsync();
            
            var auditScores = audits.Select(a => new { a.SiteName, a.ActualScore, a.Date }).ToList();

            return Ok(new 
            {
                TotalSafeHours = totalHours, // Ignoring 'lost time' subtraction for MVP unless simpler
                IncidentsTotal = incidentsCount,
                NearMisses = nearMisses,
                Injuries = injuries,
                Environmentals = incidents.Count(i => i.Type == Shared.Enums.IncidentType.Environmental),
                RecentAuditScores = auditScores
            });
        }
    }
}
