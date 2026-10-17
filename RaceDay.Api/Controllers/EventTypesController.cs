
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventTypesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventTypesController(RaceDayContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEventTypes()
        {
            var eventTypes = await _context.EventTypes
                .AsNoTracking()
                .ToListAsync();

            return Ok(eventTypes);
        }
    }
}
