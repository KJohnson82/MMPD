using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Models;

namespace MMPD.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoctypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LoctypesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Loctypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loctype>>> GetLoctypes()
        {
            return await _context.Loctypes.ToListAsync();
        }

        // GET: api/Loctypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Loctype>> GetLoctype(int id)
        {
            var loctype = await _context.Loctypes.FindAsync(id);

            if (loctype == null)
            {
                return NotFound();
            }

            return loctype;
        }

        // PUT: api/Loctypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoctype(int id, Loctype loctype)
        {
            if (id != loctype.Id)
            {
                return BadRequest();
            }

            _context.Entry(loctype).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoctypeExists(id))
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

        // POST: api/Loctypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Loctype>> PostLoctype(Loctype loctype)
        {
            _context.Loctypes.Add(loctype);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLoctype", new { id = loctype.Id }, loctype);
        }

        // DELETE: api/Loctypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoctype(int id)
        {
            var loctype = await _context.Loctypes.FindAsync(id);
            if (loctype == null)
            {
                return NotFound();
            }

            _context.Loctypes.Remove(loctype);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LoctypeExists(int id)
        {
            return _context.Loctypes.Any(e => e.Id == id);
        }
    }
}
