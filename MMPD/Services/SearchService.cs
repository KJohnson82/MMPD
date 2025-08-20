using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Models;
using Telerik.DataSource;
using Telerik.DataSource.Extensions;

namespace MMPD.Services;

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

        var employees = await SearchEmployeesAsync(searchTerm);
        var departments = await SearchDepartmentsAsync(searchTerm);
        var locations = await SearchLocationsAsync(searchTerm);

        return new SearchResults
        {
            Employees = employees,
            Departments = departments,
            Locations = locations,
            SearchTerm = searchTerm
        };
    }

    private async Task<List<Employee>> SearchEmployeesAsync(string searchTerm)
    {
        var allEmployees = await _context.Employees
            .Include(e => e.EmpDepartment)
            .Include(e => e.EmpLocation)
            .Where(e => e.Active == true)
            .ToListAsync();

        // Create Telerik DataSource request with composite filter
        var request = new DataSourceRequest()
        {
            Filters = new List<IFilterDescriptor>()
        };

        var compositeFilter = new CompositeFilterDescriptor
        {
            LogicalOperator = FilterCompositionLogicalOperator.Or
        };

        // Add filters for all employee searchable fields
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("FirstName", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("LastName", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("JobTitle", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("PhoneNumber", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("CellNumber", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Email", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Extension", FilterOperator.Contains, searchTerm));

        request.Filters.Add(compositeFilter);

        // Apply Telerik filtering
        var result = allEmployees.ToDataSourceResult(request);
        return (result.Data as IEnumerable<Employee>)?.Take(20).ToList() ?? new List<Employee>();
    }

    private async Task<List<Department>> SearchDepartmentsAsync(string searchTerm)
    {
        var allDepartments = await _context.Departments
            .Include(d => d.DeptLocation)
            .Where(d => d.Active == true)
            .ToListAsync();

        var request = new DataSourceRequest()
        {
            Filters = new List<IFilterDescriptor>()
        };

        var compositeFilter = new CompositeFilterDescriptor
        {
            LogicalOperator = FilterCompositionLogicalOperator.Or
        };

        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("DeptName", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("DeptManager", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("DeptPhone", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("DeptEmail", FilterOperator.Contains, searchTerm));

        request.Filters.Add(compositeFilter);

        var result = allDepartments.ToDataSourceResult(request);
        return (result.Data as IEnumerable<Department>)?.Take(20).ToList() ?? new List<Department>();
    }

    private async Task<List<MMPD.Data.Models.Location>> SearchLocationsAsync(string searchTerm)
    {
        var allLocations = await _context.Locations
            .Include(l => l.LocationType)
            .Where(l => l.Active == true)
            .ToListAsync();

        var request = new DataSourceRequest()
        {
            Filters = new List<IFilterDescriptor>()
        };

        var compositeFilter = new CompositeFilterDescriptor
        {
            LogicalOperator = FilterCompositionLogicalOperator.Or
        };

        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("LocName", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Address", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("City", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("State", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Zipcode", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("PhoneNumber", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("AreaManager", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("StoreManager", FilterOperator.Contains, searchTerm));

        // Handle LocNum as string for search
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("LocNum", FilterOperator.Contains, searchTerm));

        request.Filters.Add(compositeFilter);

        var result = allLocations.ToDataSourceResult(request);
        return (result.Data as IEnumerable<MMPD.Data.Models.Location>)?.Take(20).ToList() ?? new List<MMPD.Data.Models.Location>();
    }
}

public class SearchResults
{
    public List<Employee> Employees { get; set; } = new();
    public List<Department> Departments { get; set; } = new();
    public List<MMPD.Data.Models.Location> Locations { get; set; } = new();
    public string SearchTerm { get; set; } = "";

    public int TotalResults => Employees.Count + Departments.Count + Locations.Count;
    public bool HasResults => TotalResults > 0;
}