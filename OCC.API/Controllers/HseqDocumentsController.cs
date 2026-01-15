using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HseqDocumentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HseqDocumentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HseqDocument>>> GetDocuments()
        {
            return await _context.HseqDocuments.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<HseqDocument>> UploadDocument(HseqDocument document)
        {
            // In a real app, we would handle the file upload separately or here if included in the request model
            // For now, we are saving the metadata record.
            
            document.Id = Guid.NewGuid();
            document.UploadDate = DateTime.UtcNow;
            
            _context.HseqDocuments.Add(document);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDocuments", new { id = document.Id }, document);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            var document = await _context.HseqDocuments.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            _context.HseqDocuments.Remove(document);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
