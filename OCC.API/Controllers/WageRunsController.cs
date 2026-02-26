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
                PayType = request.PayType,
                Notes = request.Notes
            };

            // 2. Fetch Active Staff matching the PayType
            var rateType = RateType.Hourly; // Default
            if (!string.IsNullOrEmpty(request.PayType) && Enum.TryParse<RateType>(request.PayType, out var parsedType))
            {
                rateType = parsedType;
            }

            var employees = await _context.Employees
                .Where(e => e.Status == EmployeeStatus.Active && e.RateType == rateType)
                .ToListAsync();

            // 3. Fetch Attendance for the Period (up to RunDate)
            var attendance = await _context.AttendanceRecords
                .Where(a => a.Date >= request.StartDate && a.Date <= runDate)
                .ToListAsync();

            // 4. Fetch Active Loans
            var activeLoans = await _context.EmployeeLoans
                .Where(l => l.IsActive && l.OutstandingBalance > 0 && l.StartDate <= runDate)
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
                    line.Overtime15Hours += hours.Overtime15;
                    line.Overtime20Hours += hours.Overtime20;
                    line.LunchDeductionHours += hours.Lunch;
                    
                    if (record.Status == AttendanceStatus.Absent)
                    {
                        line.VarianceNotes += $"{record.Date:dd/MM}: Absent; ";
                    }
                }

                // B. Calculate Projected Hours (RunDate+1 -> EndDate)
                var projectedStart = runDate.AddDays(1);
                var projectedEnd = request.EndDate;

                if (projectedStart <= projectedEnd)
                {
                    for (var d = projectedStart; d <= projectedEnd; d = d.AddDays(1))
                    {
                        var dow = d.DayOfWeek;
                        // Skip Weekend or Public Holiday
                        if (dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday || OCC.Shared.Utils.HolidayUtils.IsPublicHoliday(d)) continue;

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
                                            a.Date <= lastRunProjectedEnd)
                                .ToListAsync();

                            double actualHoursInProjectedWindow = 0;
                            foreach(var r in pastActualRecords)
                            {
                                var h = CalculateHours(r, emp);
                                actualHoursInProjectedWindow += (h.Normal + h.Overtime15 + h.Overtime20); // Sum all valid work
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
                // Formula: ((Normal + Projected + Variance) * Rate) + (OT15 * Rate * 1.5) + (OT20 * Rate * 2.0)
                
                line.TotalWage = (decimal)(line.NormalHours + line.ProjectedHours + line.VarianceHours) * line.HourlyRate +
                                 (decimal)line.Overtime15Hours * line.HourlyRate * 1.5m +
                                 (decimal)line.Overtime20Hours * line.HourlyRate * 2.0m;
                    
                // E. Loans
                var empLoans = activeLoans.Where(l => l.EmployeeId == emp.Id).ToList();
                decimal totalLoanDeduction = 0;
                foreach (var loan in empLoans)
                {
                   var deduction = loan.MonthlyInstallment;
                   if (deduction > loan.OutstandingBalance) deduction = loan.OutstandingBalance;
                   totalLoanDeduction += deduction;
                }
                line.DeductionLoan = totalLoanDeduction;

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
        private (double Normal, double Overtime15, double Overtime20, double Lunch) CalculateHours(AttendanceRecord r, Employee e)
        {
            if (r.CheckInTime == null || r.Status == AttendanceStatus.Absent) return (0, 0, 0, 0);
            
            DateTime start = r.CheckInTime.Value;
            DateTime end = r.CheckOutTime ?? r.CheckInTime.Value; 
            if (r.CheckOutTime == null) return (0, 0, 0, 0);

            // 1. Calculate Standard Lunch Deduction (12:00 - 13:00)
            double lunchDeduction = 0;
            DateTime lunchStart = r.Date.Date.AddHours(12);
            DateTime lunchEnd = r.Date.Date.AddHours(13);

            // Find intersection of [start, end] and [lunchStart, lunchEnd]
            var intersectStart = start > lunchStart ? start : lunchStart;
            var intersectEnd = end < lunchEnd ? end : lunchEnd;

            if (intersectStart < intersectEnd)
            {
                lunchDeduction = (intersectEnd - intersectStart).TotalHours;
            }

            var totalDuration = (end - start).TotalHours;
            if (totalDuration <= 0) return (0, 0, 0, 0);

            // Determine Multiplier
            var dow = r.Date.DayOfWeek;
            bool isSunday = dow == DayOfWeek.Sunday;
            bool isSaturday = dow == DayOfWeek.Saturday;
            bool isHoliday = OCC.Shared.Utils.HolidayUtils.IsPublicHoliday(r.Date);
            
            if (isSunday || isHoliday)
            {
                // Lunch deduction applies as per "not paying for those"
                return (0, 0, totalDuration - lunchDeduction, lunchDeduction);
            }
            
            if (isSaturday)
            {
                return (0, totalDuration - lunchDeduction, 0, lunchDeduction);
            }
            
            // Weekday: Check Shift Bounds
            TimeSpan shiftStart = e.ShiftStartTime ?? new TimeSpan(7, 0, 0);
            TimeSpan shiftEnd = e.ShiftEndTime ?? new TimeSpan(16, 0, 0);
            
            DateTime shiftStartDt = r.Date.Date.Add(shiftStart);
            DateTime shiftEndDt = r.Date.Date.Add(shiftEnd);
            
            // Normal (Shift Overlap)
            var overlapStart = start > shiftStartDt ? start : shiftStartDt;
            var overlapEnd = end < shiftEndDt ? end : shiftEndDt;
            
            double normal = 0;
            if (overlapStart < overlapEnd)
            {
                normal = (overlapEnd - overlapStart).TotalHours;
                
                // Deduct lunch from normal hours if it overlaps the shift
                var normalLunchStart = overlapStart > lunchStart ? overlapStart : lunchStart;
                var normalLunchEnd = overlapEnd < lunchEnd ? overlapEnd : lunchEnd;
                
                if (normalLunchStart < normalLunchEnd)
                {
                    normal -= (normalLunchEnd - normalLunchStart).TotalHours;
                }
            }
            
            double totalOT = totalDuration - (normal + lunchDeduction);
            if (totalOT < 0) totalOT = 0; 
            
            return (normal, totalOT, 0, lunchDeduction);
        }
    }
}
