using FootballBooking_BE.Models.DTOs.Auth;
using Microsoft.AspNetCore.Identity.Data;
using ForgotPasswordRequest = FootballBooking_BE.Models.DTOs.Auth.ForgotPasswordRequest;
using LoginRequest = FootballBooking_BE.Models.DTOs.Auth.LoginRequest;
using RegisterRequest = FootballBooking_BE.Models.DTOs.Auth.RegisterRequest;
using ResetPasswordRequest = FootballBooking_BE.Models.DTOs.Auth.ResetPasswordRequest;

namespace FootballBooking_BE.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress);
        Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
        Task LogoutAsync(string refreshToken, string ipAddress);
        Task LogoutAllDevicesAsync(int userId);

        Task<UserProfileResponse> GetProfileAsync(int userId);
        Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task ChangePasswordAsync(int userId, ChangePasswordRequest request);

        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
    }
}
