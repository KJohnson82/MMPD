using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Enums;
using MMPD.Data.Enums.MMPD.Data.Enums;
using MMPD.Data.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MMPD.Data.Data
{
    public class ExportData
    {
        private readonly AppDbContext _context;

        public ExportData(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ✅ SINGLE METHOD - Replaces 4 duplicate methods with 200+ lines of code
        /// </summary>
        public async Task<Dictionary<string, List<Location>>> FetchLocationsByTypeAsync(LocationType locationType)
        {
            var locations = await _context.Locations
                .Where(l => l.Loctype == (int)locationType && l.Active == true)
                .Select(l => new Location
                {
                    Id = l.Id,
                    LocName = l.LocName,
                    LocNum = l.LocNum,
                    Address = l.Address,
                    City = l.City,
                    State = l.State,
                    Zipcode = l.Zipcode,
                    PhoneNumber = l.PhoneNumber,
                    FaxNumber = l.FaxNumber,
                    Email = l.Email,
                    Hours = l.Hours,
                    Loctype = l.Loctype,
                    AreaManager = l.AreaManager,
                    StoreManager = l.StoreManager,
                    Active = l.Active,
                    Departments = l.Departments
                        .Where(d => d.Active == true)
                        .Select(d => new Department
                        {
                            Id = d.Id,
                            DeptName = d.DeptName,
                            DeptManager = d.DeptManager,
                            DeptPhone = d.DeptPhone,
                            DeptEmail = d.DeptEmail,
                            DeptFax = d.DeptFax,
                            Location = d.Location,
                            Active = d.Active,
                            Employees = d.Employees
                                .Where(e => e.Active == true)
                                .OrderBy(e => e.FirstName)
                                .ThenBy(e => e.LastName)
                                .Select(e => new Employee
                                {
                                    Id = e.Id,
                                    FirstName = e.FirstName,
                                    LastName = e.LastName,
                                    JobTitle = e.JobTitle,
                                    IsManager = e.IsManager,
                                    PhoneNumber = e.PhoneNumber,
                                    CellNumber = e.CellNumber,
                                    Extension = e.Extension,
                                    Email = e.Email,
                                    NetworkId = e.NetworkId,
                                    EmpAvatar = e.EmpAvatar,
                                    Location = e.Location,
                                    Department = e.Department,
                                    Active = e.Active,
                                }).ToList()
                        })
                        .OrderBy(d => d.DeptName)
                        .ToList()
                })
                .OrderBy(l => l.LocNum ?? 0)
                .ThenBy(l => l.LocName)
                .ToListAsync();

            return new Dictionary<string, List<Location>> { { "locations", locations } };
        }

        /// <summary>
        /// ✅ SIMPLIFIED - Now just calls the single method (was 50+ lines of duplicate code)
        /// </summary>
        public async Task<Dictionary<string, List<Location>>> FetchCorpDataAsync() =>
            await FetchLocationsByTypeAsync(LocationType.Corporate);

        public async Task<Dictionary<string, List<Location>>> FetchPlantDataAsync() =>
            await FetchLocationsByTypeAsync(LocationType.Plant);

        public async Task<Dictionary<string, List<Location>>> FetchMMDataAsync() =>
            await FetchLocationsByTypeAsync(LocationType.MetalMart);

        public async Task<Dictionary<string, List<Location>>> FetchSCDataAsync() =>
            await FetchLocationsByTypeAsync(LocationType.ServiceCenter);

        /// <summary>
        /// ✅ PARALLEL EXECUTION - Much faster than sequential calls
        /// </summary>
        public async Task<Dictionary<string, object>> GenerateJson()
        {
            // Execute all location type queries in parallel for better performance
            var tasks = new[]
            {
                FetchLocationsByTypeAsync(LocationType.Corporate),
                FetchLocationsByTypeAsync(LocationType.MetalMart),
                FetchLocationsByTypeAsync(LocationType.ServiceCenter),
                FetchLocationsByTypeAsync(LocationType.Plant)
            };

            var results = await Task.WhenAll(tasks);

            return new Dictionary<string, object>
            {
                { "loctype", new Dictionary<string, object>
                    {
                        { LocationType.Corporate.GetJsonKey(), results[0] },
                        { LocationType.MetalMart.GetJsonKey(), results[1] },
                        { LocationType.ServiceCenter.GetJsonKey(), results[2] },
                        { LocationType.Plant.GetJsonKey(), results[3] }
                    }
                }
            };
        }

        /// <summary>
        /// ✅ CONFIGURABLE - No more hardcoded file paths
        /// </summary>
        public async Task ExportJsonToFileAsync(string? outputDirectory = null)
        {
            try
            {
                var jsonData = await GenerateJson();
                var jsonString = JsonConvert.SerializeObject(jsonData, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                });

                // ✅ Use provided directory or default to user's Documents folder
                var directoryPath = outputDirectory ??
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EMP_Data");

                Directory.CreateDirectory(directoryPath);

                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmm");
                var fileName = $"MED_Export_{timestamp}.json";
                var filePath = Path.Combine(directoryPath, fileName);

                await File.WriteAllTextAsync(filePath, jsonString);

                Console.WriteLine($"JSON Export Successful. File saved at {filePath}");
            }
            catch (Exception ex)
            {
                throw new Exception($"JSON export failed: {ex.Message}", ex);
            }
        }


    }
}
