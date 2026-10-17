
using System;
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
    [Authorize(Roles = "Organiser,Participant")]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ProfileController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Profile
        // Return the authenticated user's own profile.
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserID == userId.Value)
                .Select(u => new
                {
                    u.UserID,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Role,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new
                {
                    message = "Profile not found."
                });

            return Ok(user);
        }

        // PUT: api/Profile
        // Update only the authenticated user's own profile details.
        [HttpPut]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileRequest request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var firstName = request.FirstName?.Trim();
            var lastName = request.LastName?.Trim();
            var email = request.Email?.Trim();

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new
                {
                    message = "First name, last name and email are required."
                });
            }

            if (firstName.Length > 100 ||
                lastName.Length > 100 ||
                email.Length > 255 ||
                !email.Contains("@"))
            {
                return BadRequest(new
                {
                    message = "Please provide valid profile details."
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId.Value);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "Profile not found."
                });
            }

            var emailAlreadyUsed = await _context.Users.AnyAsync(u =>
                u.Email == email && u.UserID != userId.Value);

            if (emailAlreadyUsed)
            {
                return Conflict(new
                {
                    message = "That email address is already in use."
                });
            }

            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                profile = new
                {
                    user.UserID,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Role,
                    user.CreatedAt
                }
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

    public class UpdateProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
