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
using Microsoft.AspNetCore.SignalR;
using OCC.API.Hubs;
// using OCC.Shared.Extensions; // Assuming extension is not available, using Substring safe check

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BugReportsController : ControllerBase
    {
        #region Fields & Constructor

        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public BugReportsController(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
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

        // DELETE: api/BugReports/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteBugReport(Guid id)
        {
            if (!IsNeilDev())
            {
                return Forbid("Only the Developer (Neil) can delete bug reports.");
            }

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
            var bugReport = await _context.BugReports.FindAsync(id);
            if (bugReport == null)
            {
                return NotFound();
            }

            // REPLY PERMISSION: Neil OR the original Reporter
            var isDev = IsNeilDev();
            var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value?.ToLowerInvariant();
            var isReporter = false;
            
            // Check if current user is the reporter
            if (bugReport.ReporterId.HasValue && User.FindFirst(ClaimTypes.NameIdentifier)?.Value == bugReport.ReporterId.ToString())
            {
                isReporter = true;
            }
            // Fallback: check by name matches? No, rely on ID or Email if stored.
            // Earlier PostBugReport set ReporterId from Auth. 
            // Also check if ReporterId matches the sub claim.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var uid) && uid == bugReport.ReporterId)
            {
                isReporter = true;
            }

            if (!isDev && !isReporter)
            {
                return Forbid("Only the Developer (Neil) or the original Reporter can comment on this bug.");
            }

            comment.BugReportId = id;
            comment.CreatedAt = DateTime.UtcNow;
            
            // Ensure Author Details
            if (string.IsNullOrEmpty(comment.AuthorName))
            {
               comment.AuthorName = User.FindFirst(ClaimTypes.Name)?.Value ?? (isDev ? "Neil Ketting" : "User");
            }
            if (string.IsNullOrEmpty(comment.AuthorEmail))
            {
                comment.AuthorEmail = currentUserEmail ?? string.Empty;
            }

            _context.BugComments.Add(comment);

            if (!string.IsNullOrEmpty(status))
            {
                // Validate Status? (Open, Resolved, Closed)
                bugReport.Status = status;
                _context.Entry(bugReport).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            // Notify...
            // If Dev replied -> Notify Reporter
            // If Reporter replied -> Notify Dev (Neil)
            
            try
            {
                string message = "";
                if (isDev && bugReport.ReporterId.HasValue)
                {
                     var desc = bugReport.Description.Length > 20 ? bugReport.Description.Substring(0, 20) : bugReport.Description;
                     message = $"Update on Bug Report: {desc}... (For: {bugReport.ReporterName})";
                }
                else if (isReporter)
                {
                     // Notify Neil
                     message = $"New Reply from {bugReport.ReporterName} on Bug Report: {bugReport.ViewName}";
                     // Since Neil might not be listening filters identically, we'll just broadcast.
                }

                if (!string.IsNullOrEmpty(message))
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending notification: {ex.Message}");
            }

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
