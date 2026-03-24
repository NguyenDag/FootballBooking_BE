using FootballBooking_BE.Common;
using FootballBooking_BE.Models.DTOs.Staff;
using FootballBooking_BE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FootballBooking_BE.Controllers
{
    /// <summary>
    /// ADMIN: Quản lý nhân viên, phân công sân, ca làm việc
    /// </summary>
    [ApiController]
    [Route("api/admin/staff")]
    [Authorize(Policy = "AdminOnly")]
    [Produces("application/json")]
    public class StaffManagementController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffManagementController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        // ═══════════════════════════════════════════════════════════
        // STAFF ACCOUNT CRUD
        // ═══════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/staff — Danh sách toàn bộ nhân viên</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<StaffSummaryResponse>>>> GetAll()
        {
            var result = await _staffService.GetAllStaffAsync();
            return Ok(ApiResponse<List<StaffSummaryResponse>>.Ok(result));
        }

        /// <summary>GET /api/admin/staff/{id} — Chi tiết một nhân viên</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<StaffResponse>>> GetById(int id)
        {
            var result = await _staffService.GetStaffDetailAsync(id);
            return Ok(ApiResponse<StaffResponse>.Ok(result));
        }

        /// <summary>POST /api/admin/staff — Tạo tài khoản nhân viên mới</summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<StaffResponse>>> Create(
            [FromBody] CreateStaffRequest request)
        {
            var result = await _staffService.CreateStaffAsync(request);
            return CreatedAtAction(nameof(GetById),
                new { id = result.UserId },
                ApiResponse<StaffResponse>.Ok(result, "Tạo nhân viên thành công."));
        }

        /// <summary>PUT /api/admin/staff/{id} — Cập nhật thông tin nhân viên</summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<StaffResponse>>> Update(
            int id,
            [FromBody] UpdateStaffRequest request)
        {
            var result = await _staffService.UpdateStaffAsync(id, request);
            return Ok(ApiResponse<StaffResponse>.Ok(result, "Cập nhật thành công."));
        }

        /// <summary>DELETE /api/admin/staff/{id} — Vô hiệu hoá nhân viên (soft delete)</summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            await _staffService.DeleteStaffAsync(id);
            return Ok(ApiResponse<object>.Ok(null!, "Nhân viên đã bị vô hiệu hoá."));
        }

        /// <summary>DELETE /api/admin/staff/{id}/permanent — Xóa vĩnh viễn nhân viên</summary>
        [HttpDelete("{id:int}/permanent")]
        public async Task<ActionResult<ApiResponse<object>>> HardDelete(int id)
        {
            await _staffService.HardDeleteStaffAsync(id);
            return Ok(ApiResponse<object>.Ok(null!, "Nhân viên đã bị xóa vĩnh viễn khỏi hệ thống."));
        }

        // ═══════════════════════════════════════════════════════════
        // PITCH ASSIGNMENT
        // ═══════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/staff/{id}/pitches — Xem sân được phân công</summary>
        [HttpGet("{id:int}/pitches")]
        public async Task<ActionResult<ApiResponse<List<AssignedPitchResponse>>>> GetPitches(int id)
        {
            var result = await _staffService.GetAssignedPitchesAsync(id);
            return Ok(ApiResponse<List<AssignedPitchResponse>>.Ok(result));
        }

        /// <summary>POST /api/admin/staff/{id}/pitches — Phân công sân cho nhân viên</summary>
        [HttpPost("{id:int}/pitches")]
        public async Task<ActionResult<ApiResponse<AssignedPitchResponse>>> AssignPitch(
            int id,
            [FromBody] AssignPitchRequest request)
        {
            var result = await _staffService.AssignPitchAsync(id, request);
            return Ok(ApiResponse<AssignedPitchResponse>.Ok(result, "Phân công sân thành công."));
        }

        /// <summary>DELETE /api/admin/staff/{id}/pitches/{pitchId} — Thu hồi phân công sân</summary>
        [HttpDelete("{id:int}/pitches/{pitchId:int}")]
        public async Task<ActionResult<ApiResponse<object>>> UnassignPitch(int id, int pitchId)
        {
            await _staffService.UnassignPitchAsync(id, pitchId);
            return Ok(ApiResponse<object>.Ok(null!, "Đã thu hồi phân công sân. Các ca làm liên quan cũng bị xoá."));
        }

        // ═══════════════════════════════════════════════════════════
        // SHIFT MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/staff/{id}/shifts — Xem toàn bộ ca làm của nhân viên</summary>
        [HttpGet("{id:int}/shifts")]
        public async Task<ActionResult<ApiResponse<List<ShiftResponse>>>> GetShifts(int id)
        {
            var result = await _staffService.GetShiftsAsync(id);
            return Ok(ApiResponse<List<ShiftResponse>>.Ok(result));
        }

        /// <summary>POST /api/admin/staff/{id}/shifts — Tạo ca làm việc mới</summary>
        [HttpPost("{id:int}/shifts")]
        public async Task<ActionResult<ApiResponse<ShiftResponse>>> CreateShift(
            int id,
            [FromBody] CreateShiftRequest request)
        {
            var result = await _staffService.CreateShiftAsync(id, request);
            return Ok(ApiResponse<ShiftResponse>.Ok(result, "Tạo ca làm việc thành công."));
        }

        /// <summary>PUT /api/admin/staff/{id}/shifts/{shiftId} — Cập nhật ca làm việc</summary>
        [HttpPut("{id:int}/shifts/{shiftId:int}")]
        public async Task<ActionResult<ApiResponse<ShiftResponse>>> UpdateShift(
            int id,
            int shiftId,
            [FromBody] UpdateShiftRequest request)
        {
            var result = await _staffService.UpdateShiftAsync(id, shiftId, request);
            return Ok(ApiResponse<ShiftResponse>.Ok(result, "Cập nhật ca làm việc thành công."));
        }

        /// <summary>DELETE /api/admin/staff/{id}/shifts/{shiftId} — Xoá ca làm việc</summary>
        [HttpDelete("{id:int}/shifts/{shiftId:int}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteShift(int id, int shiftId)
        {
            await _staffService.DeleteShiftAsync(id, shiftId);
            return Ok(ApiResponse<object>.Ok(null!, "Đã xoá ca làm việc."));
        }
    }
}
