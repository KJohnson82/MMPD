using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MMPD.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DirectoryController : ControllerBase
    {
        private readonly IDirectoryService _directoryService;

        public DirectoryController(IDirectoryService directoryService)
        {
            _directoryService = directoryService;
        }

        [HttpGet("sync")]
        public async Task<IActionResult> GetFullDirectory()
        {
            var data = new
            {
                Employees = await _directoryService.GetEmployeesAsync(),
                Departments = await _directoryService.GetDepartmentsAsync(),
                Locations = await _directoryService.GetLocationsAsync(),
                LocationTypes = await _directoryService.GetLoctypesAsync(),
                Timestamp = DateTime.UtcNow
            };
            return Ok(data);
        }
    }
}
