using System.Security.Claims;
using FootballBooking_BE.Common;
using FootballBooking_BE.Models.DTOs.Staff;
using FootballBooking_BE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FootballBooking_BE.Controllers
{
    /// <summary>
    /// STAFF: Tự xem thông tin và xử lý booking trong ca
    /// </summary>
    [Route("api/staff")]
    [ApiController]
    [Authorize(Policy = "StaffOrAdmin")]
    [Produces("application/json")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        // ═══════════════════════════════════════════════════════════
        // STAFF TỰ XEM
        // ═══════════════════════════════════════════════════════════

        /// <summary>GET /api/staff/me — Xem thông tin + sân + ca làm của mình</summary>
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<StaffResponse>>> GetMyProfile()
        {
            var result = await _staffService.GetMyProfileAsync(GetCurrentUserId());
            return Ok(ApiResponse<StaffResponse>.Ok(result));
        }

        /// <summary>GET /api/staff/me/shifts — Xem ca làm của mình</summary>
        [HttpGet("me/shifts")]
        public async Task<ActionResult<ApiResponse<List<ShiftResponse>>>> GetMyShifts()
        {
            var result = await _staffService.GetShiftsAsync(GetCurrentUserId());
            return Ok(ApiResponse<List<ShiftResponse>>.Ok(result));
        }

        /// <summary>GET /api/staff/me/pitches — Xem sân mình phụ trách</summary>
        [HttpGet("me/pitches")]
        public async Task<ActionResult<ApiResponse<List<AssignedPitchResponse>>>> GetMyPitches()
        {
            var result = await _staffService.GetAssignedPitchesAsync(GetCurrentUserId());
            return Ok(ApiResponse<List<AssignedPitchResponse>>.Ok(result));
        }

        // ═══════════════════════════════════════════════════════════
        // XỬ LÝ BOOKING
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/staff/bookings/pending
        /// Danh sách booking PENDING tại các sân mình phụ trách
        /// (có trường IsWithinShift để biết có trong ca không)
        /// </summary>
        [HttpGet("bookings/pending")]
        public async Task<ActionResult<ApiResponse<List<PendingBookingDetailResponse>>>> GetPendingBookings()
        {
            var result = await _staffService.GetPendingBookingsAsync(GetCurrentUserId());
            return Ok(ApiResponse<List<PendingBookingDetailResponse>>.Ok(result));
        }

        /// <summary>
        /// POST /api/staff/bookings/{detailId}/confirm
        /// Xác nhận một booking detail (phải trong ca + đúng sân phân công)
        /// </summary>
        [HttpPost("bookings/{detailId:int}/confirm")]
        public async Task<ActionResult<ApiResponse<object>>> ConfirmBooking(
            int detailId,
            [FromBody] ConfirmBookingDetailRequest request)
        {
            await _staffService.ConfirmBookingDetailAsync(GetCurrentUserId(), detailId, request);
            return Ok(ApiResponse<object>.Ok(null!, "Đã xác nhận booking thành công."));
        }

        /// <summary>
        /// POST /api/staff/bookings/{detailId}/reject
        /// Từ chối một booking detail (phải đúng sân phân công)
        /// </summary>
        [HttpPost("bookings/{detailId:int}/reject")]
        public async Task<ActionResult<ApiResponse<object>>> RejectBooking(
            int detailId,
            [FromBody] RejectBookingDetailRequest request)
        {
            await _staffService.RejectBookingDetailAsync(GetCurrentUserId(), detailId, request);
            return Ok(ApiResponse<object>.Ok(null!, "Đã từ chối booking."));
        }

        // ─── HELPER ───────────────────────────────────────────────

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("userId");
            return int.Parse(claim!.Value);
        }
    }
}
