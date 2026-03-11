using FootballBooking_BE.Common;
using FootballBooking_BE.Models.DTOs;
using FootballBooking_BE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FootballBooking_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceSlotsController : ControllerBase
    {
        private readonly IPriceSlotService _slotService;

        public PriceSlotsController(IPriceSlotService slotService)
        {
            _slotService = slotService;
        }

       
        [HttpGet("pitch/{pitchId}")]
        public async Task<IActionResult> GetByPitch(int pitchId)
        {
            var result = await _slotService.GetSlotsByPitchIdAsync(pitchId);
            return Ok(ApiResponse<IEnumerable<PriceSlotDTO>>.Ok(result, "Lấy danh sách khung giá thành công"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _slotService.GetSlotByIdAsync(id);
            if (result == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy khung giá"));
            return Ok(ApiResponse<PriceSlotDTO>.Ok(result, "Lấy thông tin khung giá thành công"));
        }

        [HttpPost("pitch/{pitchId}")]
        public async Task<IActionResult> Create(int pitchId, [FromBody] PriceSlotRequest request)
        {
            try
            {
                var result = await _slotService.CreateSlotAsync(pitchId, request);
                return CreatedAtAction(nameof(GetById), new { id = result.PriceSlotId }, ApiResponse<PriceSlotDTO>.Ok(result, "Tạo khung giá thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PriceSlotRequest request)
        {
            var success = await _slotService.UpdateSlotAsync(id, request);
            if (!success) return NotFound(ApiResponse<object>.Fail("Không tìm thấy khung giá để cập nhật"));
            return Ok(ApiResponse<object>.Ok(null, "Cập nhật khung giá thành công"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _slotService.DeleteSlotAsync(id);
            if (!success) return NotFound(ApiResponse<object>.Fail("Không tìm thấy khung giá để xóa"));
            return Ok(ApiResponse<object>.Ok(null, "Xóa khung giá thành công"));
        }
    }
}
