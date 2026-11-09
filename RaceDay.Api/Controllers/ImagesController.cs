using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Organiser")]
    public class ImagesController : ControllerBase
    {
        private const long MaxFileSize = 5 * 1024 * 1024;

        private readonly BlobStorageService _blobStorageService;

        public ImagesController(BlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(MaxFileSize)]
        public async Task<IActionResult> UploadImage(
            IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please select an image.");
            }

            if (file.Length > MaxFileSize)
            {
                return BadRequest("The image cannot exceed 5 MB.");
            }

            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedTypes.Contains(
                file.ContentType.ToLowerInvariant()))
            {
                return BadRequest(
                    "Only JPEG, PNG and WebP images are allowed.");
            }

            await using var stream = file.OpenReadStream();

            var header = new byte[12];
            var bytesRead = await stream.ReadAsync(header);
            stream.Position = 0;

            if (!IsValidImage(header, bytesRead, file.ContentType))
            {
                return BadRequest("The uploaded file is not a valid image.");
            }

            var extension = file.ContentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => throw new InvalidOperationException(
                    "Unsupported image type.")
            };

            var fileName = $"{Guid.NewGuid()}{extension}";

            try
            {
                var imageUrl = await _blobStorageService.UploadAsync(
                    stream,
                    fileName,
                    file.ContentType);

                return Ok(new
                {
                    message = "Image uploaded successfully.",
                    fileName,
                    imageUrl
                });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "The image could not be uploaded.");
            }
        }

        private static bool IsValidImage(
            byte[] header,
            int bytesRead,
            string contentType)
        {
            if (contentType.Equals(
                "image/jpeg",
                StringComparison.OrdinalIgnoreCase))
            {
                return bytesRead >= 3 &&
                       header[0] == 0xFF &&
                       header[1] == 0xD8 &&
                       header[2] == 0xFF;
            }

            if (contentType.Equals(
                "image/png",
                StringComparison.OrdinalIgnoreCase))
            {
                return bytesRead >= 8 &&
                       header[0] == 0x89 &&
                       header[1] == 0x50 &&
                       header[2] == 0x4E &&
                       header[3] == 0x47 &&
                       header[4] == 0x0D &&
                       header[5] == 0x0A &&
                       header[6] == 0x1A &&
                       header[7] == 0x0A;
            }

            if (contentType.Equals(
                "image/webp",
                StringComparison.OrdinalIgnoreCase))
            {
                return bytesRead >= 12 &&
                       header[0] == 0x52 &&
                       header[1] == 0x49 &&
                       header[2] == 0x46 &&
                       header[3] == 0x46 &&
                       header[8] == 0x57 &&
                       header[9] == 0x45 &&
                       header[10] == 0x42 &&
                       header[11] == 0x50;
            }

            return false;
        }
    }
}