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
    public class DepartmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Departments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments([FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            return await _context.Departments
                .Include(d => d.DeptLocation) // Include location info
                .Include(d => d.Employees.Where(e => e.Active == true)) // Include active employees
                .Where(d => d.Active == true) // Only active departments
                .OrderBy(d => d.DeptName)
                .ToListAsync();
        }

        // GET: api/Departments/5
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

        // GET: api/Departments/location/5
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

        // GET: api/Departments/5/employees
        [HttpGet("{id}/employees")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetDepartmentEmployees(int id, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            var department = await _context.Departments.FindAsync(id);
            if (department == null || department.Active != true)
            {
                return NotFound();
            }

            return await _context.Employees
                .Include(e => e.EmpLocation)
                .Include(e => e.EmpDepartment)
                .Where(e => e.Department == id && e.Active == true)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync();
        }

        // PUT: api/Departments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartment(int id, Department department, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            if (id != department.Id)
            {
                return BadRequest("ID mismatch");
            }

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

        // POST: api/Departments
        [HttpPost]
        public async Task<ActionResult<Department>> PostDepartment(Department department, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            // Set defaults for new department
            department.Active = true;
            department.RecordAdd = DateTime.UtcNow;

            // Validate location exists
            var locationExists = await _context.Locations.AnyAsync(l => l.Id == department.Location && l.Active == true);
            if (!locationExists)
            {
                return BadRequest("Invalid location ID or location is inactive");
            }

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDepartment", new { id = department.Id, apiKey }, department);
        }

        // DELETE: api/Departments/5 (Soft Delete)
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

            // Check if department has active employees
            var hasActiveEmployees = department.Employees.Any(e => e.Active == true);
            if (hasActiveEmployees)
            {
                return BadRequest("Cannot delete department with active employees. Please reassign or deactivate employees first.");
            }

            // Soft delete - set Active to false
            department.Active = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Departments/5/restore
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

            // Ensure the location is still active
            var location = await _context.Locations.FindAsync(department.Location);
            if (location == null || location.Active != true)
            {
                return BadRequest("Cannot restore department because its location is inactive");
            }

            department.Active = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
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