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
using System.Security.Claims;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BugReportsController : ControllerBase
    {
        #region Fields & Constructor

        private readonly AppDbContext _context;

        public BugReportsController(AppDbContext context)
        {
            _context = context;
        }

        #endregion

        #region Public Methods

        // GET: api/BugReports
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BugReport>>> GetBugReports()
        {
            if (!HasViewAccess(out bool isDev))
            {
                return Forbid();
            }

            var query = _context.BugReports.Include(b => b.Comments).AsQueryable();

            // Normal users don't see Closed bugs to keep list clean
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
        [Authorize]
        public async Task<ActionResult<BugReport>> GetBugReport(Guid id)
        {
            if (!HasViewAccess(out _)) return Forbid();

            var bugReport = await _context.BugReports.FindAsync(id);

            if (bugReport == null)
            {
                return NotFound();
            }

            return bugReport;
        }

        // POST: api/BugReports
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BugReport>> PostBugReport(BugReport bugReport)
        {
            if (!HasViewAccess(out _)) return Forbid("Only Admin, Office, and Site Managers can report bugs.");

            bugReport.ReportedDate = DateTime.UtcNow;
            
            // Ensure ID is set
            if (bugReport.Id == Guid.Empty) bugReport.Id = Guid.NewGuid();

            _context.BugReports.Add(bugReport);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBugReport", new { id = bugReport.Id }, bugReport);
        }

        // POST: api/BugReports/5/comments
        [HttpPost("{id}/comments")]
        [Authorize]
        public async Task<ActionResult<BugComment>> PostBugComment(Guid id, [FromQuery] string? status, BugComment comment)
        {
            // REPLY RESTRICTION: Only Neil can reply
            if (!IsNeilDev())
            {
                return Forbid("Only the Developer (Neil) can comment on or update bug reports.");
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

        #endregion

        #region Helpers

        private bool IsNeilDev()
        {
            // Check standard ClaimTypes.Email
            var email = User.FindFirst(ClaimTypes.Email)?.Value?.ToLowerInvariant();
            if (email == "neil@mdk.co.za") return true;

            // Check "email" claim fallback (common in some JWT configurations)
            var emailFallback = User.FindFirst("email")?.Value?.ToLowerInvariant();
            if (emailFallback == "neil@mdk.co.za") return true;

            // Check Name claim as fallback
            var nameEmail = User.FindFirst(ClaimTypes.Name)?.Value?.ToLowerInvariant();
            if (nameEmail == "neil@mdk.co.za") return true;

            return false;
        }

        private bool HasViewAccess(out bool isDev)
        {
            isDev = IsNeilDev();
            if (isDev) return true;

            // Check Roles for View/Create access
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            // Fallback for "role" claim
            if (string.IsNullOrEmpty(roleClaim))
            {
                roleClaim = User.FindFirst("role")?.Value;
            }

            if (string.IsNullOrEmpty(roleClaim)) return false;

            // Allow: Admin, Office, SiteManager
            var isAdmin = roleClaim == nameof(UserRole.Admin) || roleClaim == "0";
            var isOffice = roleClaim == nameof(UserRole.Office) || roleClaim == "1";
            var isSiteManager = roleClaim == nameof(UserRole.SiteManager) || roleClaim == "2";
            
            return isAdmin || isOffice || isSiteManager;
        }

        private bool BugReportExists(Guid id)
        {
            return _context.BugReports.Any(e => e.Id == id);
        }

        #endregion
    }
}
