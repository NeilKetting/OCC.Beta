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
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(AppDbContext context, ILogger<ProjectsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            try
            {
                // Including related data might be heavy, be careful
                return await _context.Projects
                    .Include(p => p.Tasks)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(Guid id)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Tasks)
                    .ThenInclude(t => t.Assignments)
                    .Include(p => p.Tasks)
                    .ThenInclude(t => t.Comments)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return NotFound();
                }

                return project;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Projects
        [HttpPost]
        public async Task<ActionResult<Project>> PostProject(Project project)
        {
            try
            {
                if (project.Id == Guid.Empty) project.Id = Guid.NewGuid();

                _context.Projects.Add(project);

                // Audit Log handled by DbContext, but we can add specific info if needed
                // _context.AuditLogs.Add(...) 

                await _context.SaveChangesAsync();

                return CreatedAtAction("GetProject", new { id = project.Id }, project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(Guid id, Project project)
        {
            if (id != project.Id)
            {
                return BadRequest();
            }

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project {Id}", id);
                return StatusCode(500, "Internal server error");
            }

            return NoContent();
        }

        // DELETE: api/Projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            try
            {
                var project = await _context.Projects.FindAsync(id);
                if (project == null)
                {
                    return NotFound();
                }

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private bool ProjectExists(Guid id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
