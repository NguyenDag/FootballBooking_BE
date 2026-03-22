using FootballBooking_BE.Common;
using FootballBooking_BE.Models.DTOs;
using FootballBooking_BE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballBooking_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BookingResponse>>> CreateBooking(BookingCreateRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookingService.CreateBookingAsync(userId, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("my-bookings")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingResponse>>>> GetMyBookings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookingService.GetMyBookingsAsync(userId);
            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingResponse>>>> GetBookingHistory()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookingService.GetBookingHistoryAsync(userId);
            return Ok(result);
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingResponse>>>> GetUpcomingBookings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookingService.GetUpcomingBookingsAsync(userId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<BookingResponse>>> GetBookingById(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookingService.GetBookingByIdAsync(userId, id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPut("details/{detailId}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelBooking(int detailId, CancelBookingRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookingService.CancelBookingAsync(userId, detailId, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("staff/details/{detailId}/reject")]
        [Authorize(Policy = "StaffOrAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> StaffRejectBooking(int detailId, CancelBookingRequest request)
        {
            var staffId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookingService.StaffCancelBookingAsync(staffId, detailId, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("staff/details/{detailId}/confirm")]
        [Authorize(Policy = "StaffOrAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> StaffConfirmBooking(int detailId)
        {
            var staffId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookingService.StaffConfirmBookingAsync(staffId, detailId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("staff/bulk-cancel")]
        [Authorize(Policy = "StaffOrAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> BulkCancel(BulkCancelBookingRequest request)
        {
            var staffId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookingService.BulkCancelByPitchAsync(staffId, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("staff/daily")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingResponse>>>> GetStaffDailyBookings([FromQuery] DateOnly? date)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized(ApiResponse<object>.Fail("Không tìm thấy thông tin định danh."));
            
            var staffId = int.Parse(userIdStr);
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.Now);
            var result = await _bookingService.GetStaffBookingsByDateAsync(staffId, targetDate);
            return Ok(result);
        }

        [HttpGet("staff/pending")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> GetStaffPendingBookings()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized(ApiResponse<object>.Fail("Không tìm thấy thông tin định danh."));

            var staffId = int.Parse(userIdStr);
            var result = await _bookingService.GetStaffPendingBookingsAsync(staffId);
            return Ok(result);
        }

        [HttpGet("staff/all")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> GetStaffAllBookings([FromQuery] DateOnly? date = null)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized(ApiResponse<object>.Fail("Không tìm thấy thông tin định danh."));

            var staffId = int.Parse(userIdStr);
            var result = await _bookingService.GetStaffAllBookingsAsync(staffId, date);
            return Ok(result);
        }

        [HttpGet("pitch/{pitchId}/date/{date}")]
        public async Task<IActionResult> GetPitchBookingsByDate(int pitchId, string date)
        {
            if (!DateOnly.TryParse(date, out var dateOnly)) return BadRequest("Định dạng ngày không hợp lệ.");
            var result = await _bookingService.GetPitchBookingsByDateAsync(pitchId, dateOnly);
            return Ok(result);
        }

        [HttpGet("availability")]
        [AllowAnonymous] // Cho phép khách xem lịch trống mà không cần login
        public async Task<ActionResult<ApiResponse<IEnumerable<AvailabilitySlot>>>> GetAvailability(int pitchId, DateOnly playDate)
        {
            var result = await _bookingService.GetAvailableSlotsAsync(pitchId, playDate);
            return Ok(result);
        }
    }
}
