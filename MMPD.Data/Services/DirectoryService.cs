// Replace these methods in your DirectoryService with the corrected versions:

using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Models;

public class DirectoryService : IDirectoryService
{
    private readonly AppDbContext _context;
    public DirectoryService(AppDbContext context)
    {
        _context = context;
    }

    // =================================================================
    // FIXED METHODS - Now properly filter by Active status
    // =================================================================

    /// <summary>
    /// Gets location with its active departments only
    /// </summary>
    public async Task<Location?> GetLocationWithDepartmentsAsync(int locationId) =>
        await _context.Locations
            .Where(l => l.Active == true)
            .Include(l => l.Departments.Where(d => d.Active == true)) // Filter departments by Active
            .FirstOrDefaultAsync(l => l.Id == locationId);

    /// <summary>
    /// Gets department with its active employees only
    /// </summary>
    public async Task<Department?> GetDepartmentWithEmployeesAsync(int departmentId) =>
        await _context.Departments
            .Where(d => d.Active == true)
            .Include(d => d.Employees.Where(e => e.Active == true)) // Filter employees by Active
            .FirstOrDefaultAsync(d => d.Id == departmentId);

    /// <summary>
    /// Gets employee by ID (only if active)
    /// </summary>
    public async Task<Employee?> GetEmployeeByIdAsync(int employeeId) =>
        await _context.Employees
            .Where(e => e.Active == true) // Add Active filter
            .FirstOrDefaultAsync(e => e.Id == employeeId);

    /// <summary>
    /// Gets active employees by department
    /// </summary>
    public async Task<List<Employee>> GetEmployeesByDepartmentAsync(int departmentId) =>
        await _context.Employees
            .Where(e => e.Active == true) // Add Active filter
            .Where(e => e.Department == departmentId)
            .ToListAsync();

    /// <summary>
    /// Gets all active departments
    /// </summary>
    public async Task<List<Department>> GetDepartmentsAsync() =>
        await _context.Departments
            .Where(d => d.Active == true)
            .Include(d => d.DeptLocation)
            .ToListAsync();

    /// <summary>
    /// Gets active department by ID
    /// </summary>
    public async Task<Department?> GetDepartmentByIdAsync(int id) =>
        await _context.Departments
            .Where(d => d.Active == true)
            .Include(d => d.DeptLocation)
            .FirstOrDefaultAsync(d => d.Id == id);

    /// <summary>
    /// Gets active locations by type
    /// </summary>
    public async Task<List<Location>> GetLocationsByTypeAsync(string loctypeName)
    {
        return await _context.Locations
            .Where(l => l.Active == true)
            .Include(l => l.LocationType)
            .Where(l => l.LocationType!.LoctypeName.ToLower() == loctypeName.ToLower())
            .ToListAsync();
    }

    /// <summary>
    /// Gets active location by type and ID
    /// </summary>
    public async Task<Location?> GetLocationByIdAsync(string loctypeName, int id)
    {
        return await _context.Locations
            .Where(l => l.Active == true)
            .Include(l => l.LocationType)
            .Where(l => l.LocationType!.LoctypeName.ToLower() == loctypeName.ToLower() && l.Id == id)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all location types (no Active filter needed)
    /// </summary>
    public async Task<List<Loctype>> GetLoctypesAsync()
    {
        return await _context.Loctypes.ToListAsync();
    }

    /// <summary>
    /// Gets all active locations
    /// </summary>
    public async Task<List<Location>> GetLocationsAsync()
    {
        return await _context.Locations
            .Where(l => l.Active == true)
            .Include(l => l.LocationType)
            .ToListAsync();
    }

    /// <summary>
    /// Gets active location by ID
    /// </summary>
    public async Task<Location?> GetLocationByIdAsync(int id)
    {
        return await _context.Locations
            .Where(l => l.Active == true)
            .Include(l => l.LocationType)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    /// <summary>
    /// Gets all active employees
    /// </summary>
    public async Task<List<Employee>> GetEmployeesAsync()
    {
        return await _context.Employees
            .Where(e => e.Active == true)
            .Include(e => e.EmpDepartment)
            .Include(e => e.EmpLocation)
            .ToListAsync();
    }

    /// <summary>
    /// Gets active employees by location
    /// </summary>
    public async Task<List<Employee>> GetEmployeesByLocationAsync(int locationId)
    {
        return await _context.Employees
            .Where(e => e.Active == true)
            .Where(e => e.Location == locationId)
            .ToListAsync();
    }

    /// <summary>
    /// Gets active employees by location and department
    /// </summary>
    public async Task<List<Employee>> GetEmployeesByLocationAndDepartmentAsync(int locationId, int departmentId)
    {
        return await _context.Employees
            .Where(e => e.Active == true)
            .Where(e => e.Location == locationId && e.Department == departmentId)
            .ToListAsync();
    }
}