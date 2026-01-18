using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.Shared.Models;

namespace OCC.API.Controllers.Projects
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectVariationOrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectVariationOrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectVariationOrder>>> GetVariationOrders(Guid? projectId = null)
        {
            var query = _context.ProjectVariationOrders.AsQueryable();
            
            if (projectId.HasValue)
            {
                query = query.Where(v => v.ProjectId == projectId.Value);
            }

            return await query.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectVariationOrder>> GetVariationOrder(Guid id)
        {
            var variationOrder = await _context.ProjectVariationOrders.FindAsync(id);

            if (variationOrder == null)
            {
                return NotFound();
            }

            return variationOrder;
        }

        [HttpPost]
        public async Task<ActionResult<ProjectVariationOrder>> PostVariationOrder(ProjectVariationOrder variationOrder)
        {
            _context.ProjectVariationOrders.Add(variationOrder);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVariationOrder), new { id = variationOrder.Id }, variationOrder);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutVariationOrder(Guid id, ProjectVariationOrder variationOrder)
        {
            if (id != variationOrder.Id)
            {
                return BadRequest();
            }

            _context.Entry(variationOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VariationOrderExists(id))
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
        public async Task<IActionResult> DeleteVariationOrder(Guid id)
        {
            var variationOrder = await _context.ProjectVariationOrders.FindAsync(id);
            if (variationOrder == null)
            {
                return NotFound();
            }

            _context.ProjectVariationOrders.Remove(variationOrder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VariationOrderExists(Guid id)
        {
            return _context.ProjectVariationOrders.Any(e => e.Id == id);
        }
    }
}
