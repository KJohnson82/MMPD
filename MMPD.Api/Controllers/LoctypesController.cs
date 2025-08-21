using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Models;

namespace MMPD.Api.Controllers
{
    /// <summary>
    /// API controller for managing Location Type entities.
    /// This controller is primarily for administrative purposes to manage the available types of locations.
    /// It provides basic CRUD operations and does not require an API key, assuming it's for internal or trusted use.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LoctypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the LoctypesController.
        /// </summary>
        /// <param name="context">The database context injected for data access.</param>
        public LoctypesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/Loctypes
        /// Retrieves a list of all location types.
        /// </summary>
        /// <returns>A list of all Loctype objects.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loctype>>> GetLoctypes()
        {
            return await _context.Loctypes.ToListAsync();
        }

        /// <summary>
        /// GET: api/Loctypes/5
        /// Retrieves a specific location type by its ID.
        /// </summary>
        /// <param name="id">The ID of the location type to retrieve.</param>
        /// <returns>The requested Loctype or a 404 Not Found response.</returns>
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

        /// <summary>
        /// PUT: api/Loctypes/5
        /// Updates an existing location type.
        /// </summary>
        /// <param name="id">The ID of the location type to update.</param>
        /// <param name="loctype">The updated location type object.</param>
        /// <returns>A 204 No Content response on success.</returns>
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

        /// <summary>
        /// POST: api/Loctypes
        /// Creates a new location type.
        /// </summary>
        /// <param name="loctype">The location type object to create.</param>
        /// <returns>The newly created location type with a 201 Created status.</returns>
        [HttpPost]
        public async Task<ActionResult<Loctype>> PostLoctype(Loctype loctype)
        {
            _context.Loctypes.Add(loctype);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLoctype", new { id = loctype.Id }, loctype);
        }

        /// <summary>
        /// DELETE: api/Loctypes/5
        /// Deletes a location type. Note: This is a hard delete.
        /// </summary>
        /// <param name="id">The ID of the location type to delete.</param>
        /// <returns>A 204 No Content response on success.</returns>
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

        /// <summary>
        /// Checks if a location type with the specified ID exists.
        /// </summary>
        /// <param name="id">The ID of the location type to check.</param>
        /// <returns>True if the location type exists, otherwise false.</returns>
        private bool LoctypeExists(int id)
        {
            return _context.Loctypes.Any(e => e.Id == id);
        }
    }
}
