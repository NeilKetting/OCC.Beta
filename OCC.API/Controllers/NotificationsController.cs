using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.Shared.Models;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(AppDbContext context, ILogger<NotificationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Notifications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            try
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out var userId))
                {
                    return Unauthorized("User ID not found in claims.");
                }

                return await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.Timestamp)
                    .Take(50) // Limit to last 50
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Notifications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotification(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            return notification;
        }

        // POST: api/Notifications
        [HttpPost]
        public async Task<ActionResult<Notification>> PostNotification(Notification notification)
        {
            try
            {
                if (notification.Id == Guid.Empty) notification.Id = Guid.NewGuid();
                notification.Timestamp = DateTime.UtcNow;
                
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetNotification", new { id = notification.Id }, notification);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error creating notification");
                 return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Notifications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
             try
            {
                var notification = await _context.Notifications.FindAsync(id);
                if (notification == null) return NotFound();

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                return NoContent();
            }
             catch (Exception ex)
            {
                 _logger.LogError(ex, "Error deleting notification {Id}", id);
                 return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Notifications/5/Read
        [HttpPut("{id}/Read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
             try
            {
                var notification = await _context.Notifications.FindAsync(id);
                if (notification == null) return NotFound();

                notification.IsRead = true;
                await _context.SaveChangesAsync();

                return NoContent();
            }
             catch (Exception ex)
            {
                 _logger.LogError(ex, "Error marking notification as read {Id}", id);
                 return StatusCode(500, "Internal server error");
            }
        }
    }
}
