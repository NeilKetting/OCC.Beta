using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.Shared.Models;
using OCC.Shared.Enums;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IncidentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Incident>>> GetIncidents()
        {
            return await _context.Incidents
                .Include(i => i.Photos)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Incident>> GetIncident(Guid id)
        {
            var incident = await _context.Incidents
                .Include(i => i.Photos)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (incident == null)
            {
                return NotFound();
            }

            return incident;
        }

        [HttpPost]
        public async Task<ActionResult<Incident>> PostIncident(Incident incident)
        {
            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIncident", new { id = incident.Id }, incident);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutIncident(Guid id, Incident incident)
        {
            if (id != incident.Id)
            {
                return BadRequest();
            }

            _context.Entry(incident).State = EntityState.Modified;
            
            // Handle Photos Update? Usually simpler to handle separately or assume full graph update if careful.
            // For now, assume incident structure includes photos.
            // But EF Core detached entities might need help.
            // Let's rely on basic update first.

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IncidentExists(id))
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIncident(Guid id)
        {
            var incident = await _context.Incidents.FindAsync(id);
            if (incident == null)
            {
                return NotFound();
            }

            _context.Incidents.Remove(incident); // Soft delete handled by context
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool IncidentExists(Guid id)
        {
            return _context.Incidents.Any(e => e.Id == id);
        }
    }
}
