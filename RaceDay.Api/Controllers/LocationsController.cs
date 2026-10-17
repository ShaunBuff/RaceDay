
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public LocationsController(RaceDayContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetLocations()
        {
            var locations = await _context.Locations
                .AsNoTracking()
                .ToListAsync();

            return Ok(locations);
        }
    }
}
