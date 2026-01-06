using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.Shared.Models;
using System.Security.Claims;
using System.Text.Json;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectTasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProjectTasksController> _logger;

        public ProjectTasksController(AppDbContext context, ILogger<ProjectTasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ProjectTasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectTask>>> GetProjectTasks(Guid? projectId = null)
        {
            try
            {
                var query = _context.ProjectTasks
                    .Include(t => t.Assignments)
                    .Include(t => t.Comments)
                    .AsNoTracking()
                    .AsQueryable();

                if (projectId.HasValue)
                {
                    query = query.Where(t => t.ProjectId == projectId.Value);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/ProjectTasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectTask>> GetProjectTask(Guid id)
        {
            try
            {
                var task = await _context.ProjectTasks
                    .Include(t => t.Assignments)
                    .Include(t => t.Comments)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    return NotFound();
                }

                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/ProjectTasks
        [HttpPost]
        public async Task<ActionResult<ProjectTask>> PostProjectTask(ProjectTask task)
        {
            try
            {
                if (task.Id == Guid.Empty) task.Id = Guid.NewGuid();
                TaskHelper.EnsureUtcDates(task);

                _context.ProjectTasks.Add(task);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetProjectTask", new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/ProjectTasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProjectTask(Guid id, ProjectTask task)
        {
            if (id != task.Id) return BadRequest();

            TaskHelper.EnsureUtcDates(task);
            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectTaskExists(id)) return NotFound();
                else throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {Id}", id);
                return StatusCode(500, "Internal server error");
            }

            return NoContent();
        }

        // DELETE: api/ProjectTasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectTask(Guid id)
        {
            try
            {
                var task = await _context.ProjectTasks.FindAsync(id);
                if (task == null) return NotFound();

                _context.ProjectTasks.Remove(task);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        
        // Helper to check existence
        private bool ProjectTaskExists(Guid id) => _context.ProjectTasks.Any(e => e.Id == id);
    }
    
    public static class TaskHelper
    {
        public static void EnsureUtcDates(ProjectTask task)
        {
            // EF Core on SQL Server can be picky with DateTimes if they are Unspecified or Local when storing to datetime2
            // Best practice is to ensure UTC or Unspecified that is treated as UTC
            if (task.StartDate.Kind == DateTimeKind.Local) task.StartDate = task.StartDate.ToUniversalTime();
            if (task.FinishDate.Kind == DateTimeKind.Local) task.FinishDate = task.FinishDate.ToUniversalTime();
        }
    }
}
