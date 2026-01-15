using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.Shared.Models;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HseqTrainingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HseqTrainingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HseqTrainingRecord>>> GetTrainingRecords()
        {
            return await _context.HseqTrainingRecords
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.DateCompleted)
                .ToListAsync();
        }
        
        [HttpGet("expiring/{days}")]
        public async Task<ActionResult<IEnumerable<HseqTrainingRecord>>> GetExpiringTraining(int days)
        {
             var threshold = DateTime.UtcNow.AddDays(days);
             var today = DateTime.UtcNow;
             
             return await _context.HseqTrainingRecords
                .Where(t => !t.IsDeleted && t.ValidUntil.HasValue && t.ValidUntil <= threshold && t.ValidUntil >= today)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<HseqTrainingRecord>> PostTrainingRecord(HseqTrainingRecord record)
        {
            _context.HseqTrainingRecords.Add(record);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetTrainingRecords", new { id = record.Id }, record);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrainingRecord(Guid id)
        {
            var record = await _context.HseqTrainingRecords.FindAsync(id);
            if (record == null) return NotFound();
            
            record.IsDeleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
