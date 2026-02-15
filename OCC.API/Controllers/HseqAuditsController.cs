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
    public class HseqAuditsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HseqAuditsController> _logger;

        public HseqAuditsController(AppDbContext context, ILogger<HseqAuditsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditSummaryDto>>> GetAudits()
        {
            var audits = await _context.HseqAudits
                .AsNoTracking()
                .OrderByDescending(a => a.Date)
                .ToListAsync();
            
            return audits.Select(ToSummaryDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuditDto>> GetAudit(Guid id)
        {
            var audit = await _context.HseqAudits
                .Include(a => a.Sections)
                .Include(a => a.ComplianceItems)
                .Include(a => a.NonComplianceItems)
                    .ThenInclude(i => i.Attachments)
                .Include(a => a.Attachments)
                .AsNoTracking()
                .AsSplitQuery() // Prevent MultipleCollectionIncludeWarning
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null)
            {
                return NotFound();
            }

            return ToDetailDto(audit);
        }

        [HttpPost]
        public async Task<ActionResult<AuditDto>> PostAudit(AuditDto auditDto)
        {
            var audit = new HseqAudit
            {
                Id = auditDto.Id != Guid.Empty ? auditDto.Id : Guid.NewGuid(),
                Date = auditDto.Date,
                SiteName = auditDto.SiteName,
                ScopeOfWorks = auditDto.ScopeOfWorks,
                SiteManager = auditDto.SiteManager,
                SiteSupervisor = auditDto.SiteSupervisor,
                HseqConsultant = auditDto.HseqConsultant,
                AuditNumber = auditDto.AuditNumber,
                TargetScore = auditDto.TargetScore,
                ActualScore = auditDto.ActualScore,
                Status = auditDto.Status,
                CloseOutDate = auditDto.CloseOutDate,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            // Map Sections
            if (auditDto.Sections != null)
            {
                foreach (var s in auditDto.Sections)
                {
                    audit.Sections.Add(new HseqAuditSection
                    {
                        Id = s.Id != Guid.Empty ? s.Id : Guid.NewGuid(),
                        Name = s.Name,
                        PossibleScore = s.PossibleScore,
                        ActualScore = s.ActualScore
                    });
                }
            }

            // Map NonComplianceItems
            if (auditDto.NonComplianceItems != null)
            {
                foreach (var i in auditDto.NonComplianceItems)
                {
                    audit.NonComplianceItems.Add(new HseqAuditNonComplianceItem
                    {
                        Id = i.Id != Guid.Empty ? i.Id : Guid.NewGuid(),
                        Description = i.Description,
                        RegulationReference = i.RegulationReference,
                        CorrectiveAction = i.CorrectiveAction,
                        ResponsiblePerson = i.ResponsiblePerson,
                        TargetDate = i.TargetDate,
                        Status = i.Status,
                        ClosedDate = i.ClosedDate,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    });
                }
            }

            _context.HseqAudits.Add(audit);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAudit", new { id = audit.Id }, ToDetailDto(audit));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAudit(Guid id, AuditDto auditDto)
        {
            if (id != auditDto.Id) return BadRequest();

            // Load existing audit with children
            var existingAudit = await _context.HseqAudits
                .Include(a => a.Sections)
                .Include(a => a.NonComplianceItems)
                    .ThenInclude(i => i.Attachments)
                .Include(a => a.Attachments)
                .AsSplitQuery() // Use split query for complex graph load
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingAudit == null)
            {
                return NotFound();
            }

            // 1. Handle Collection Updates First
            // This allows triggers to fire and update the parent record's RowVersion in the DB

            // Update Sections
            var sectionIdsInDto = auditDto.Sections.Where(s => s.Id != Guid.Empty).Select(s => s.Id).ToList();
            var sectionsToRemove = existingAudit.Sections.Where(s => !sectionIdsInDto.Contains(s.Id)).ToList();
            foreach (var s in sectionsToRemove) existingAudit.Sections.Remove(s);

            foreach (var sectionDto in auditDto.Sections)
            {
                var existingSection = existingAudit.Sections.FirstOrDefault(s => s.Id == sectionDto.Id);
                if (existingSection != null)
                {
                    existingSection.ActualScore = sectionDto.ActualScore;
                    existingSection.PossibleScore = sectionDto.PossibleScore;
                    existingSection.Name = sectionDto.Name;
                    existingSection.UpdatedAtUtc = DateTime.UtcNow;

                    // Propagate RowVersion for concurrency check
                    if (sectionDto.RowVersion != null && sectionDto.RowVersion.Length > 0)
                    {
                        _context.Entry(existingSection).Property("RowVersion").OriginalValue = sectionDto.RowVersion;
                    }
                }
                else
                {
                    existingAudit.Sections.Add(new HseqAuditSection
                    {
                        Id = sectionDto.Id != Guid.Empty ? sectionDto.Id : Guid.NewGuid(),
                        Name = sectionDto.Name,
                        PossibleScore = sectionDto.PossibleScore,
                        ActualScore = sectionDto.ActualScore,
                        AuditId = existingAudit.Id,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    });
                }
            }
            
            // Update NonComplianceItems
            if (auditDto.NonComplianceItems != null)
            {
                var itemIdsInDto = auditDto.NonComplianceItems.Where(i => i.Id != Guid.Empty).Select(i => i.Id).ToList();
                var itemsToRemove = existingAudit.NonComplianceItems.Where(i => !itemIdsInDto.Contains(i.Id)).ToList();
                foreach (var i in itemsToRemove) existingAudit.NonComplianceItems.Remove(i);

                foreach (var itemDto in auditDto.NonComplianceItems)
                {
                    var existingItem = existingAudit.NonComplianceItems.FirstOrDefault(i => i.Id == itemDto.Id);
                    if (existingItem != null)
                    {
                        existingItem.Description = itemDto.Description;
                        existingItem.RegulationReference = itemDto.RegulationReference;
                        existingItem.CorrectiveAction = itemDto.CorrectiveAction;
                        existingItem.ResponsiblePerson = itemDto.ResponsiblePerson;
                        existingItem.TargetDate = itemDto.TargetDate;
                        existingItem.Status = itemDto.Status;
                        existingItem.ClosedDate = itemDto.ClosedDate;
                        existingItem.UpdatedAtUtc = DateTime.UtcNow;

                        // Propagate RowVersion for concurrency check
                        if (itemDto.RowVersion != null && itemDto.RowVersion.Length > 0)
                        {
                            _context.Entry(existingItem).Property("RowVersion").OriginalValue = itemDto.RowVersion;
                        }
                    }
                    else
                    {
                        existingAudit.NonComplianceItems.Add(new HseqAuditNonComplianceItem
                        {
                            Id = itemDto.Id != Guid.Empty ? itemDto.Id : Guid.NewGuid(),
                            Description = itemDto.Description,
                            RegulationReference = itemDto.RegulationReference,
                            CorrectiveAction = itemDto.CorrectiveAction,
                            ResponsiblePerson = itemDto.ResponsiblePerson,
                            TargetDate = itemDto.TargetDate,
                            Status = itemDto.Status,
                            ClosedDate = itemDto.ClosedDate,
                            CreatedAtUtc = DateTime.UtcNow,
                            UpdatedAtUtc = DateTime.UtcNow,
                            AuditId = existingAudit.Id
                        }); 
                    }
                }
            }
            else
            {
                existingAudit.NonComplianceItems.Clear();
            }

            // Save child changes first to let triggers update the audit row (e.g. roll up scores)
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.FirstOrDefault();
                var tableName = entry?.Metadata.GetTableName();
                var dbValues = await entry?.GetDatabaseValuesAsync();
                _logger.LogError(ex, "Concurrency conflict updating HSEQ Audit {AuditId}. Table: {TableName}, Id: {EntryId}. DB RowVersion: {DbRv}", 
                    id, tableName, entry?.Property("Id").CurrentValue, dbValues?["RowVersion"]);

                if (!_context.HseqAudits.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            // 2. Update Parent Properties
            // Reload the audit to get the LATEST RowVersion (potentially changed by triggers)
            await _context.Entry(existingAudit).ReloadAsync();

            // Apply values from DTO to re-synched entity
            existingAudit.Date = auditDto.Date;
            existingAudit.SiteName = auditDto.SiteName;
            existingAudit.SiteManager = auditDto.SiteManager;
            existingAudit.SiteSupervisor = auditDto.SiteSupervisor;
            existingAudit.HseqConsultant = auditDto.HseqConsultant;
            existingAudit.ScopeOfWorks = auditDto.ScopeOfWorks;
            existingAudit.Status = auditDto.Status;
            existingAudit.TargetScore = auditDto.TargetScore;
            existingAudit.ActualScore = auditDto.ActualScore;
            existingAudit.CloseOutDate = auditDto.CloseOutDate;
            existingAudit.UpdatedAtUtc = DateTime.UtcNow;

            // Use the client's RowVersion as the basis for the final concurrency check 
            // but we must be careful: if we reload, EF will use the NEW RowVersion from DB.
            // We WANT to ensure no OTHER user changed it between the client's load and our update.
            if (auditDto.RowVersion != null && auditDto.RowVersion.Length > 0)
            {
                _context.Entry(existingAudit).Property("RowVersion").OriginalValue = auditDto.RowVersion;
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
        public async Task<ActionResult<IEnumerable<AuditNonComplianceItemDto>>> GetAuditDeviations(Guid id)
        {
             var items = await _context.HseqAuditNonComplianceItems
                .Include(i => i.Attachments)
                .AsNoTracking()
                .Where(i => i.AuditId == id)
                .ToListAsync();
             
             return items.Select(ToNonComplianceItemDto).ToList();
        }

        #region Mapping Helpers

        private static AuditSummaryDto ToSummaryDto(HseqAudit audit)
        {
            return new AuditSummaryDto
            {
                Id = audit.Id,
                Date = audit.Date,
                SiteName = audit.SiteName,
                AuditNumber = audit.AuditNumber,
                Status = audit.Status,
                HseqConsultant = audit.HseqConsultant,
                TargetScore = audit.TargetScore,
                ActualScore = audit.ActualScore
            };
        }

        private static AuditDto ToDetailDto(HseqAudit audit)
        {
            return new AuditDto
            {
                Id = audit.Id,
                Date = audit.Date,
                SiteName = audit.SiteName,
                ScopeOfWorks = audit.ScopeOfWorks,
                SiteManager = audit.SiteManager,
                SiteSupervisor = audit.SiteSupervisor,
                HseqConsultant = audit.HseqConsultant,
                AuditNumber = audit.AuditNumber,
                TargetScore = audit.TargetScore,
                ActualScore = audit.ActualScore,
                Status = audit.Status,
                CloseOutDate = audit.CloseOutDate,
                Sections = audit.Sections.Select(s => new AuditSectionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    PossibleScore = s.PossibleScore,
                    ActualScore = s.ActualScore,
                    RowVersion = s.RowVersion ?? Array.Empty<byte>()
                }).ToList(),
                NonComplianceItems = audit.NonComplianceItems.Select(ToNonComplianceItemDto).ToList(),
                Attachments = audit.Attachments.Select(ToAttachmentDto).ToList(),
                RowVersion = audit.RowVersion ?? Array.Empty<byte>()
            };
        }

        private static AuditNonComplianceItemDto ToNonComplianceItemDto(HseqAuditNonComplianceItem item)
        {
            return new AuditNonComplianceItemDto
            {
                Id = item.Id,
                Description = item.Description,
                RegulationReference = item.RegulationReference,
                CorrectiveAction = item.CorrectiveAction,
                ResponsiblePerson = item.ResponsiblePerson,
                TargetDate = item.TargetDate,
                Status = item.Status,
                ClosedDate = item.ClosedDate,
                Attachments = item.Attachments?.Select(ToAttachmentDto).ToList() ?? new List<AuditAttachmentDto>(),
                RowVersion = item.RowVersion ?? Array.Empty<byte>()
            };
        }

        private static AuditAttachmentDto ToAttachmentDto(HseqAuditAttachment attachment)
        {
             return new AuditAttachmentDto
             {
                 Id = attachment.Id,
                 NonComplianceItemId = attachment.NonComplianceItemId,
                 FileName = attachment.FileName,
                 FilePath = attachment.FilePath,
                 FileSize = attachment.FileSize,
                 UploadedBy = attachment.UploadedBy,
                 UploadedAt = attachment.UploadedAt
             };
        }

        #endregion

        public class HseqAuditAttachmentRequest
        {
            [FromForm] public Guid AuditId { get; set; }
            [FromForm] public Guid? NonComplianceItemId { get; set; }
            [FromForm] public IFormFile? File { get; set; }
        }

        [HttpPost("attachments")]
        public async Task<ActionResult<HseqAuditAttachment>> PostAttachment([FromForm] HseqAuditAttachmentRequest request)
        {
            if (request.File == null || request.File.Length == 0) return BadRequest("No file uploaded.");

            var audit = await _context.HseqAudits.FindAsync(request.AuditId);
            if (audit == null) return NotFound("Audit not found.");

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "audits");
            if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}_{request.File.FileName}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            var attachment = new HseqAuditAttachment
            {
                AuditId = request.AuditId,
                NonComplianceItemId = request.NonComplianceItemId,
                FileName = request.File.FileName,
                FilePath = $"/uploads/audits/{fileName}",
                FileSize = $"{(request.File.Length / 1024.0):F2} KB",
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
