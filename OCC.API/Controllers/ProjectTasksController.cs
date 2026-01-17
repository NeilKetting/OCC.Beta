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
    public class ProjectTasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<ProjectTasksController> _logger;

        public ProjectTasksController(AppDbContext context, IHubContext<NotificationHub> hubContext, ILogger<ProjectTasksController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        // GET: api/ProjectTasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectTask>>> GetProjectTasks(Guid? projectId = null, bool assignedToMe = false)
        {
            try
            {
                var query = _context.ProjectTasks
                    .Include(t => t.Assignments)
                    .Include(t => t.Comments)
                    .Include(t => t.Children)
                    .AsNoTracking()
                    .AsQueryable();

                if (projectId.HasValue)
                {
                    query = query.Where(t => t.ProjectId == projectId.Value);
                }

                if (assignedToMe)
                {
                    // 1. Get current logged-in user's ID from Claims
                    var userIdString = User.Identity?.Name;
                    if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId)) 
                        return Unauthorized();

                    // 2. Find the User record
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null) return Unauthorized();

                    // 3. Find the Employee linked to this User
                    var linkedEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.LinkedUserId == user.Id);

                    if (linkedEmployee == null)
                    {
                        // User is not linked to any Employee resource -> Show 0 tasks
                        return new List<ProjectTask>();
                    }

                    // 4. Filter tasks where this Employee is assigned
                    query = query.Where(t => t.Assignments.Any(a => a.AssigneeId == linkedEmployee.Id));
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
                    .Include(t => t.Children)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null) return NotFound();
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

                await _hubContext.Clients.All.SendAsync("EntityUpdate", "ProjectTask", "Create", task.Id);

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
        [Authorize]
        public async Task<IActionResult> PutProjectTask(Guid id, ProjectTask task)
        {
            if (id != task.Id)
            {
                return BadRequest();
            }

            var existingTask = await _context.ProjectTasks.FindAsync(id);
            if (existingTask == null)
            {
                return NotFound();
            }

            try
            {
                // DEBUG LOGGING TO AuditLog TABLE
                var logEntry = new AuditLog
                {
                    UserId = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "System",
                    TableName = "ProjectTasks",
                    RecordId = id.ToString(),
                    Action = "Update Start",
                    Timestamp = DateTime.UtcNow,
                    NewValues = $"Updating Task: {task.Name}, Percent: {task.PercentComplete}, Status: {task.Status}"
                };
                _context.AuditLogs.Add(logEntry);

                // Surgical Update: Copy scalar properties only to avoid EF navigation issues
                existingTask.Name = task.Name;
                existingTask.Description = task.Description;
                existingTask.StartDate = TaskHelper.EnsureUtc(task.StartDate);
                existingTask.FinishDate = TaskHelper.EnsureUtc(task.FinishDate);
                existingTask.ActualStartDate = TaskHelper.EnsureUtc(task.ActualStartDate);
                existingTask.ActualCompleteDate = TaskHelper.EnsureUtc(task.ActualCompleteDate);
                existingTask.PercentComplete = task.PercentComplete;
                existingTask.Priority = task.Priority;
                existingTask.Status = task.Status;
                existingTask.OriginalDuration = task.OriginalDuration;
                existingTask.RemainingDuration = task.RemainingDuration;
                existingTask.ProjectId = task.ProjectId;
                existingTask.ParentTaskId = task.ParentTaskId;
                existingTask.Cost = task.Cost;
                existingTask.Tags = task.Tags;

                // Signal automated project status if progress starts
                if (existingTask.PercentComplete > 0 && existingTask.PercentComplete < 100)
                {
                    var project = await _context.Projects.FindAsync(existingTask.ProjectId);
                    if (project != null && project.Status == ProjectStatus.Planned)
                    {
                        project.Status = ProjectStatus.InProgress;
                        _context.AuditLogs.Add(new AuditLog
                        {
                            UserId = "System",
                            TableName = "Projects",
                            RecordId = project.Id.ToString(),
                            Action = "Auto Status Update",
                            Timestamp = DateTime.UtcNow,
                            NewValues = "Project moved to InProgress because task progress started."
                        });
                        await _hubContext.Clients.All.SendAsync("EntityUpdate", "Project", "Update", project.Id);
                    }
                }

                await _context.SaveChangesAsync();

                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = "System",
                    TableName = "ProjectTasks",
                    RecordId = id.ToString(),
                    Action = "Update Success",
                    Timestamp = DateTime.UtcNow
                });

                await _hubContext.Clients.All.SendAsync("EntityUpdate", "ProjectTask", "Update", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = "System",
                    TableName = "ProjectTasks",
                    RecordId = id.ToString(),
                    Action = "Update Error",
                    Timestamp = DateTime.UtcNow,
                    NewValues = $"Error: {ex.Message} | Stack: {ex.StackTrace?.Substring(0, Math.Min(ex.StackTrace.Length, 500))}"
                });
                await _context.SaveChangesAsync();
                throw; // Rethrow to let global handler catch or return 500
            }
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

                await _hubContext.Clients.All.SendAsync("EntityUpdate", "ProjectTask", "Delete", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        
        private bool ProjectTaskExists(Guid id) => _context.ProjectTasks.Any(e => e.Id == id);
    }
    
    public static class TaskHelper
    {
        public static void EnsureUtcDates(ProjectTask task)
        {
            if (task.StartDate.Kind == DateTimeKind.Local) task.StartDate = task.StartDate.ToUniversalTime();
            if (task.FinishDate.Kind == DateTimeKind.Local) task.FinishDate = task.FinishDate.ToUniversalTime();
            
            if (task.ActualStartDate.HasValue && task.ActualStartDate.Value.Kind == DateTimeKind.Local) 
                task.ActualStartDate = task.ActualStartDate.Value.ToUniversalTime();
                
            if (task.ActualCompleteDate.HasValue && task.ActualCompleteDate.Value.Kind == DateTimeKind.Local) 
                task.ActualCompleteDate = task.ActualCompleteDate.Value.ToUniversalTime();
        }
    }
}
