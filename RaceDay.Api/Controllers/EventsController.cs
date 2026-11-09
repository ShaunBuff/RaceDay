
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Events
        // Everyone can view events.
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context.Events
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Events/5
        // Everyone can view event details.
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return eventItem;
        }

        // POST: api/Events
        // Only organisers can create events.
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event eventItem)
        {
            if (!TryGetOrganiserId(out var organiserId))
            {
                return Unauthorized();
            }

            // Assign ownership using the logged-in organiser.
            // Never trust an OrganiserID supplied by the client.
            eventItem.OrganiserID = organiserId;
            eventItem.CreatedAt = DateTime.UtcNow;

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = eventItem.EventID },
                eventItem);
        }

        // PUT: api/Events/5
        // Only the event's owning organiser can update it.
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            Event eventItem)
        {
            if (id != eventItem.EventID)
            {
                return BadRequest();
            }

            if (!TryGetOrganiserId(out var organiserId))
            {
                return Unauthorized();
            }

            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (existingEvent == null)
            {
                return NotFound();
            }

            if (existingEvent.OrganiserID != organiserId)
            {
                return Forbid();
            }

            // Update editable properties only.
            // Preserve EventID, OrganiserID and CreatedAt.
            existingEvent.EventTypeID = eventItem.EventTypeID;
            existingEvent.LocationID = eventItem.LocationID;
            existingEvent.EventName = eventItem.EventName;
            existingEvent.Description = eventItem.Description;
            existingEvent.EventDate = eventItem.EventDate;
            existingEvent.DistanceKm = eventItem.DistanceKm;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Events/5
        // Only the event's owning organiser can delete it.
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!TryGetOrganiserId(out var organiserId))
            {
                return Unauthorized();
            }

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            if (eventItem.OrganiserID != organiserId)
            {
                return Forbid();
            }

            _context.Events.Remove(eventItem);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Get the logged-in organiser's database ID.
        private bool TryGetOrganiserId(out int organiserId)
        {
            var userIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(userIdClaim, out organiserId);
        }

        private bool EventExists(int id)
        {
            return _context.Events
                .Any(e => e.EventID == id);
        }
    }
}
