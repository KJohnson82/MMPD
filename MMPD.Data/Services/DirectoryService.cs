
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


    public async Task<Location?> GetLocationWithDepartmentsAsync(int locationId) =>
        await _context.Locations
            .Include(l => l.Departments)
            .FirstOrDefaultAsync(l => l.Id == locationId);

    public async Task<Department?> GetDepartmentWithEmployeesAsync(int departmentId) =>
        await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == departmentId);

    public async Task<Employee?> GetEmployeeByIdAsync(int employeeId) =>
        await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);

    public async Task<List<Employee>> GetEmployeesByDepartmentAsync(int departmentId) =>
        await _context.Employees
            .Where(e => e.Department == departmentId)
            .ToListAsync();

    public async Task<List<Department>> GetDepartmentsAsync() =>
        await _context.Departments
            .Include(d => d.DeptLocation)
            .ToListAsync();

    public async Task<Department?> GetDepartmentByIdAsync(int id) =>
        await _context.Departments
            .Include(d => d.DeptLocation)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<List<Location>> GetLocationsByTypeAsync(string loctypeName)
    {
        return await _context.Locations
            .Include(l => l.LocationType)
            .Where(l => l.LocationType!.LoctypeName.ToLower() == loctypeName.ToLower())
            .ToListAsync();
    }

    public async Task<Location?> GetLocationByIdAsync(string loctypeName, int id)
    {
        return await _context.Locations
            .Include(l => l.LocationType)
            .Where(l => l.LocationType!.LoctypeName.ToLower() == loctypeName.ToLower() && l.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Loctype>> GetLoctypesAsync()
    {
        return await _context.Loctypes.ToListAsync();
    }

    public async Task<List<Location>> GetLocationsAsync()
    {
        return await _context.Locations
            .Include(l => l.LocationType)
            .ToListAsync();
    }

    public async Task<Location?> GetLocationByIdAsync(int id)
    {
        return await _context.Locations
            .Include(l => l.LocationType)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<List<Employee>> GetEmployeesAsync()
    {
        return await _context.Employees
            .Include(e => e.EmpDepartment)
            .ToListAsync();
    }

    public async Task<List<Employee>> GetEmployeesByLocationAsync(int locationId)
    {
        return await _context.Employees
            .Where(e => e.Location == locationId)
            .ToListAsync();
    }

    public async Task<List<Employee>> GetEmployeesByLocationAndDepartmentAsync(int locationId, int departmentId)
    {
        return await _context.Employees
            .Where(e => e.Location == locationId && e.Department == departmentId)
            .ToListAsync();
    }



}
