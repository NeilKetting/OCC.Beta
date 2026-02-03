using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.Shared.Models;
using OCC.Shared.Enums;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HseqAuditsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HseqAuditsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HseqAudit>>> GetAudits()
        {
            return await _context.HseqAudits
                .Include(a => a.Sections)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HseqAudit>> GetAudit(Guid id)
        {
            var audit = await _context.HseqAudits
                .Include(a => a.Sections)
                .Include(a => a.ComplianceItems)
                .Include(a => a.NonComplianceItems)
                    .ThenInclude(i => i.Attachments)
                .Include(a => a.Attachments)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null)
            {
                return NotFound();
            }

            return audit;
        }

        [HttpPost]
        public async Task<ActionResult<HseqAudit>> PostAudit(HseqAudit audit)
        {
            _context.HseqAudits.Add(audit);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAudit", new { id = audit.Id }, audit);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAudit(Guid id, HseqAudit audit)
        {
            if (id != audit.Id) return BadRequest();

            var existingAudit = await _context.HseqAudits
                .Include(a => a.Sections)
                .Include(a => a.NonComplianceItems)
                    .ThenInclude(i => i.Attachments)
                .Include(a => a.Attachments)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingAudit == null)
            {
                return NotFound();
            }

            // Update properties
            existingAudit.Date = audit.Date;
            existingAudit.SiteName = audit.SiteName;
            existingAudit.SiteManager = audit.SiteManager;
            existingAudit.HseqConsultant = audit.HseqConsultant;
            existingAudit.ScopeOfWorks = audit.ScopeOfWorks;
            existingAudit.Status = audit.Status;
            existingAudit.TargetScore = audit.TargetScore;
            existingAudit.ActualScore = audit.ActualScore;
            
            
            existingAudit.UpdatedAtUtc = DateTime.UtcNow;
            
            // Update Sections
            foreach (var section in audit.Sections)
            {
                // Match by ID preferred, fallback to Name
                var existingSection = existingAudit.Sections
                    .FirstOrDefault(s => (section.Id != Guid.Empty && s.Id == section.Id) || s.Name == section.Name);

                if (existingSection != null)
                {
                    existingSection.ActualScore = section.ActualScore;
                    existingSection.PossibleScore = section.PossibleScore;
                }
                else
                {
                    existingAudit.Sections.Add(section);
                }
            }
            
            // Update NonComplianceItems
            if (audit.NonComplianceItems != null)
            {
                foreach (var item in audit.NonComplianceItems)
                {
                    var existingItem = existingAudit.NonComplianceItems
                        .FirstOrDefault(i => (item.Id != Guid.Empty && i.Id == item.Id));

                    if (existingItem != null)
                    {
                        existingItem.Description = item.Description;
                        existingItem.RegulationReference = item.RegulationReference;
                        existingItem.CorrectiveAction = item.CorrectiveAction;
                        existingItem.ResponsiblePerson = item.ResponsiblePerson;
                        existingItem.TargetDate = item.TargetDate;
                        existingItem.Status = item.Status;
                        existingItem.ClosedDate = item.ClosedDate;
                        existingItem.UpdatedAtUtc = DateTime.UtcNow;
                    }
                    else
                    {
                        item.AuditId = existingAudit.Id;
                        existingAudit.NonComplianceItems.Add(item);
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.HseqAudits.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // Endpoint for Deviation Report
        [HttpGet("{id}/deviations")]
        public async Task<ActionResult<IEnumerable<HseqAuditNonComplianceItem>>> GetAuditDeviations(Guid id)
        {
             var items = await _context.HseqAuditNonComplianceItems
                .Include(i => i.Attachments)
                .Where(i => i.AuditId == id)
                .ToListAsync();
             return items;
        }

        [HttpPost("attachments")]
        public async Task<ActionResult<HseqAuditAttachment>> PostAttachment([FromForm] Guid auditId, [FromForm] Guid? nonComplianceItemId, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            var audit = await _context.HseqAudits.FindAsync(auditId);
            if (audit == null) return NotFound("Audit not found.");

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "audits");
            if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new HseqAuditAttachment
            {
                AuditId = auditId,
                NonComplianceItemId = nonComplianceItemId,
                FileName = file.FileName,
                FilePath = $"/uploads/audits/{fileName}",
                FileSize = $"{(file.Length / 1024.0):F2} KB",
                UploadedBy = User.Identity?.Name ?? "Admin",
                UploadedAt = DateTime.UtcNow
            };

            _context.HseqAuditAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return Ok(attachment);
        }

        [HttpDelete("attachments/{id}")]
        public async Task<IActionResult> DeleteAttachment(Guid id)
        {
            var attachment = await _context.HseqAuditAttachments.FindAsync(id);
            if (attachment == null) return NotFound();

            // Delete physical file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.HseqAuditAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
