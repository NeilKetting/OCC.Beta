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
    public class TaskCommentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TaskCommentsController> _logger;

        public TaskCommentsController(AppDbContext context, ILogger<TaskCommentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskComment>>> GetTaskComments(Guid? taskId = null)
        {
            try
            {
                var query = _context.TaskComments.AsQueryable();
                if (taskId.HasValue) query = query.Where(c => c.TaskId == taskId.Value);
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskComment>> GetTaskComment(Guid id)
        {
            var comment = await _context.TaskComments.FindAsync(id);
            if (comment == null) return NotFound();
            return comment;
        }

        [HttpPost]
        public async Task<ActionResult<TaskComment>> PostTaskComment(TaskComment comment)
        {
            try
            {
                if (comment.Id == Guid.Empty) comment.Id = Guid.NewGuid();
                comment.CreatedAt = DateTime.UtcNow; 
                _context.TaskComments.Add(comment);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetTaskComment", new { id = comment.Id }, comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskComment(Guid id)
        {
            try
            {
                var comment = await _context.TaskComments.FindAsync(id);
                if (comment == null) return NotFound();
                _context.TaskComments.Remove(comment);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
