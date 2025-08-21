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
    /// <summary>
    /// API controller for managing Location entities.
    /// Provides endpoints for CRUD operations and specialized queries for locations.
    /// All endpoints require a valid API key for authorization.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the LocationsController.
        /// </summary>
        /// <param name="context">The database context injected for data access.</param>
        public LocationsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/Locations
        /// Retrieves a list of all active locations, including their active departments and employees.
        /// </summary>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A list of active locations.</returns>
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

        /// <summary>
        /// GET: api/Locations/5
        /// Retrieves a specific active location by its ID, including all its active children.
        /// </summary>
        /// <param name="id">The ID of the location to retrieve.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>The requested location or a 404 Not Found response.</returns>
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

        /// <summary>
        /// GET: api/Locations/type/corporate
        /// Retrieves all active locations of a specific type (e.g., "plant", "corporate").
        /// </summary>
        /// <param name="locationType">The string representation of the location type.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A list of locations matching the specified type.</returns>
        [HttpGet("type/{locationType}")]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocationsByType(string locationType, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            // Map the incoming string to a corresponding location type ID.
            var locationTypeId = locationType.ToLower() switch
            {
                "corporate" => 1,
                "metalmart" or "metal-mart" or "metal mart" => 2,
                "servicecenter" or "service-center" or "service center" => 3,
                "plant" => 4,
                _ => -1 // Invalid type
            };

            if (locationTypeId == -1)
            {
                return BadRequest("Invalid location type. Valid types: corporate, metalmart, servicecenter, plant");
            }

            // Use the helper method to fetch data by the resolved type ID.
            return await GetLocationsByTypeId(locationTypeId);
        }

        #region Convenience Endpoints by Type
        // These endpoints provide simple, direct routes to get locations of a specific type.

        /// <summary>GET: api/Locations/corporate</summary>
        [HttpGet("corporate")]
        public async Task<ActionResult<IEnumerable<Location>>> GetCorporateLocations([FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey)) return Unauthorized("Invalid or missing API key");
            return await GetLocationsByTypeId(1);
        }

        /// <summary>GET: api/Locations/metalmart</summary>
        [HttpGet("metalmart")]
        public async Task<ActionResult<IEnumerable<Location>>> GetMetalMartLocations([FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey)) return Unauthorized("Invalid or missing API key");
            return await GetLocationsByTypeId(2);
        }

        /// <summary>GET: api/Locations/servicecenter</summary>
        [HttpGet("servicecenter")]
        public async Task<ActionResult<IEnumerable<Location>>> GetServiceCenterLocations([FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey)) return Unauthorized("Invalid or missing API key");
            return await GetLocationsByTypeId(3);
        }

        /// <summary>GET: api/Locations/plant</summary>
        [HttpGet("plant")]
        public async Task<ActionResult<IEnumerable<Location>>> GetPlantLocations([FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey)) return Unauthorized("Invalid or missing API key");
            return await GetLocationsByTypeId(4);
        }

        #endregion

        /// <summary>
        /// GET: api/Locations/5/departments
        /// Retrieves all active departments for a specific location.
        /// </summary>
        /// <param name="id">The ID of the location.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A list of active departments for the location.</returns>
        [HttpGet("{id}/departments")]
        public async Task<ActionResult<IEnumerable<Department>>> GetLocationDepartments(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            // Ensure the parent location exists and is active before fetching its children.
            var location = await _context.Locations.FindAsync(id);
            if (location == null || location.Active != true)
            {
                return NotFound("Location not found or is inactive.");
            }

            return await _context.Departments
                .Include(d => d.Employees.Where(e => e.Active == true))
                .Where(d => d.Location == id && d.Active == true)
                .OrderBy(d => d.DeptName)
                .ToListAsync();
        }

        /// <summary>
        /// GET: api/Locations/5/employees
        /// Retrieves all active employees for a specific location.
        /// </summary>
        /// <param name="id">The ID of the location.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A list of active employees for the location.</returns>
        [HttpGet("{id}/employees")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetLocationEmployees(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            var location = await _context.Locations.FindAsync(id);
            if (location == null || location.Active != true)
            {
                return NotFound("Location not found or is inactive.");
            }

            return await _context.Employees
                .Include(e => e.EmpDepartment)
                .Where(e => e.Location == id && e.Active == true)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync();
        }

        /// <summary>
        /// PUT: api/Locations/5
        /// Updates an existing location.
        /// </summary>
        /// <param name="id">The ID of the location to update.</param>
        /// <param name="location">The updated location object.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A 204 No Content response on success.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, Location location, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            if (id != location.Id)
            {
                return BadRequest("ID mismatch between route and request body.");
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

        /// <summary>
        /// POST: api/Locations
        /// Creates a new location.
        /// </summary>
        /// <param name="location">The location object to create.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>The newly created location with a 201 Created status.</returns>
        [HttpPost]
        public async Task<ActionResult<Location>> PostLocation(Location location, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            // Set default values for a new location.
            location.Active = true;
            location.RecordAdd = DateTime.UtcNow;

            // Validate that the specified location type exists.
            var locationTypeExists = await _context.Loctypes.AnyAsync(lt => lt.Id == location.Loctype);
            if (!locationTypeExists)
            {
                return BadRequest("Invalid location type ID.");
            }

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLocation", new { id = location.Id, apiKey }, location);
        }

        /// <summary>
        /// DELETE: api/Locations/5 (Soft Delete)
        /// Deactivates a location if it has no active departments or employees.
        /// </summary>
        /// <param name="id">The ID of the location to delete.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A 204 No Content response on success.</returns>
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

            // Business rule: Prevent deletion if the location contains any active children.
            var hasActiveDepartments = location.Departments.Any(d => d.Active == true);
            var hasActiveEmployees = location.Departments.Any(d => d.Employees.Any(e => e.Active == true));

            if (hasActiveDepartments || hasActiveEmployees)
            {
                return BadRequest("Cannot delete location with active departments or employees. Please reassign or deactivate them first.");
            }

            // Perform a soft delete by setting the Active flag to false.
            location.Active = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// POST: api/Locations/5/restore
        /// Restores a soft-deleted (inactive) location.
        /// </summary>
        /// <param name="id">The ID of the location to restore.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A 204 No Content response on success.</returns>
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

            // Restore the location by setting its Active flag back to true.
            location.Active = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Reusable helper method to fetch locations by their type ID.
        /// </summary>
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

        /// <summary>
        /// Checks if a location with the specified ID exists.
        /// </summary>
        private bool LocationExists(int id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }

        /// <summary>
        /// Validates the provided API key against a list of known, valid keys.
        /// </summary>
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
