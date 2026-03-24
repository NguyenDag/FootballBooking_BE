using FootballBooking_BE.Common;
using FootballBooking_BE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FootballBooking_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;

        public UploadController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("image")]
        [Authorize(Roles = "ADMIN,STAFF")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                var url = await _cloudinaryService.UploadImageAsync(file);
                return Ok(ApiResponse<string>.Ok(url, "Upload ảnh thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Lỗi server: {ex.Message}"));
            }
        }
    }
}
