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

            _context.Entry(audit).State = EntityState.Modified;

            // Simple update, deeper graph updates might require custom logic
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
                .Where(i => i.AuditId == id && !i.IsDeleted)
                .ToListAsync();
             return items;
        }
    }
}
