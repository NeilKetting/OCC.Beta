using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WageRunsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WageRunsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/WageRuns
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WageRun>>> GetWageRuns()
        {
            return await _context.WageRuns
                .Include(w => w.Lines)
                .OrderByDescending(w => w.StartDate)
                .ToListAsync();
        }

        // GET: api/WageRuns/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WageRun>> GetWageRun(Guid id)
        {
            var wageRun = await _context.WageRuns
                .Include(w => w.Lines)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (wageRun == null)
            {
                return NotFound();
            }

            return wageRun;
        }

        // POST: api/WageRuns/draft
        [HttpPost("draft")]
        public async Task<ActionResult<WageRun>> GenerateDraft([FromBody] WageRun request)
        {
            // Request contains StartDate, EndDate. RunDate is Now.
            var runDate = DateTime.Now.Date; // "Today"
            
            // 1. Create the Shell
            var draftRun = new WageRun
            {
                Id = Guid.NewGuid(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RunDate = runDate,
                Status = WageRunStatus.Draft,
                Notes = request.Notes
            };

            // 2. Fetch Active Staff
            var employees = await _context.Employees
                .Where(e => e.Status == EmployeeStatus.Active && e.RateType == RateType.Hourly)
                .ToListAsync();

            // 3. Fetch Attendance for the Period (up to RunDate)
            var attendance = await _context.AttendanceRecords
                .Where(a => a.Date >= request.StartDate && a.Date <= runDate && !a.IsDeleted)
                .ToListAsync();

            // 4. Fetch Previous Finalized Run (for Variance)
            // We assume the run strictly before this one. Or we can look for the last finalized run *per employee*.
            var lastRun = await _context.WageRuns
                .Include(w => w.Lines)
                .Where(w => w.Status == WageRunStatus.Finalized && w.EndDate < request.StartDate)
                .OrderByDescending(w => w.EndDate)
                .FirstOrDefaultAsync();

            foreach (var emp in employees)
            {
                var line = new WageRunLine
                {
                    Id = Guid.NewGuid(),
                    WageRunId = draftRun.Id,
                    EmployeeId = emp.Id,
                    EmployeeName = $"{emp.FirstName} {emp.LastName}",
                    Branch = emp.Branch,
                    HourlyRate = (decimal)emp.HourlyRate
                };

                // A. Calculate Normal & Overtime Hours (Actual)
                var empAttendance = attendance
                    .Where(a => a.EmployeeId == emp.Id)
                    .OrderBy(a => a.Date)
                    .ToList();

                foreach (var record in empAttendance)
                {
                    // Basic Calc matching ViewModel logic
                    var hours = CalculateHours(record, emp);
                    line.NormalHours += hours.Normal;
                    line.OvertimeHours += hours.Overtime;
                }

                // B. Calculate Projected Hours (RunDate+1 -> EndDate)
                var projectedStart = runDate.AddDays(1);
                var projectedEnd = request.EndDate;

                if (projectedStart <= projectedEnd)
                {
                    for (var d = projectedStart; d <= projectedEnd; d = d.AddDays(1))
                    {
                        var dow = d.DayOfWeek;
                        // Skip Weekend
                        if (dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday) continue;

                        // Add Standard Shift (e.g. 9 hours or ShiftDiff)
                        double dailyHours = 9.0;
                        if (emp.ShiftStartTime.HasValue && emp.ShiftEndTime.HasValue)
                        {
                            dailyHours = (emp.ShiftEndTime.Value - emp.ShiftStartTime.Value).TotalHours;
                            // Deduct Lunch? Simplify to 9 for now unless specified.
                            // Better: Use Shift diff.
                        }
                        line.ProjectedHours += dailyHours;
                    }
                }

                // C. Variance Calculation (Previous Run)
                if (lastRun != null)
                {
                    var lastLine = lastRun.Lines.FirstOrDefault(l => l.EmployeeId == emp.Id);
                    if (lastLine != null && lastLine.ProjectedHours > 0)
                    {
                        // Check what ACTUALLY happened in that window
                        // Last Run Projected Window = (LastRun.RunDate + 1) -> LastRun.EndDate
                        var lastRunProjectedStart = lastRun.RunDate.AddDays(1);
                        var lastRunProjectedEnd = lastRun.EndDate;

                        if (lastRunProjectedStart <= lastRunProjectedEnd)
                        {
                            // Fetch actual records for that past window
                            var pastActualRecords = await _context.AttendanceRecords
                                .Where(a => a.EmployeeId == emp.Id && 
                                            a.Date >= lastRunProjectedStart && 
                                            a.Date <= lastRunProjectedEnd &&
                                            !a.IsDeleted)
                                .ToListAsync();

                            double actualHoursInProjectedWindow = 0;
                            foreach(var r in pastActualRecords)
                            {
                                var h = CalculateHours(r, emp);
                                actualHoursInProjectedWindow += (h.Normal + h.Overtime); // Sum all valid work
                            }

                            // Variance = Actual - Projected
                            // Example: Paid 18 (Projected). Worked 0. Variance = -18.
                            // Example: Paid 18. Worked 20 (OT). Variance = +2.
                            
                            // NOTE: We generally only project Normal hours. If they worked OT in that window, we pay it now?
                            // Yes, Variance captures the difference.
                            
                            line.VarianceHours = actualHoursInProjectedWindow - lastLine.ProjectedHours;
                            if (Math.Abs(line.VarianceHours) > 0.01)
                            {
                                line.VarianceNotes = $"Adj from {lastRun.EndDate:MMM dd}: Paid {lastLine.ProjectedHours:F1}, Wrked {actualHoursInProjectedWindow:F1}";
                            }
                        }
                    }
                }

                // D. Total Wage
                // Simple Multiplier: (Normal + Variance + Projected) * Rate + (Overtime * Rate * 1.5)
                // Note: Variance might be negative, so it reduces Normal pay.
                // Note: Overtime Rate logic (1.5x vs 2.0x) is simplified here. Ideally we store Weighted OT Hours.
                // Let's assume input 'OvertimeHours' is "Normal Equivalent"? No, usually it's "Clock Hours".
                // We need to apply the specific multipliers.
                
                // Refinment: CalculateHours returns "CostAwareHours"?
                // Let's keep it simple: Normal * 1.0, and we track OT separately.
                // But OT has different rates (1.5 Sat, 2.0 Sun).
                // API should probably return Weighted Hours?
                
                // Correction: The stored OvertimeHours in the Line should probably be "Weighted Overtime Hours" to make the math easy?
                // Or we separate OT1.5 and OT2.0.
                // For MVP, let's convert everything to "Normal Hours Equivalent" for the TotalWage calculation?
                // No, UI needs to show "OT Hours: 5" not "7.5".
                
                // Let's approximate: OT is 1.5x average.
                decimal otMultiplier = 1.5m;
                
                line.TotalWage = (decimal)line.NormalHours * line.HourlyRate +
                                 (decimal)line.ProjectedHours * line.HourlyRate +
                                 (decimal)line.VarianceHours * line.HourlyRate +
                                 (decimal)line.OvertimeHours * line.HourlyRate * otMultiplier;

                draftRun.Lines.Add(line);
            }

            // Save Draft to DB so we can edit/finalize it
            _context.WageRuns.Add(draftRun);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWageRun", new { id = draftRun.Id }, draftRun);
        }

        // POST: api/WageRuns/finalize/5
        [HttpPost("finalize/{id}")]
        public async Task<IActionResult> FinalizeRun(Guid id)
        {
            var run = await _context.WageRuns.FindAsync(id);
            if (run == null) return NotFound();
            
            run.Status = WageRunStatus.Finalized;
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        // POST: api/WageRuns/delete/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRun(Guid id)
        {
             var run = await _context.WageRuns.FindAsync(id);
             if (run == null) return NotFound();
             
             if (run.Status == WageRunStatus.Finalized) 
                 return BadRequest("Cannot delete a finalized run.");
                 
             _context.WageRuns.Remove(run);
             await _context.SaveChangesAsync();
             return NoContent();
        }

        // Helper: Calculate Hours
        private (double Normal, double Overtime) CalculateHours(AttendanceRecord r, Employee e)
        {
            if (r.CheckInTime == null) return (0, 0);
            
            DateTime start = r.CheckInTime.Value;
            DateTime end = r.CheckOutTime ?? r.CheckInTime.Value; // If active, 0 hours? Or 'Now'?
            if (r.CheckOutTime == null) 
            {
                 // If processing a past date that is still open, assume 0 or shift end?
                 // Safer to assume 0 until they clock out.
                 return (0, 0);
            }

            var totalDuration = (end - start).TotalHours;
            if (totalDuration <= 0) return (0, 0);

            // Determine if Multiplier Applies (Weekend/Holiday)
            var dow = r.Date.DayOfWeek;
            bool isWeekend = dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;
            // Public Holiday Check omitted for brevity (Need PublicHoliday Service/Db access)
            
            if (isWeekend)
            {
                // All hours are OT
                return (0, totalDuration);
            }
            
            // Weekday: Check Shift Bounds
            TimeSpan shiftStart = e.ShiftStartTime ?? new TimeSpan(7, 0, 0);
            TimeSpan shiftEnd = e.ShiftEndTime ?? new TimeSpan(16, 0, 0);
            
            // This is complex to exact match ViewModel.
            // Simplified: Anything > 9 hours is OT?
            // Or: Anything outside 7am-4pm is OT.
            
            // Let's use the exact VM logic logic:
            // Intersect [Start, End] with [ShiftStart, ShiftEnd] is Normal.
            // Rest is OT.
            
            // Normalize dates to same day for comparison
            DateTime shiftStartDt = r.Date.Date.Add(shiftStart);
            DateTime shiftEndDt = r.Date.Date.Add(shiftEnd);
            
            // Clamp shift end if < start (overnight?) - assume day shifts
            
            // Overlap
            var overlapStart = start > shiftStartDt ? start : shiftStartDt;
            var overlapEnd = end < shiftEndDt ? end : shiftEndDt;
            
            double normal = 0;
            if (overlapStart < overlapEnd)
            {
                normal = (overlapEnd - overlapStart).TotalHours;
            }
            
            double ot = totalDuration - normal;
            if (ot < 0) ot = 0; // Floating point safety
            
            // Hack fix: If 'Overtime' status was manually set?
            
            return (normal, ot);
        }
    }
}
