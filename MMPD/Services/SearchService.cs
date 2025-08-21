using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Models;
using Telerik.DataSource;
using Telerik.DataSource.Extensions;
using Telerik.SvgIcons;

namespace MMPD.Services;

/// <summary>
/// Provides services for searching across different data entities like employees, departments, and locations.
/// Utilizes Telerik DataSource components to perform flexible, in-memory filtering on data retrieved from the database.
/// </summary>
public class SearchService
{
    private readonly AppDbContext _context;

    /// <summary>
    ///* Initializes a new instance of the SearchService class.
    ///* </summary>
    ///* <param name = "context" > The database context injected for data access.</param>
    public SearchService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Orchestrates a comprehensive search across employees, departments, and locations for a given search term.
    /// </summary>
    /// <param name="searchTerm">The string to search for across various fields.</param>
    /// <returns>A SearchResults object containing lists of matching entities.</returns>
    public async Task<SearchResults> SearchAllAsync(string searchTerm)
    {
        // Return empty results if the search term is null or whitespace to avoid unnecessary queries.
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new SearchResults();

        // Asynchronously perform searches in parallel for each entity type.
        var employees = await SearchEmployeesAsync(searchTerm);
        var departments = await SearchDepartmentsAsync(searchTerm);
        var locations = await SearchLocationsAsync(searchTerm);

        // Aggregate the results into a single SearchResults object.
        return new SearchResults
        {
            Employees = employees,
            Departments = departments,
            Locations = locations,
            SearchTerm = searchTerm
        };
    }

    /// <summary>
    /// Searches for active employees where any of the specified fields contain the search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <returns>A list of up to 20 matching employees.</returns>
    private async Task<List<Employee>> SearchEmployeesAsync(string searchTerm)
    {
        // Retrieve all active employees from the database, including related department and location info.
        var allEmployees = await _context.Employees
            .Include(e => e.EmpDepartment)
            .Include(e => e.EmpLocation)
            .Where(e => e.Active == true)
            .ToListAsync();

        // Configure a Telerik DataSourceRequest for in-memory filtering.
        var request = new DataSourceRequest()
        {
            Filters = new List<IFilterDescriptor>()
        };

        // Create a composite filter with an "OR" logic to match any of the specified fields.
        var compositeFilter = new CompositeFilterDescriptor
        {
            LogicalOperator = FilterCompositionLogicalOperator.Or
        };

        // Add filters for all searchable employee fields.
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("FirstName", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("LastName", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("JobTitle", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("PhoneNumber", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("CellNumber", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Email", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Extension", FilterOperator.Contains, searchTerm));

        request.Filters.Add(compositeFilter);

        // Apply the Telerik filtering logic to the in-memory list of employees.
        var result = allEmployees.ToDataSourceResult(request);
        // Return the top 20 results, or an empty list if no matches are found.
        return (result.Data as IEnumerable<Employee>)?.Take(20).ToList() ?? new List<Employee>();
    }

    /// <summary>
    /// Searches for active departments where any of the specified fields contain the search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <returns>A list of up to 20 matching departments.</returns>
    private async Task<List<Department>> SearchDepartmentsAsync(string searchTerm)
    {
        // Retrieve all active departments from the database, including related location info.
        var allDepartments = await _context.Departments
            .Include(d => d.DeptLocation)
            .Where(d => d.Active == true)
            .ToListAsync();

        // Configure a Telerik DataSourceRequest for in-memory filtering.
        var request = new DataSourceRequest()
        {
            Filters = new List<IFilterDescriptor>()
        };

        // Create a composite filter with an "OR" logic.
        var compositeFilter = new CompositeFilterDescriptor
        {
            LogicalOperator = FilterCompositionLogicalOperator.Or
        };

        // Add filters for all searchable department fields.
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("DeptName", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("DeptManager", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("DeptPhone", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("DeptEmail", FilterOperator.Contains, searchTerm));

        request.Filters.Add(compositeFilter);

        // Apply the Telerik filtering and return the top 20 results.
        var result = allDepartments.ToDataSourceResult(request);
        return (result.Data as IEnumerable<Department>)?.Take(20).ToList() ?? new List<Department>();
    }

    /// <summary>
    /// Searches for active locations where any of the specified fields contain the search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <returns>A list of up to 20 matching locations.</returns>
    private async Task<List<MMPD.Data.Models.Location>> SearchLocationsAsync(string searchTerm)
    {
        // Retrieve all active locations from the database, including related location type info.
        var allLocations = await _context.Locations
            .Include(l => l.LocationType)
            .Where(l => l.Active == true)
            .ToListAsync();

        // Configure a Telerik DataSourceRequest for in-memory filtering.
        var request = new DataSourceRequest()
        {
            Filters = new List<IFilterDescriptor>()
        };

        // Create a composite filter with an "OR" logic.
        var compositeFilter = new CompositeFilterDescriptor
        {
            LogicalOperator = FilterCompositionLogicalOperator.Or
        };

        // Add filters for all searchable location fields.
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("LocName", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Address", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("City", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("State", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Zipcode", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("PhoneNumber", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("AreaManager", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("StoreManager", FilterOperator.Contains, searchTerm));
        compositeFilter.FilterDescriptors.Add(new FilterDescriptor("LocNum", FilterOperator.Contains, searchTerm));

        request.Filters.Add(compositeFilter);

        // Apply the Telerik filtering and return the top 20 results.
        var result = allLocations.ToDataSourceResult(request);
        return (result.Data as IEnumerable<MMPD.Data.Models.Location>)?.Take(20).ToList() ?? new List<MMPD.Data.Models.Location>();
    }
}

/// <summary>
/// A data transfer object (DTO) to hold the aggregated results from a search operation.
/// </summary>
public class SearchResults
{
    /// <summary>
    /// A list of employees that matched the search criteria.
    /// </summary>
    public List<Employee> Employees { get; set; } = new();

    /// <summary>
    /// A list of departments that matched the search criteria.
    /// </summary>
    public List<Department> Departments { get; set; } = new();

    /// <summary>
    /// A list of locations that matched the search criteria.
    /// </summary>
    public List<MMPD.Data.Models.Location> Locations { get; set; } = new();

    /// <summary>
    /// The original search term used to generate these results.
    /// </summary>
    public string SearchTerm { get; set; } = "";

    /// <summary>
    /// A calculated property that returns the total count of all results.
    /// </summary>
    public int TotalResults => Employees.Count + Departments.Count + Locations.Count;

    /// <summary>
    /// A calculated boolean property indicating if any results were found.
    /// </summary>
    public bool HasResults => TotalResults > 0;
}
