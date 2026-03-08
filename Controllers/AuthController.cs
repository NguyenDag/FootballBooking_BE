using System.Security.Claims;
using FootballBooking_BE.Common;
using FootballBooking_BE.Models.DTOs.Auth;
using FootballBooking_BE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using ForgotPasswordRequest = FootballBooking_BE.Models.DTOs.Auth.ForgotPasswordRequest;
using LoginRequest = FootballBooking_BE.Models.DTOs.Auth.LoginRequest;
using ResetPasswordRequest = FootballBooking_BE.Models.DTOs.Auth.ResetPasswordRequest;

namespace FootballBooking_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/auth/register
        // ─────────────────────────────────────────────────────────────
        /// <summary>Đăng ký tài khoản mới (role = CUSTOMER)</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] Models.DTOs.Auth.RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request, GetIpAddress());
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Đăng ký thành công."));
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/auth/login
        // ─────────────────────────────────────────────────────────────
        /// <summary>Đăng nhập, trả về access token + refresh token</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request, GetIpAddress());
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Đăng nhập thành công."));
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/auth/refresh-token
        // ─────────────────────────────────────────────────────────────
        /// <summary>Làm mới access token bằng refresh token</summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request, GetIpAddress());
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Làm mới token thành công."));
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/auth/logout
        // ─────────────────────────────────────────────────────────────
        /// <summary>Đăng xuất thiết bị hiện tại (thu hồi refresh token)</summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] string refreshToken)
        {
            await _authService.LogoutAsync(refreshToken, GetIpAddress());
            return Ok(ApiResponse<object>.Ok(null!, "Đăng xuất thành công."));
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/auth/logout-all
        // ─────────────────────────────────────────────────────────────
        /// <summary>Đăng xuất tất cả thiết bị</summary>
        [HttpPost("logout-all")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> LogoutAll()
        {
            await _authService.LogoutAllDevicesAsync(GetCurrentUserId());
            return Ok(ApiResponse<object>.Ok(null!, "Đã đăng xuất khỏi tất cả thiết bị."));
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/auth/me
        // ─────────────────────────────────────────────────────────────
        /// <summary>Lấy thông tin profile của user đang đăng nhập</summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetProfile()
        {
            var result = await _authService.GetProfileAsync(GetCurrentUserId());
            return Ok(ApiResponse<UserProfileResponse>.Ok(result));
        }

        // ─────────────────────────────────────────────────────────────
        // PUT /api/auth/me
        // ─────────────────────────────────────────────────────────────
        /// <summary>Cập nhật thông tin cá nhân</summary>
        [HttpPut("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserProfileResponse>>> UpdateProfile(
            [FromBody] UpdateProfileRequest request)
        {
            var result = await _authService.UpdateProfileAsync(GetCurrentUserId(), request);
            return Ok(ApiResponse<UserProfileResponse>.Ok(result, "Cập nhật thông tin thành công."));
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/auth/change-password
        // ─────────────────────────────────────────────────────────────
        /// <summary>Đổi mật khẩu (yêu cầu đăng nhập lại sau khi đổi)</summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
            [FromBody] ChangePasswordRequest request)
        {
            await _authService.ChangePasswordAsync(GetCurrentUserId(), request);
            return Ok(ApiResponse<object>.Ok(null!, "Đổi mật khẩu thành công. Vui lòng đăng nhập lại."));
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/auth/forgot-password
        // ─────────────────────────────────────────────────────────────
        /// <summary>Yêu cầu reset mật khẩu qua email</summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPassword(
            [FromBody] ForgotPasswordRequest request)
        {
            await _authService.ForgotPasswordAsync(request);
            // Luôn trả về 200 để không lộ email có tồn tại không
            return Ok(ApiResponse<object>.Ok(null!,
                "Nếu email tồn tại, hướng dẫn đặt lại mật khẩu đã được gửi."));
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/auth/reset-password
        // ─────────────────────────────────────────────────────────────
        /// <summary>Đặt lại mật khẩu bằng token từ email</summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword(
            [FromBody] ResetPasswordRequest request)
        {
            await _authService.ResetPasswordAsync(request);
            return Ok(ApiResponse<object>.Ok(null!, "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại."));
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("userId");
            return int.Parse(claim!.Value);
        }

        private string GetIpAddress()
        {
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
                return forwarded.ToString();

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
