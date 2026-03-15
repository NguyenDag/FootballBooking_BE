using System.Security.Claims;
using FootballBooking_BE.Common;
using FootballBooking_BE.Models.DTOs.Dashboard;
using FootballBooking_BE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballBooking_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public DashboardController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<DashboardStatsResponse>>> GetStats()
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var result = await _bookingService.GetDashboardStatsAsync(userId, role);
            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            return int.Parse(claim!.Value);
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? UserRole.Customer;
        }
    }
}
