
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RaceDay.Api.Controllers;

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
    // Public access: anyone can browse events.
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events
            .AsNoTracking()
            .ToListAsync();

        return Ok(events);
    }

    // POST: api/Events
    // Only organisers can create events.
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateEvent(
        [FromBody] CreateEventRequest request)
    {
        var organiserId = GetCurrentUserId();

        if (organiserId == null)
            return Unauthorized();

        if (!request.EventDate.HasValue ||
            request.EventDate.Value == default)
        {
            return BadRequest(new
            {
                message = "A valid event date is required."
            });
        }

        if (!await ReferencesExist(
            request.EventTypeID, request.LocationID))
        {
            return BadRequest(new
            {
                message = "The selected event type or location is invalid."
            });
        }

        var newEvent = new Event
        {
            OrganiserID = organiserId.Value,
            EventTypeID = request.EventTypeID,
            LocationID = request.LocationID,
            EventName = request.EventName.Trim(),
            Description = request.Description?.Trim(),
            EventDate = request.EventDate.Value,
            DistanceKm = request.DistanceKm,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return StatusCode(201, new
        {
            message = "Event created successfully.",
            eventDetails = newEvent
        });
    }

    // PUT: api/Events/1
    // Organisers can update only their own events.
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        [FromBody] CreateEventRequest request)
    {
        var organiserId = GetCurrentUserId();

        if (organiserId == null)
            return Unauthorized();

        var existingEvent = await _context.Events
            .FirstOrDefaultAsync(e =>
                e.EventID == id &&
                e.OrganiserID == organiserId.Value);

        if (existingEvent == null)
        {
            return NotFound(new
            {
                message = "Event not found or you do not own it."
            });
        }

        if (!request.EventDate.HasValue ||
            request.EventDate.Value == default)
        {
            return BadRequest(new
            {
                message = "A valid event date is required."
            });
        }

        if (!await ReferencesExist(
            request.EventTypeID, request.LocationID))
        {
            return BadRequest(new
            {
                message = "The selected event type or location is invalid."
            });
        }

        existingEvent.EventName = request.EventName.Trim();
        existingEvent.Description = request.Description?.Trim();
        existingEvent.EventTypeID = request.EventTypeID;
        existingEvent.LocationID = request.LocationID;
        existingEvent.EventDate = request.EventDate.Value;
        existingEvent.DistanceKm = request.DistanceKm;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Event updated successfully.",
            eventDetails = existingEvent
        });
    }

    // DELETE: api/Events/1
    // Organisers can delete only their own events.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var organiserId = GetCurrentUserId();

        if (organiserId == null)
            return Unauthorized();

        var existingEvent = await _context.Events
            .FirstOrDefaultAsync(e =>
                e.EventID == id &&
                e.OrganiserID == organiserId.Value);

        if (existingEvent == null)
        {
            return NotFound(new
            {
                message = "Event not found or you do not own it."
            });
        }

        _context.Events.Remove(existingEvent);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Read the authenticated user's ID from their JWT token.
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdClaim, out var userId)
            ? userId
            : null;
    }

    // Check that the selected event type and location exist.
    private async Task<bool> ReferencesExist(
        int eventTypeId,
        int locationId)
    {
        var eventTypeExists = await _context.EventTypes
            .AnyAsync(t => t.EventTypeID == eventTypeId);

        if (!eventTypeExists)
            return false;

        return await _context.Locations
            .AnyAsync(l => l.LocationID == locationId);
    }
}
