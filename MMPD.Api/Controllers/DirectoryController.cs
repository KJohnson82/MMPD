using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Models;

namespace MMPD.Api.Controllers
{
    /// <summary>
    /// API controller for handling directory-wide data synchronization.
    /// Provides efficient endpoints for full and incremental data syncs, primarily for the MAUI application.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DirectoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DirectoryController> _logger;

        /// <summary>
        /// Initializes a new instance of the DirectoryController.
        /// </summary>
        /// <param name="context">The database context for data access.</param>
        /// <param name="logger">The logger for recording operational information and errors.</param>
        public DirectoryController(AppDbContext context, ILogger<DirectoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/Directory/sync
        /// Performs a full directory sync, returning all active locations, departments, and employees.
        /// This is designed to be a highly efficient, single-query operation for client applications.
        /// </summary>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A comprehensive response object containing all directory data.</returns>
        [HttpGet("sync")]
        public async Task<ActionResult<DirectorySyncResponse>> GetDirectorySync([FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            try
            {
                _logger.LogInformation("Directory sync requested at {Timestamp}", DateTime.UtcNow);

                // A single, efficient query to fetch all active locations and their related active children.
                var allLocations = await _context.Locations
                    .Include(l => l.LocationType)
                    .Include(l => l.Departments.Where(d => d.Active == true))
                        .ThenInclude(d => d.Employees.Where(e => e.Active == true))
                    .Where(l => l.Active == true)
                    .OrderBy(l => l.Loctype)
                        .ThenBy(l => l.LocNum ?? 0)
                        .ThenBy(l => l.LocName)
                    .ToListAsync();

                // Group the flat list of locations by their type to create a structured response.
                var groupedData = allLocations
                    .GroupBy(l => l.Loctype)
                    .ToDictionary(
                        g => GetLocationTypeName(g.Key ?? 0),
                        g => new Dictionary<string, List<Location>>
                        {
                            { "locations", g.ToList() }
                        }
                    );

                var response = new DirectorySyncResponse
                {
                    Data = new Dictionary<string, object>
                    {
                        { "loctype", groupedData }
                    },
                    SyncTimestamp = DateTime.UtcNow,
                    Success = true,
                    Message = $"Successfully synced {allLocations.Count} locations",
                    RecordCounts = new RecordCounts
                    {
                        Locations = allLocations.Count,
                        Departments = allLocations.SelectMany(l => l.Departments).Count(),
                        Employees = allLocations.SelectMany(l => l.Departments).SelectMany(d => d.Employees).Count()
                    }
                };

                _logger.LogInformation("Directory sync completed successfully: {Locations} locations, {Departments} departments, {Employees} employees",
                    response.RecordCounts.Locations, response.RecordCounts.Departments, response.RecordCounts.Employees);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Directory sync failed");
                return StatusCode(500, new DirectorySyncResponse
                {
                    Success = false,
                    Message = $"Sync failed: {ex.Message}",
                    SyncTimestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// GET: api/Directory/sync/status
        /// Retrieves metadata about the directory, such as last modification time and record counts, without transferring the full dataset.
        /// </summary>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A status response with key directory metrics.</returns>
        [HttpGet("sync/status")]
        public async Task<ActionResult<SyncStatusResponse>> GetSyncStatus([FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            try
            {
                var lastModified = await GetLastModifiedTimestamp();
                var counts = await GetRecordCounts();

                return Ok(new SyncStatusResponse
                {
                    LastModified = lastModified,
                    RecordCounts = counts,
                    DatabaseVersion = await GetDatabaseVersion(),
                    ServerTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sync status check failed");
                return StatusCode(500, $"Status check failed: {ex.Message}");
            }
        }

        /// <summary>
        /// GET: api/Directory/sync/incremental
        /// Performs an incremental sync, returning only records that have been added or modified since a specified timestamp.
        /// </summary>
        /// <param name="since">The timestamp from which to check for changes. If null, all records are returned.</param>
        /// <param name="apiKey">The API key for authorization.</param>
        /// <returns>A sync response containing only the changed data.</returns>
        [HttpGet("sync/incremental")]
        public async Task<ActionResult<DirectorySyncResponse>> GetIncrementalSync(
            [FromQuery] DateTime? since = null,
            [FromQuery] string? apiKey = null)
        {
            if (!IsValidApiKey(apiKey))
                return Unauthorized("Invalid or missing API key");

            try
            {
                var sinceDate = since ?? DateTime.MinValue;
                _logger.LogInformation("Incremental sync requested since {SinceDate}", sinceDate);

                // Fetch only locations that have a RecordAdd timestamp greater than the 'since' date.
                var modifiedLocations = await _context.Locations
                    .Include(l => l.LocationType)
                    .Include(l => l.Departments.Where(d => d.Active == true))
                        .ThenInclude(d => d.Employees.Where(e => e.Active == true))
                    .Where(l => l.Active == true && l.RecordAdd > sinceDate)
                    .OrderBy(l => l.Loctype)
                        .ThenBy(l => l.LocNum ?? 0)
                        .ThenBy(l => l.LocName)
                    .ToListAsync();

                // If no locations have been modified, return a success response with an empty data set.
                if (!modifiedLocations.Any())
                {
                    return Ok(new DirectorySyncResponse
                    {
                        Data = new Dictionary<string, object>(),
                        SyncTimestamp = DateTime.UtcNow,
                        Success = true,
                        Message = "No changes since last sync",
                        RecordCounts = new RecordCounts()
                    });
                }

                // Group the modified locations by type for the response body.
                var groupedData = modifiedLocations
                    .GroupBy(l => l.Loctype)
                    .ToDictionary(
                        g => GetLocationTypeName(g.Key ?? 0),
                        g => new Dictionary<string, List<Location>>
                        {
                            { "locations", g.ToList() }
                        }
                    );

                var response = new DirectorySyncResponse
                {
                    Data = new Dictionary<string, object>
                    {
                        { "loctype", groupedData }
                    },
                    SyncTimestamp = DateTime.UtcNow,
                    Success = true,
                    Message = $"Incremental sync: {modifiedLocations.Count} modified locations",
                    RecordCounts = new RecordCounts
                    {
                        Locations = modifiedLocations.Count,
                        Departments = modifiedLocations.SelectMany(l => l.Departments).Count(),
                        Employees = modifiedLocations.SelectMany(l => l.Departments).SelectMany(d => d.Employees).Count()
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Incremental sync failed");
                return StatusCode(500, $"Incremental sync failed: {ex.Message}");
            }
        }

        /// <summary>
        /// GET: api/Directory/health
        /// A simple health check endpoint to verify API and database connectivity.
        /// </summary>
        /// <returns>An object indicating the health status.</returns>
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var recordCount = await _context.Locations.CountAsync(l => l.Active == true);

                return Ok(new
                {
                    Status = canConnect ? "Healthy" : "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Database = canConnect ? "Connected" : "Disconnected",
                    ActiveLocations = recordCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Error = ex.Message
                });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Converts a location type ID to its string representation.
        /// </summary>
        private string GetLocationTypeName(int locationType)
        {
            return locationType switch
            {
                1 => "corporate",
                2 => "metal mart",
                3 => "service center",
                4 => "plant",
                _ => "unknown"
            };
        }

        /// <summary>
        /// Determines the most recent modification timestamp across all directory-related tables.
        /// </summary>
        private async Task<DateTime> GetLastModifiedTimestamp()
        {
            var employeeMax = await _context.Employees
                .Where(e => e.RecordAdd.HasValue)
                .MaxAsync(e => (DateTime?)e.RecordAdd) ?? DateTime.MinValue;

            var locationMax = await _context.Locations
                .Where(l => l.RecordAdd.HasValue)
                .MaxAsync(l => (DateTime?)l.RecordAdd) ?? DateTime.MinValue;

            var departmentMax = await _context.Departments
                .Where(d => d.RecordAdd.HasValue)
                .MaxAsync(d => (DateTime?)d.RecordAdd) ?? DateTime.MinValue;

            return new[] { employeeMax, locationMax, departmentMax }.Max();
        }

        /// <summary>
        /// Calculates the total counts of active records for each main entity type.
        /// </summary>
        private async Task<RecordCounts> GetRecordCounts()
        {
            return new RecordCounts
            {
                Locations = await _context.Locations.CountAsync(l => l.Active == true),
                Departments = await _context.Departments.CountAsync(d => d.Active == true),
                Employees = await _context.Employees.CountAsync(e => e.Active == true)
            };
        }

        /// <summary>
        /// Checks the database connectivity status.
        /// </summary>
        private async Task<string> GetDatabaseVersion()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                return canConnect ? "Connected" : "Disconnected";
            }
            catch
            {
                return "Unknown";
            }
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
                "crud-web-app-key-2025",
                "mobile-app-key-2025"
            };

            return validKeys.Contains(apiKey);
        }

        #endregion
    }

    #region Response Models

    /// <summary>
    /// Represents the response for a full or incremental directory sync operation.
    /// </summary>
    public class DirectorySyncResponse
    {
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime SyncTimestamp { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public RecordCounts RecordCounts { get; set; } = new();
    }

    /// <summary>
    /// Represents the response for a sync status check.
    /// </summary>
    public class SyncStatusResponse
    {
        public DateTime LastModified { get; set; }
        public RecordCounts RecordCounts { get; set; } = new();
        public string DatabaseVersion { get; set; } = string.Empty;
        public DateTime ServerTime { get; set; }
    }

    /// <summary>
    /// A DTO for holding the total counts of active records.
    /// </summary>
    public class RecordCounts
    {
        public int Locations { get; set; }
        public int Departments { get; set; }
        public int Employees { get; set; }
    }

    #endregion
}
