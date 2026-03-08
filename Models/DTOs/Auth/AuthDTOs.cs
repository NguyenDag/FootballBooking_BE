using System.ComponentModel.DataAnnotations;

namespace FootballBooking_BE.Models.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = null!;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(15)]
        public string? Phone { get; set; }
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = null!;
    }

    public class RefreshTokenRequest
    {
        [Required]
        public string AccessToken { get; set; } = null!;

        [Required]
        public string RefreshToken { get; set; } = null!;
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmNewPassword { get; set; } = null!;
    }

    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự")]
        public string NewPassword { get; set; } = null!;

        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmNewPassword { get; set; } = null!;
    }

    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(15)]
        public string? Phone { get; set; }
    }

    // ─── RESPONSE DTOs ───────────────────────────────────────────

    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime AccessTokenExpiry { get; set; }
        public UserProfileResponse User { get; set; } = null!;
    }

    public class UserProfileResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
