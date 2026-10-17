
using System.Linq;
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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public CategoriesController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        // Everyone can view available categories.
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/Categories
        // Only an organiser who owns the event can create its categories.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CreateCategory(
            [FromBody] CategoryRequest request)
        {
            var organiserId = GetCurrentUserId();

            if (organiserId == null)
                return Unauthorized();

            var eventExists = await _context.Events.AnyAsync(e =>
                e.EventID == request.EventID &&
                e.OrganiserID == organiserId.Value);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Event not found or you do not own it."
                });
            }

            if (string.IsNullOrWhiteSpace(request.CategoryName) ||
                request.CategoryName.Trim().Length > 100 ||
                request.DistanceKm <= 0)
            {
                return BadRequest(new
                {
                    message = "Provide a category name (maximum 100 characters) and a positive distance."
                });
            }

            var category = new Category
            {
                EventID = request.EventID,
                CategoryName = request.CategoryName.Trim(),
                DistanceKm = request.DistanceKm
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Category created successfully.",
                category
            });
        }

        // PUT: api/Categories/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCategory(
            int id,
            [FromBody] CategoryRequest request)
        {
            var organiserId = GetCurrentUserId();

            if (organiserId == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.CategoryName) ||
                request.CategoryName.Trim().Length > 100 ||
                request.DistanceKm <= 0)
            {
                return BadRequest(new
                {
                    message = "Provide a category name (maximum 100 characters) and a positive distance."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            var eventExists = await _context.Events.AnyAsync(e =>
                e.EventID == category.EventID &&
                e.OrganiserID == organiserId.Value);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Category not found or you do not own its event."
                });
            }

            // Prevent moving a category to an event owned by someone else.
            var targetEventExists = await _context.Events.AnyAsync(e =>
                e.EventID == request.EventID &&
                e.OrganiserID == organiserId.Value);

            if (!targetEventExists)
            {
                return BadRequest(new
                {
                    message = "The target event does not exist or is not yours."
                });
            }

            category.EventID = request.EventID;
            category.CategoryName = request.CategoryName.Trim();
            category.DistanceKm = request.DistanceKm;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully.",
                category
            });
        }

        // DELETE: api/Categories/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var organiserId = GetCurrentUserId();

            if (organiserId == null)
                return Unauthorized();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            var eventExists = await _context.Events.AnyAsync(e =>
                e.EventID == category.EventID &&
                e.OrganiserID == organiserId.Value);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Category not found or you do not own its event."
                });
            }

            // Do not delete a category that has enrolments.
            var hasEnrolments = await _context.Enrolments
                .AnyAsync(e => e.CategoryID == id);

            if (hasEnrolments)
            {
                return Conflict(new
                {
                    message = "This category has enrolments and cannot be deleted."
                });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(userIdClaim, out var userId)
                ? userId
                : null;
        }
    }

    public class CategoryRequest
    {
        public int EventID { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public decimal DistanceKm { get; set; }
    }
}
