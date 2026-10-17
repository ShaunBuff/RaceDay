
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Results
        // Participants see their own results.
        // Organisers see results for their own events.
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetResults()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            if (User.IsInRole("Participant"))
            {
                var results = await _context.Results
                    .AsNoTracking()
                    .Where(r => _context.Enrolments.Any(e =>
                        e.EnrolmentID == r.EnrolmentID &&
                        e.ParticipantID == userId.Value))
                    .ToListAsync();

                return Ok(results);
            }

            if (User.IsInRole("Organiser"))
            {
                var results = await _context.Results
                    .AsNoTracking()
                    .Where(r => _context.Enrolments.Any(e =>
                        e.EnrolmentID == r.EnrolmentID &&
                        _context.Events.Any(ev =>
                            ev.EventID == e.EventID &&
                            ev.OrganiserID == userId.Value)))
                    .ToListAsync();

                return Ok(results);
            }

            return Forbid();
        }

        // POST: api/Results/{enrolmentId}
        // Only the organiser who owns the event may record its result.
        [HttpPost("{enrolmentId:int}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> CreateResult(
            int enrolmentId,
            [FromBody] CreateResultRequest request)
        {
            var organiserId = GetCurrentUserId();

            if (organiserId == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.FinishTime))
            {
                return BadRequest(new
                {
                    message = "Finish time is required. Use HH:mm:ss format."
                });
            }

            if (!TimeSpan.TryParseExact(
                    request.FinishTime,
                    new[] { @"h\:mm\:ss", @"hh\:mm\:ss" },
                    CultureInfo.InvariantCulture,
                    out var finishTime)
                || finishTime < TimeSpan.Zero)
            {
                return BadRequest(new
                {
                    message = "Invalid finish time. Use HH:mm:ss, for example 01:45:30."
                });
            }

            if (request.FinishingPosition <= 0)
            {
                return BadRequest(new
                {
                    message = "Finishing position must be greater than zero."
                });
            }

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "The enrolment was not found."
                });
            }

            var organiserOwnsEvent = await _context.Events.AnyAsync(e =>
                e.EventID == enrolment.EventID &&
                e.OrganiserID == organiserId.Value);

            if (!organiserOwnsEvent)
            {
                return NotFound(new
                {
                    message = "Enrolment not found for one of your events."
                });
            }

            var resultExists = await _context.Results
                .AnyAsync(r => r.EnrolmentID == enrolmentId);

            if (resultExists)
            {
                return Conflict(new
                {
                    message = "A result has already been recorded for this enrolment."
                });
            }

            var result = new Result
            {
                EnrolmentID = enrolmentId,
                FinishTime = finishTime,
                FinishingPosition = request.FinishingPosition,
                RecordedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Result recorded successfully.",
                result
            });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(userIdClaim, out var userId)
                ? userId
                : null;
        }
    }

    public class CreateResultRequest
    {
        public string FinishTime { get; set; } = string.Empty;

        public int FinishingPosition { get; set; }
    }
}
