using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Models;
using Telerik.DataSource;
using Telerik.DataSource.Extensions;

namespace MMPD.Shared.Services;

//public class SearchService
//{
//    private readonly AppDbContext _context;

//    public SearchService(AppDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<SearchResults> SearchAllAsync(string searchTerm)
//    {
//        if (string.IsNullOrWhiteSpace(searchTerm))
//            return new SearchResults();

//        var employees = await SearchEmployeesAsync(searchTerm);
//        var departments = await SearchDepartmentsAsync(searchTerm);
//        var locations = await SearchLocationsAsync(searchTerm);

//        return new SearchResults
//        {
//            Employees = employees,
//            Departments = departments,
//            Locations = locations,
//            SearchTerm = searchTerm
//        };
//    }

//    private async Task<List<Employee>> SearchEmployeesAsync(string searchTerm)
//    {
//        var allEmployees = await _context.Employees
//            .Include(e => e.EmpDepartment)
//            .Include(e => e.EmpLocation)
//            .Where(e => e.Active == true)
//            .ToListAsync();

//        // Create Telerik DataSource request with composite filter
//        var request = new DataSourceRequest()
//        {
//            Filters = new List<IFilterDescriptor>()
//        };

//        var compositeFilter = new CompositeFilterDescriptor
//        {
//            LogicalOperator = FilterCompositionLogicalOperator.Or
//        };

//        // Add filters for all employee searchable fields
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("FirstName", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("LastName", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("JobTitle", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("PhoneNumber", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("CellNumber", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Email", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Extension", FilterOperator.Contains, searchTerm));

//        request.Filters.Add(compositeFilter);

//        // Apply Telerik filtering
//        var result = allEmployees.ToDataSourceResult(request);
//        return (result.Data as IEnumerable<Employee>)?.Take(20).ToList() ?? new List<Employee>();
//    }

//    private async Task<List<Department>> SearchDepartmentsAsync(string searchTerm)
//    {
//        var allDepartments = await _context.Departments
//            .Include(d => d.DeptLocation)
//            .Where(d => d.Active == true)
//            .ToListAsync();

//        var request = new DataSourceRequest()
//        {
//            Filters = new List<IFilterDescriptor>()
//        };

//        var compositeFilter = new CompositeFilterDescriptor
//        {
//            LogicalOperator = FilterCompositionLogicalOperator.Or
//        };

//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("DeptName", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("DeptManager", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("DeptPhone", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("DeptEmail", FilterOperator.Contains, searchTerm));

//        request.Filters.Add(compositeFilter);

//        var result = allDepartments.ToDataSourceResult(request);
//        return (result.Data as IEnumerable<Department>)?.Take(20).ToList() ?? new List<Department>();
//    }

//    private async Task<List<Location>> SearchLocationsAsync(string searchTerm)
//    {
//        var allLocations = await _context.Locations
//            .Include(l => l.LocationType)
//            .Where(l => l.Active == true)
//            .ToListAsync();

//        var request = new DataSourceRequest()
//        {
//            Filters = new List<IFilterDescriptor>()
//        };

//        var compositeFilter = new CompositeFilterDescriptor
//        {
//            LogicalOperator = FilterCompositionLogicalOperator.Or
//        };

//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("LocName", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Address", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("City", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("State", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Zipcode", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("PhoneNumber", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("AreaManager", FilterOperator.Contains, searchTerm));
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("StoreManager", FilterOperator.Contains, searchTerm));

//        // Handle LocNum as string for search
//        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("LocNum", FilterOperator.Contains, searchTerm));

//        request.Filters.Add(compositeFilter);

//        var result = allLocations.ToDataSourceResult(request);
//        return (result.Data as IEnumerable<Location>)?.Take(20).ToList() ?? new List<Location>();
//    }
//}

//public class SearchResults
//{
//    public List<Employee> Employees { get; set; } = new();
//    public List<Department> Departments { get; set; } = new();
//    public List<Location> Locations { get; set; } = new();
//    public string SearchTerm { get; set; } = "";

//    public int TotalResults => Employees.Count + Departments.Count + Locations.Count;
//    public bool HasResults => TotalResults > 0;
//}


public class SearchService
{
    private readonly AppDbContext _context;

    public SearchService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SearchResults> SearchAllAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new SearchResults();

        Console.WriteLine($"DEBUG: Starting search for term: '{searchTerm}'");

        try
        {
            var employees = await SearchEmployeesAsync(searchTerm);
            var departments = await SearchDepartmentsAsync(searchTerm);
            var locations = await SearchLocationsAsync(searchTerm);

            Console.WriteLine($"DEBUG: Found {employees.Count} employees, {departments.Count} departments, {locations.Count} locations");

            return new SearchResults
            {
                Employees = employees,
                Departments = departments,
                Locations = locations,
                SearchTerm = searchTerm
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: Search error: {ex.Message}");
            return new SearchResults { SearchTerm = searchTerm };
        }
    }

    private async Task<List<Employee>> SearchEmployeesAsync(string searchTerm)
    {
        try
        {
            Console.WriteLine($"DEBUG: Searching employees for '{searchTerm}'");

            // First, let's see if we can get any employees at all
            var allEmployees = await _context.Employees
                .Include(e => e.EmpDepartment)
                .Include(e => e.EmpLocation)
                .Where(e => e.Active == true)
                .ToListAsync();

            Console.WriteLine($"DEBUG: Total active employees in database: {allEmployees.Count}");

            if (!allEmployees.Any())
            {
                Console.WriteLine("DEBUG: No active employees found in database!");
                return new List<Employee>();
            }

            // Try simple LINQ filtering first (without Telerik)
            var filteredEmployees = allEmployees.Where(e =>
                (e.FirstName != null && e.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (e.LastName != null && e.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (e.JobTitle != null && e.JobTitle.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (e.PhoneNumber != null && e.PhoneNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (e.CellNumber != null && e.CellNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (e.Email != null && e.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (e.Extension != null && e.Extension.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            ).Take(20).ToList();

            Console.WriteLine($"DEBUG: Found {filteredEmployees.Count} matching employees using simple LINQ");

            return filteredEmployees;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: Employee search error: {ex.Message}");
            return new List<Employee>();
        }
    }

    private async Task<List<Department>> SearchDepartmentsAsync(string searchTerm)
    {
        try
        {
            Console.WriteLine($"DEBUG: Searching departments for '{searchTerm}'");

            var allDepartments = await _context.Departments
                .Include(d => d.DeptLocation)
                .Where(d => d.Active == true)
                .ToListAsync();

            Console.WriteLine($"DEBUG: Total active departments in database: {allDepartments.Count}");

            var filteredDepartments = allDepartments.Where(d =>
                (d.DeptName != null && d.DeptName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (d.DeptManager != null && d.DeptManager.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (d.DeptPhone != null && d.DeptPhone.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (d.DeptEmail != null && d.DeptEmail.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            ).Take(20).ToList();

            Console.WriteLine($"DEBUG: Found {filteredDepartments.Count} matching departments");

            return filteredDepartments;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: Department search error: {ex.Message}");
            return new List<Department>();
        }
    }

    private async Task<List<Location>> SearchLocationsAsync(string searchTerm)
    {
        try
        {
            Console.WriteLine($"DEBUG: Searching locations for '{searchTerm}'");

            var allLocations = await _context.Locations
                .Include(l => l.LocationType)
                .Where(l => l.Active == true)
                .ToListAsync();

            Console.WriteLine($"DEBUG: Total active locations in database: {allLocations.Count}");

            var filteredLocations = allLocations.Where(l =>
                (l.LocName != null && l.LocName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (l.Address != null && l.Address.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (l.City != null && l.City.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (l.State != null && l.State.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (l.Zipcode != null && l.Zipcode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (l.PhoneNumber != null && l.PhoneNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (l.AreaManager != null && l.AreaManager.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (l.StoreManager != null && l.StoreManager.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (l.LocNum.HasValue && l.LocNum.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            ).Take(20).ToList();

            Console.WriteLine($"DEBUG: Found {filteredLocations.Count} matching locations");

            return filteredLocations;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: Location search error: {ex.Message}");
            return new List<Location>();
        }
    }
}

public class SearchResults
{
    public List<Employee> Employees { get; set; } = new();
    public List<Department> Departments { get; set; } = new();
    public List<Location> Locations { get; set; } = new();
    public string SearchTerm { get; set; } = "";

    public int TotalResults => Employees.Count + Departments.Count + Locations.Count;
    public bool HasResults => TotalResults > 0;
}