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
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees([FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            return await _context.Employees
                .Include(e => e.EmpDepartment)
                .Include(e => e.EmpLocation)
                .Where(e => e.Active == true) // Only active employees
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync();
        }

        // GET: api/Employees/5
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

        // GET: api/Employees/department/5
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

        // GET: api/Employees/location/5
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

        // PUT: api/Employees/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            if (id != employee.Id)
            {
                return BadRequest("ID mismatch");
            }

            // Ensure we don't accidentally set Active to false unless intended
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

        // POST: api/Employees
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee, [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            // Set defaults for new employee
            employee.Active = true;
            employee.RecordAdd = DateTime.UtcNow;

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmployee", new { id = employee.Id, apiKey }, employee);
        }

        // DELETE: api/Employees/5 (Soft Delete)
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

            // Soft delete - set Active to false instead of removing
            employee.Active = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Employees/5/restore (Restore soft deleted employee)
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

            employee.Active = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

        private bool IsValidApiKey(string? apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                return false;

            // Define your valid API keys
            var validKeys = new[]
            {
                "maui-app-key-2025",      // For MAUI sync
                "crud-web-app-key-2025"   // For your Blazor admin app
            };

            return validKeys.Contains(apiKey);
        }
    }
}