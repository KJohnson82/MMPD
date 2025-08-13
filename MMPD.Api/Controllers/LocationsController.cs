using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Models;

namespace MMPD.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Locations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocations([FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            return await _context.Locations
                .Include(l => l.LocationType)
                .Include(l => l.Departments.Where(d => d.Active == true))
                    .ThenInclude(d => d.Employees.Where(e => e.Active == true))
                .Where(l => l.Active == true)
                .OrderBy(l => l.LocName)
                .ToListAsync();
        }

        // GET: api/Locations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Location>> GetLocation(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            var location = await _context.Locations
                .Include(l => l.LocationType)
                .Include(l => l.Departments.Where(d => d.Active == true))
                    .ThenInclude(d => d.Employees.Where(e => e.Active == true))
                .FirstOrDefaultAsync(l => l.Id == id && l.Active == true);

            if (location == null)
            {
                return NotFound();
            }

            return location;
        }

        // GET: api/Locations/type/corporate
        [HttpGet("type/{locationType}")]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocationsByType(string locationType, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            // Map string to location type ID based on your ExportData class
            var locationTypeId = locationType.ToLower() switch
            {
                "corporate" => 1,
                "metalmart" or "metal-mart" or "metal mart" => 2,
                "servicecenter" or "service-center" or "service center" => 3,
                "plant" => 4,
                _ => -1
            };

            if (locationTypeId == -1)
            {
                return BadRequest("Invalid location type. Valid types: corporate, metalmart, servicecenter, plant");
            }

            return await _context.Locations
                .Include(l => l.LocationType)
                .Include(l => l.Departments.Where(d => d.Active == true))
                    .ThenInclude(d => d.Employees.Where(e => e.Active == true))
                .Where(l => l.Loctype == locationTypeId && l.Active == true)
                .OrderBy(l => l.LocNum ?? 0)
                .ThenBy(l => l.LocName)
                .ToListAsync();
        }

        // GET: api/Locations/corporate (Matches your ExportData.FetchCorpDataAsync)
        [HttpGet("corporate")]
        public async Task<ActionResult<IEnumerable<Location>>> GetCorporateLocations([FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            return await GetLocationsByTypeId(1);
        }

        // GET: api/Locations/metalmart (Matches your ExportData.FetchMMDataAsync)
        [HttpGet("metalmart")]
        public async Task<ActionResult<IEnumerable<Location>>> GetMetalMartLocations([FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            return await GetLocationsByTypeId(2);
        }

        // GET: api/Locations/servicecenter (Matches your ExportData.FetchSCDataAsync)
        [HttpGet("servicecenter")]
        public async Task<ActionResult<IEnumerable<Location>>> GetServiceCenterLocations([FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            return await GetLocationsByTypeId(3);
        }

        // GET: api/Locations/plant (Matches your ExportData.FetchPlantDataAsync)
        [HttpGet("plant")]
        public async Task<ActionResult<IEnumerable<Location>>> GetPlantLocations([FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            return await GetLocationsByTypeId(4);
        }

        // GET: api/Locations/5/departments
        [HttpGet("{id}/departments")]
        public async Task<ActionResult<IEnumerable<Department>>> GetLocationDepartments(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            var location = await _context.Locations.FindAsync(id);
            if (location == null || location.Active != true)
            {
                return NotFound();
            }

            return await _context.Departments
                .Include(d => d.Employees.Where(e => e.Active == true))
                .Where(d => d.Location == id && d.Active == true)
                .OrderBy(d => d.DeptName)
                .ToListAsync();
        }

        // GET: api/Locations/5/employees
        [HttpGet("{id}/employees")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetLocationEmployees(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            var location = await _context.Locations.FindAsync(id);
            if (location == null || location.Active != true)
            {
                return NotFound();
            }

            return await _context.Employees
                .Include(e => e.EmpDepartment)
                .Where(e => e.Location == id && e.Active == true)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync();
        }


        // PUT: api/Locations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, Location location, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            if (id != location.Id)
            {
                return BadRequest("ID mismatch");
            }

            var existingLocation = await _context.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
            if (existingLocation == null)
            {
                return NotFound();
            }

            _context.Entry(location).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationExists(id))
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


        // POST: api/Locations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Location>> PostLocation(Location location, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            // Set defaults for new location
            location.Active = true;
            location.RecordAdd = DateTime.UtcNow;

            // Validate location type exists
            var locationTypeExists = await _context.Loctypes.AnyAsync(lt => lt.Id == location.Loctype);
            if (!locationTypeExists)
            {
                return BadRequest("Invalid location type ID");
            }

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLocation", new { id = location.Id, apiKey }, location);
        }

        // DELETE: api/Locations/5 (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            var location = await _context.Locations
                .Include(l => l.Departments)
                    .ThenInclude(d => d.Employees)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (location == null)
            {
                return NotFound();
            }

            // Check if location has active departments or employees
            var hasActiveDepartments = location.Departments.Any(d => d.Active == true);
            var hasActiveEmployees = location.Departments.Any(d => d.Employees.Any(e => e.Active == true));

            if (hasActiveDepartments || hasActiveEmployees)
            {
                return BadRequest("Cannot delete location with active departments or employees. Please reassign or deactivate them first.");
            }

            // Soft delete - set Active to false
            location.Active = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Locations/5/restore
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreLocation(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            location.Active = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<List<Location>> GetLocationsByTypeId(int locationTypeId)
        {
            return await _context.Locations
                .Include(l => l.LocationType)
                .Include(l => l.Departments.Where(d => d.Active == true))
                    .ThenInclude(d => d.Employees.Where(e => e.Active == true))
                .Where(l => l.Loctype == locationTypeId && l.Active == true)
                .OrderBy(l => l.LocNum ?? 0)
                .ThenBy(l => l.LocName)
                .ToListAsync();
        }

        private bool LocationExists(int id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }

        private bool IsValidApiKey(string? apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                return false;

            var validKeys = new[]
            {
                "maui-app-key-2025",
                "crud-web-app-key-2025"
            };

            return validKeys.Contains(apiKey);
        }
    }

}
