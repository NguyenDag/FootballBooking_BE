using FootballBooking_BE.Common;
using FootballBooking_BE.Models.DTOs;
using FootballBooking_BE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballBooking_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PitchesController : ControllerBase
    {
        private readonly IPitchService _pitchService;

        public PitchesController(IPitchService pitchService)
        {
            _pitchService = pitchService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            IEnumerable<PitchDTO> result;

            if (userRole == "STAFF" && int.TryParse(userIdClaim, out int staffId))
            {
                result = await _pitchService.GetPitchesByStaffIdAsync(staffId);
            }
            else
            {
                result = await _pitchService.GetAllPitchesAsync();
            }

            return Ok(ApiResponse<IEnumerable<PitchDTO>>.Ok(result, "Lấy danh sách sân thành công"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _pitchService.GetPitchByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy sân bóng"));
            }
            return Ok(ApiResponse<PitchDTO>.Ok(result, "Lấy thông tin sân thành công"));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreatePitchRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));
            }

            var result = await _pitchService.CreatePitchAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.PitchId }, ApiResponse<PitchDTO>.Ok(result, "Tạo sân bóng thành công"));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePitchRequest request)
        {
            var success = await _pitchService.UpdatePitchAsync(id, request);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy sân bóng để cập nhật"));
            }
            return Ok(ApiResponse<object>.Ok(null, "Cập nhật sân bóng thành công"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _pitchService.DeletePitchAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy sân bóng để xóa"));
            }
            return Ok(ApiResponse<object>.Ok(null, "Xóa sân bóng thành công"));
        }

    }
}
