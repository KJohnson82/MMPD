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
    /// API controller for managing Employee entities.
    /// Provides endpoints for CRUD operations (Create, Read, Update, Delete) on employees.
    /// All endpoints require a valid API key for authorization.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the EmployeesController.
        /// </summary>
        /// <param name="context">The database context injected for data access.</param>
        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/Employees
        /// Retrieves a list of all active employees, including their department and location details.
        /// </summary>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A list of active employees.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees([FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            return await _context.Employees
                .Include(e => e.EmpDepartment) // Eagerly load related department.
                .Include(e => e.EmpLocation)   // Eagerly load related location.
                .Where(e => e.Active == true)  // Filter for active employees only.
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync();
        }

        /// <summary>
        /// GET: api/Employees/5
        /// Retrieves a specific active employee by their ID.
        /// </summary>
        /// <param name="id">The ID of the employee to retrieve.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>The requested employee or a 404 Not Found response.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            var employee = await _context.Employees
                .Include(e => e.EmpDepartment)
                .Include(e => e.EmpLocation)
                .FirstOrDefaultAsync(e => e.Id == id && e.Active == true);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        /// <summary>
        /// GET: api/Employees/department/5
        /// Retrieves all active employees for a specific department ID.
        /// </summary>
        /// <param name="departmentId">The ID of the department.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A list of employees for the specified department.</returns>
        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesByDepartment(int departmentId, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            return await _context.Employees
                .Include(e => e.EmpDepartment)
                .Include(e => e.EmpLocation)
                .Where(e => e.Department == departmentId && e.Active == true)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync();
        }

        /// <summary>
        /// GET: api/Employees/location/5
        /// Retrieves all active employees for a specific location ID.
        /// </summary>
        /// <param name="locationId">The ID of the location.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A list of employees for the specified location.</returns>
        [HttpGet("location/{locationId}")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesByLocation(int locationId, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            return await _context.Employees
                .Include(e => e.EmpDepartment)
                .Include(e => e.EmpLocation)
                .Where(e => e.Location == locationId && e.Active == true)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync();
        }

        /// <summary>
        /// PUT: api/Employees/5
        /// Updates an existing employee.
        /// </summary>
        /// <param name="id">The ID of the employee to update.</param>
        /// <param name="employee">The updated employee object.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A 204 No Content response on success.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            if (id != employee.Id)
            {
                return BadRequest("ID mismatch between route and request body.");
            }

            // Check if the employee exists before attempting to update.
            var existingEmployee = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            if (existingEmployee == null)
            {
                return NotFound();
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
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
        /// POST: api/Employees
        /// Creates a new employee.
        /// </summary>
        /// <param name="employee">The employee object to create.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>The newly created employee with a 201 Created status.</returns>
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            // Set default values for a new employee.
            employee.Active = true;
            employee.RecordAdd = DateTime.UtcNow;

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Return a 201 Created response with a link to the new resource.
            return CreatedAtAction("GetEmployee", new { id = employee.Id, apiKey }, employee);
        }

        /// <summary>
        /// DELETE: api/Employees/5 (Soft Delete)
        /// Deactivates an employee (soft delete) by setting their Active flag to false.
        /// </summary>
        /// <param name="id">The ID of the employee to deactivate.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A 204 No Content response on success.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Perform a soft delete by setting the Active flag to false.
            employee.Active = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// POST: api/Employees/5/restore
        /// Restores a soft-deleted (inactive) employee by setting their Active flag to true.
        /// </summary>
        /// <param name="id">The ID of the employee to restore.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A 204 No Content response on success.</returns>
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreEmployee(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Restore the employee by setting their Active flag back to true.
            employee.Active = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Checks if an employee with the specified ID exists in the database.
        /// </summary>
        /// <param name="id">The ID of the employee to check.</param>
        /// <returns>True if the employee exists, otherwise false.</returns>
        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
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
                "maui-app-key-2025",   // For MAUI sync
                "crud-web-app-key-2025" // For your Blazor admin app
            };

            return validKeys.Contains(apiKey);
        }
    }
}
