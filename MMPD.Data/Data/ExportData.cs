using MMPD.Data.Models;
using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using Newtonsoft.Json;
using System.Text.Json;



namespace MMPD.Data.Data
{
    public class ExportData
    {

        //private readonly AppDbContext _context;
        private readonly AppDbContext _context;

        //public ExportData(AppDbContext context)
        //{
        //    _context = context;
        //}

        public ExportData(AppDbContext context)
        {
            _context = context;
        }

        //public async Task<List<Location>> FetchCorpDataAsync()
        public async Task<Dictionary<string, List<Location>>> FetchCorpDataAsync()
        {
            var corporate = await _context.Locations!
                .Where(l => l.Loctype == 1)
                .Select(l => new Location
                {
                    //LocId = l.LocId,
                    Id = l.Id,
                    LocName = l.LocName,
                    LocNum = l.LocNum,
                    Address = l.Address,
                    City = l.City,
                    State = l.State,
                    Zipcode = l.Zipcode,
                    PhoneNumber = l.PhoneNumber,
                    FaxNumber = l.FaxNumber,
                    //LocEmail = l.LocEmail,
                    Email = l.Email,
                    Hours = l.Hours,
                    Loctype = l.Loctype,
                    AreaManager = l.AreaManager,
                    StoreManager = l.StoreManager,
                    //RecordAdd = l.RecordAdd,
                    Active = l.Active == true,
                    Departments = l.Departments.Select(d => new Department
                    {
                        Id = d.Id,
                        DeptName = d.DeptName,
                        DeptManager = d.DeptManager,
                        DeptPhone = d.DeptPhone,
                        DeptEmail = d.DeptEmail,
                        DeptFax = d.DeptFax,
                        Location = d.Location,
                        Active = d.Active,
                        Employees = d.Employees.Select(e => new Employee
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
                            Location = e.Location,
                            Department = e.Department,
                            Active = e.Active,
                        }).OrderBy(e => e.FirstName + e.LastName)
                        .ToList()
                    }).OrderBy(d => d.DeptName)
                    .ToList()
                })
                .ToListAsync();

            return new Dictionary<string, List<Location>> { { "locations", corporate } };
        }

        public async Task<Dictionary<string, List<Location>>> FetchPlantDataAsync()
        {
            var plant = await _context.Locations!
                .Where(l => l.Loctype == 4)
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
                    //RecordAdd = l.RecordAdd,
                    Active = l.Active == true,
                    Departments = l.Departments.Select(d => new Department
                    {
                        Id = d.Id,
                        DeptName = d.DeptName,
                        DeptManager = d.DeptManager,
                        DeptPhone = d.DeptPhone,
                        DeptEmail = d.DeptEmail,
                        DeptFax = d.DeptFax,
                        Location = d.Location,
                        //RecordAdd = d.RecordAdd,
                        Active = d.Active,
                        Employees = d.Employees.Select(e => new Employee
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
                            //EmpAvatar = e.EmpAvatar,
                            Location = e.Location,
                            Department = e.Department,
                            //RecordAdd = e.RecordAdd
                            Active = e.Active,
                        }).OrderBy(e => e.FirstName + e.LastName).ToList()
                    }).OrderBy(d => d.DeptName)
                    .ToList()
                })
                .ToListAsync();
            return new Dictionary<string, List<Location>> { { "locations", plant } };
        }

        public async Task<Dictionary<string, List<Location>>> FetchMMDataAsync()
        {
            var metalmart = await _context.Locations!
                .Where(l => l.Loctype == 2)
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
                    //RecordAdd = l.RecordAdd
                    //Employees = l.Employees.Select(e => new Employee
                    //{
                    //    EmpId = e.EmpId,
                    //    FirstName = e.FirstName,
                    //    LastName = e.LastName,
                    //    JobTitle = e.JobTitle,
                    //    IsManager = e.IsManager,
                    //    PhoneNumber = e.PhoneNumber,
                    //    CellNumber = e.CellNumber,
                    //    Extension = e.Extension,
                    //    Email = e.Email,
                    //    NetworkId = e.NetworkId,
                    //    EmpAvatar = e.EmpAvatar,
                    //    LocationId = e.LocationId,
                    //    DepartmentId = e.DepartmentId,
                    //    RecordAdd = e.RecordAdd
                    //}).ToList()
                })
                .OrderBy(m => m.LocNum)
                .ToListAsync();

            return new Dictionary<string, List<Location>> { { "locations", metalmart } };
        }

        public async Task<Dictionary<string, List<Location>>> FetchSCDataAsync()
        {
            var servicecenter = await _context.Locations!
                .Where(l => l.Loctype == 3)
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
                    //RecordAdd = l.RecordAdd
                    //Employees = l.Employees.Select(e => new Employee
                    //{
                    //    EmpId = e.EmpId,
                    //    FirstName = e.FirstName,
                    //    LastName = e.LastName,
                    //    JobTitle = e.JobTitle,
                    //    IsManager = e.IsManager,
                    //    PhoneNumber = e.PhoneNumber,
                    //    CellNumber = e.CellNumber,
                    //    Extension = e.Extension,
                    //    Email = e.Email,
                    //    NetworkId = e.NetworkId,
                    //    EmpAvatar = e.EmpAvatar,
                    //    LocationId = e.LocationId,
                    //    DepartmentId = e.DepartmentId,
                    //    RecordAdd = e.RecordAdd
                    //}).ToList()
                })
                .OrderBy(s => s.LocNum)
                .ToListAsync();

            return new Dictionary<string, List<Location>> { { "locations", servicecenter } };
        }



        private async Task<List<Department>> FetchDepartmentsForLocationAsync(int locationId)
        {
            var departments = await _context.Departments!
                .Where(d => d.DeptLocation!.Id == locationId)
                .OrderBy(d => d.DeptName)
                .Select(d => new Department
                {
                    Id = d.Id,
                    DeptName = d.DeptName,
                    DeptManager = d.DeptManager,
                    DeptPhone = d.DeptPhone,
                    DeptEmail = d.DeptEmail,
                    DeptFax = d.DeptFax,
                    Location = d.Location,
                    //RecordAdd = d.RecordAdd
                    Active = d.Active,
                })
                .ToListAsync();

            return departments;
        }


        private async Task<List<Employee>> FetchEmployeesForDepartmentAsync(int departmentId)
        {
            var employees = await _context.Employees!
                .Where(e => e.Department == departmentId)
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
                    //RecordAdd = e.RecordAdd
                    Active = e.Active,
                })
                .ToListAsync();

            return employees;
        }

        private async Task<List<Employee>> FetchEmployeesForLocationAsync(int locationId)
        {
            var employees = await _context.Employees!
                .Where(e => e.Location == locationId)
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
                    //RecordAdd = e.RecordAdd
                    Active = e.Active,
                })
                .ToListAsync();

            return employees;
        }


        private async Task<Dictionary<int, Location>> FetchLocationsAsync(int locationType)
        {
            var locations = await _context.Locations!
                .Where(l => l.Loctype == locationType && l.Id != 0)
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
                    //RecordAdd = l.RecordAdd
                    Active = l.Active,
                })
                .ToDictionaryAsync(l => l.Id);

            return locations;
        }

        public async Task<Dictionary<string, object>> GenerateJson()
        {
            var corporate = await FetchCorpDataAsync();
            var plant = await FetchPlantDataAsync();
            var metalMart = await FetchMMDataAsync();
            var serviceCenter = await FetchSCDataAsync();

            return new Dictionary<string, object>
    {
        { "loctype", new Dictionary<string, object>
            {
                { "corporate", corporate },
                { "metal mart", metalMart },
                { "service center", serviceCenter },
                { "plant", plant }
            }
        }
    };
        }

        public async Task ExportJsonToFileAsync()
        {
            try
            {
                var jsonData = await GenerateJson();
                var options = new JsonSerializerOptions { WriteIndented = true };
                var jsonString = JsonConvert.SerializeObject(jsonData, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

                });
                //var jsonString = System.Text.Json.JsonSerializer.Serialize(jsonData, options);
                //var directoryPath = Path.Combine("C://Users//kevin00j1.MCELROY//Documents//EMP_Data");
                var directoryPath = Path.Combine("D://Software_Development//EmployeeDirectory//wwwroot//JSON");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var now = DateTime.Now;
                var outMoment = now.ToString("yyddMMHHmm");
                var fileName = $"MED_{outMoment}.json";
                var filePath = Path.Combine(directoryPath, fileName);


                Console.WriteLine(jsonString);
                Console.WriteLine(fileName);
                Console.WriteLine(filePath);

                await File.WriteAllTextAsync(filePath, jsonString);


                //await SaveJsonToFileAsync(jsonString, $"MPD_Data_{outMoment}.json");
                Console.WriteLine($"JSON Export Successful. File saved at {filePath}");
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new Exception("Access to the directory is denied. Ensure the application has the required permissions.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during the JSON export.", ex);
            }

        }

    }
}
