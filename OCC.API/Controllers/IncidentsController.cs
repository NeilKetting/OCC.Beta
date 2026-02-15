using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.Shared.Models;
using OCC.Shared.Enums;
using OCC.Shared.DTOs;

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
            var incidents = await _context.Incidents
                .IgnoreQueryFilters() // <--- Temporary Diagnostic: Force show all
                .Include(i => i.Photos)
                .AsNoTracking()
                .OrderByDescending(i => i.Date)
                .ToListAsync();

            return Ok(incidents.Select(ToSummaryDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IncidentDto>> GetIncident(Guid id)
        {
            var incident = await _context.Incidents
                .Include(i => i.Photos)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            if (incident == null)
            {
                return NotFound();
            }

            return Ok(ToDetailDto(incident));
        }

        [HttpPost]
        public async Task<ActionResult<IncidentDto>> PostIncident(Incident incident)
        {
            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIncident", new { id = incident.Id }, ToDetailDto(incident));
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

        public class IncidentPhotoUploadRequest
        {
            [FromForm] public Guid IncidentId { get; set; }
            [FromForm] public IFormFile? File { get; set; }
            [FromForm] public string? Description { get; set; }
        }

        [HttpPost("photos")]
        public async Task<ActionResult<IncidentPhotoDto>> PostPhoto([FromForm] IncidentPhotoUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0) return BadRequest("No file uploaded.");

            var incident = await _context.Incidents.FindAsync(request.IncidentId);
            if (incident == null) return NotFound("Incident not found.");

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "incidents");
            if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}_{request.File.FileName}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            var photo = new IncidentPhoto
            {
                IncidentId = request.IncidentId,
                FileName = request.File.FileName,
                FilePath = $"/uploads/incidents/{fileName}",
                FileSize = $"{(request.File.Length / 1024.0):F2} KB",
                Description = request.Description ?? string.Empty,
                UploadedBy = User.Identity?.Name ?? "Admin",
                UploadedAt = DateTime.UtcNow
            };

            _context.IncidentPhotos.Add(photo);
            await _context.SaveChangesAsync();

            return Ok(ToPhotoDto(photo));
        }

        [HttpDelete("photos/{id}")]
        public async Task<IActionResult> DeletePhoto(Guid id)
        {
            var photo = await _context.IncidentPhotos.FindAsync(id);
            if (photo == null) return NotFound();

            // Delete physical file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", photo.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.IncidentPhotos.Remove(photo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool IncidentExists(Guid id)
        {
            return _context.Incidents.Any(e => e.Id == id);
        }

        private IncidentSummaryDto ToSummaryDto(Incident incident)
        {
            return new IncidentSummaryDto
            {
                Id = incident.Id,
                Date = incident.Date,
                Type = incident.Type,
                Severity = incident.Severity,
                Location = incident.Location,
                Status = incident.Status,
                ReportedByUserId = incident.ReportedByUserId,
                PhotoCount = incident.Photos?.Count ?? 0
            };
        }

        private IncidentDto ToDetailDto(Incident incident)
        {
            return new IncidentDto
            {
                Id = incident.Id,
                Date = incident.Date,
                Type = incident.Type,
                Severity = incident.Severity,
                Location = incident.Location,
                Description = incident.Description,
                ReportedByUserId = incident.ReportedByUserId,
                Status = incident.Status,
                InvestigatorId = incident.InvestigatorId,
                RootCause = incident.RootCause,
                CorrectiveAction = incident.CorrectiveAction,
                Photos = incident.Photos?.Select(ToPhotoDto).ToList() ?? new List<IncidentPhotoDto>()
            };
        }

        private IncidentPhotoDto ToPhotoDto(IncidentPhoto photo)
        {
            return new IncidentPhotoDto
            {
                Id = photo.Id,
                FileName = photo.FileName,
                FilePath = photo.FilePath,
                FileSize = photo.FileSize,
                UploadedBy = photo.UploadedBy,
                UploadedAt = photo.UploadedAt
            };
        }

    }
}
