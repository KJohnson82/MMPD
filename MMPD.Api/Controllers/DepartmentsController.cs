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
    /// API controller for managing Department entities.
    /// Provides endpoints for CRUD operations (Create, Read, Update, Delete) on departments.
    /// All endpoints require a valid API key for authorization.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the DepartmentsController.
        /// </summary>
        /// <param name="context">The database context injected for data access.</param>
        public DepartmentsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/Departments
        /// Retrieves a list of all active departments, including their location and active employees.
        /// </summary>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A list of active departments.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments([FromQuery] string? apiKey = null)
        {
            // Validate the provided API key.
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            // Fetch all active departments, ordered by name.
            return await _context.Departments
                .Include(d => d.DeptLocation) // Eagerly load the related location information.
                .Include(d => d.Employees.Where(e => e.Active == true)) // Eagerly load only active employees.
                .Where(d => d.Active == true) // Filter for active departments only.
                .OrderBy(d => d.DeptName)
                .ToListAsync();
        }

        /// <summary>
        /// GET: api/Departments/5
        /// Retrieves a specific active department by its ID, including its location and active employees.
        /// </summary>
        /// <param name="id">The ID of the department to retrieve.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>The requested department or a 404 Not Found response.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            var department = await _context.Departments
                .Include(d => d.DeptLocation)
                .Include(d => d.Employees.Where(e => e.Active == true))
                .FirstOrDefaultAsync(d => d.Id == id && d.Active == true);

            if (department == null)
            {
                return NotFound();
            }

            return department;
        }

        /// <summary>
        /// GET: api/Departments/location/5
        /// Retrieves all active departments for a specific location ID.
        /// </summary>
        /// <param name="locationId">The ID of the location.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A list of departments for the specified location.</returns>
        [HttpGet("location/{locationId}")]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartmentsByLocation(int locationId, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            return await _context.Departments
                .Include(d => d.DeptLocation)
                .Include(d => d.Employees.Where(e => e.Active == true))
                .Where(d => d.Location == locationId && d.Active == true)
                .OrderBy(d => d.DeptName)
                .ToListAsync();
        }

        /// <summary>
        /// GET: api/Departments/5/employees
        /// Retrieves all active employees within a specific department.
        /// </summary>
        /// <param name="id">The ID of the department.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A list of active employees in the department.</returns>
        [HttpGet("{id}/employees")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetDepartmentEmployees(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            // First, check if the department exists and is active.
            var department = await _context.Departments.FindAsync(id);
            if (department == null || department.Active != true)
            {
                return NotFound("Department not found or is inactive.");
            }

            // If the department is valid, fetch its active employees.
            return await _context.Employees
                .Include(e => e.EmpLocation)
                .Include(e => e.EmpDepartment)
                .Where(e => e.Department == id && e.Active == true)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync();
        }

        /// <summary>
        /// PUT: api/Departments/5
        /// Updates an existing department.
        /// </summary>
        /// <param name="id">The ID of the department to update.</param>
        /// <param name="department">The updated department object.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A 204 No Content response on success.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartment(int id, Department department, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            if (id != department.Id)
            {
                return BadRequest("ID mismatch between route and request body.");
            }

            // Check if the department exists before attempting to update.
            var existingDepartment = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
            if (existingDepartment == null)
            {
                return NotFound();
            }

            _context.Entry(department).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(id))
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
        /// POST: api/Departments
        /// Creates a new department.
        /// </summary>
        /// <param name="department">The department object to create.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>The newly created department with a 201 Created status.</returns>
        [HttpPost]
        public async Task<ActionResult<Department>> PostDepartment(Department department, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            // Set default values for a new department.
            department.Active = true;
            department.RecordAdd = DateTime.UtcNow;

            // Validate that the specified location exists and is active.
            var locationExists = await _context.Locations.AnyAsync(l => l.Id == department.Location && l.Active == true);
            if (!locationExists)
            {
                return BadRequest("Invalid location ID or location is inactive.");
            }

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            // Return a 201 Created response with a link to the new resource.
            return CreatedAtAction("GetDepartment", new { id = department.Id, apiKey }, department);
        }

        /// <summary>
        /// DELETE: api/Departments/5 (Soft Delete)
        /// Deactivates a department (soft delete) if it has no active employees.
        /// </summary>
        /// <param name="id">The ID of the department to delete.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A 204 No Content response on success.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return NotFound();
            }

            // Business rule: Prevent deletion if the department contains active employees.
            var hasActiveEmployees = department.Employees.Any(e => e.Active == true);
            if (hasActiveEmployees)
            {
                return BadRequest("Cannot delete department with active employees. Please reassign or deactivate employees first.");
            }

            // Perform a soft delete by setting the Active flag to false.
            department.Active = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// POST: api/Departments/5/restore
        /// Restores a soft-deleted (inactive) department by setting its Active flag to true.
        /// </summary>
        /// <param name="id">The ID of the department to restore.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A 204 No Content response on success.</returns>
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreDepartment(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            // Business rule: A department can only be restored if its parent location is active.
            var location = await _context.Locations.FindAsync(department.Location);
            if (location == null || location.Active != true)
            {
                return BadRequest("Cannot restore department because its location is inactive.");
            }

            // Restore the department by setting its Active flag back to true.
            department.Active = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Checks if a department with the specified ID exists in the database.
        /// </summary>
        /// <param name="id">The ID of the department to check.</param>
        /// <returns>True if the department exists, otherwise false.</returns>
        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }

        /// <summary>
        /// Validates the provided API key against a list of known, valid keys.
        /// </summary>
        /// <param name="apiKey">The API key to validate.</param>
        /// <returns>True if the key is valid, otherwise false.</returns>
        private bool IsValidApiKey(string? apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                return false;

            // List of valid API keys stored in code (consider moving to configuration for production).
            var validKeys = new[]
            {
                "maui-app-key-2025",
                "crud-web-app-key-2025"
            };

            return validKeys.Contains(apiKey);
        }
    }
}
