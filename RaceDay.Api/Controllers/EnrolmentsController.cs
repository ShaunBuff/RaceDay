
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EnrolmentsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {
            // Organisers can view enrolments for their own events.
            if (User.IsInRole("Organiser"))
            {
                var organiserIdClaim = User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(organiserIdClaim, out var organiserId))
                {
                    return Unauthorized();
                }

                var organiserEnrolments = await _context.Enrolments
                    .AsNoTracking()
                    .Where(e => _context.Events.Any(ev =>
                        ev.EventID == e.EventID &&
                        ev.OrganiserID == organiserId))
                    .ToListAsync();

                return Ok(organiserEnrolments);
            }

            // Participants can view only their own enrolments.
            if (User.IsInRole("Participant"))
            {
                var userIdClaim = User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out var participantId))
                {
                    return Unauthorized();
                }

                var ownEnrolments = await _context.Enrolments
                    .AsNoTracking()
                    .Where(e => e.ParticipantID == participantId)
                    .ToListAsync();

                return Ok(ownEnrolments);
            }

            return Forbid();
        }

        // POST: api/Enrolments
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEnrolment(
            [FromBody] CreateEnrolmentRequest request)
        {
            var userIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var participantId))
            {
                return Unauthorized();
            }

            if (request.EventID <= 0 || request.CategoryID <= 0)
            {
                return BadRequest(
                    "Valid event and category IDs are required.");
            }

            var eventExists = await _context.Events
                .AnyAsync(e => e.EventID == request.EventID);

            if (!eventExists)
            {
                return NotFound("The selected event was not found.");
            }

            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.CategoryID == request.CategoryID);

            if (category == null)
            {
                return NotFound("The selected category was not found.");
            }

            if (category.EventID != request.EventID)
            {
                return BadRequest(
                    "The selected category does not belong to this event.");
            }

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantID == participantId &&
                    e.EventID == request.EventID);

            if (alreadyEnrolled)
            {
                return Conflict(
                    "You are already enrolled in this event.");
            }

            var enrolment = new Enrolment
            {
                ParticipantID = participantId,
                EventID = request.EventID,
                CategoryID = request.CategoryID,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Registered"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(
                201,
                new
                {
                    message = "Enrolment created successfully.",
                    enrolment
                });
        }
    }

    public class CreateEnrolmentRequest
    {
        public int EventID { get; set; }

        public int CategoryID { get; set; }
    }
}
