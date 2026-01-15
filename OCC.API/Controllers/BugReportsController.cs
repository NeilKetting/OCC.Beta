using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.Shared.Models;
using Microsoft.AspNetCore.Authorization;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BugReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BugReportsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/BugReports
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BugReport>>> GetBugReports()
        {
            var currentUserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value?.ToLowerInvariant();
            var isDev = currentUserEmail == "neil@mdk.co.za" || currentUserEmail == "neil@origize63.co.za";

            var query = _context.BugReports.Include(b => b.Comments).AsQueryable();

            if (!isDev)
            {
                query = query.Where(b => b.Status != "Closed");
            }

            return await query
                .OrderByDescending(b => b.ReportedDate)
                .ToListAsync();
        }

        // GET: api/BugReports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BugReport>> GetBugReport(Guid id)
        {
            var bugReport = await _context.BugReports.FindAsync(id);

            if (bugReport == null)
            {
                return NotFound();
            }

            return bugReport;
        }

        // PUT: api/BugReports/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBugReport(Guid id, BugReport bugReport)
        {
            if (id != bugReport.Id)
            {
                return BadRequest();
            }

            _context.Entry(bugReport).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BugReportExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/BugReports
        [HttpPost]
        public async Task<ActionResult<BugReport>> PostBugReport(BugReport bugReport)
        {
            bugReport.ReportedDate = DateTime.UtcNow; // Ensure server time
            _context.BugReports.Add(bugReport);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBugReport", new { id = bugReport.Id }, bugReport);
        }

        // DELETE: api/BugReports/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBugReport(Guid id)
        {
            var bugReport = await _context.BugReports.FindAsync(id);
            if (bugReport == null)
            {
                return NotFound();
            }

            _context.BugReports.Remove(bugReport);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/BugReports/5/comments
        [HttpPost("{id}/comments")]
        [Authorize]
        public async Task<ActionResult<BugComment>> PostBugComment(Guid id, [FromQuery] string? status, BugComment comment)
        {
            var currentUserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value?.ToLowerInvariant();
            var isDev = currentUserEmail == "neil@mdk.co.za" || currentUserEmail == "neil@origize63.co.za";

            if (!isDev)
            {
                return Forbid("Only the Developer can comment on or update bug reports.");
            }

            var bugReport = await _context.BugReports.FindAsync(id);
            if (bugReport == null)
            {
                return NotFound();
            }

            comment.BugReportId = id;
            comment.CreatedAt = DateTime.UtcNow;
            _context.BugComments.Add(comment);

            if (!string.IsNullOrEmpty(status))
            {
                bugReport.Status = status;
                _context.Entry(bugReport).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return Ok(comment);
        }

        private bool BugReportExists(Guid id)
        {
            return _context.BugReports.Any(e => e.Id == id);
        }
    }
}
