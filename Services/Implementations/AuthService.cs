using System.Security.Cryptography;
using FootballBooking_BE.Common;
using FootballBooking_BE.Data.Entities;
using FootballBooking_BE.Models.DTOs.Auth;
using FootballBooking_BE.Repositories.Interfaces;
using FootballBooking_BE.Services.Interfaces;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Caching.Memory;

namespace FootballBooking_BE.Services.Implementations
{
    public class AuthService :IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

        private readonly int _refreshTokenDays;
        
        // ── OTP Settings ──────────────────────────────────────────────────────────
        private const int OtpExpiryMinutes = 5;
        private const string OtpCachePrefix = "otp_";
        private const string ResetTokenCachePrefix = "reset_";

        public AuthService(
            IAuthRepository authRepo,
            IJwtService jwtService,
            IConfiguration config,
            ILogger<AuthService> logger,
            IEmailService emailService,
            IMemoryCache cache)
        {
            _authRepo = authRepo;
            _jwtService = jwtService;
            _config = config;
            _logger = logger;
            _emailService = emailService;
            _cache = cache;
            _refreshTokenDays = int.Parse(config["Jwt:RefreshTokenExpiryDays"] ?? "7");
        }

        // ─── REGISTER ─────────────────────────────────────────────────

        public async Task<AuthResponse> RegisterAsync(Models.DTOs.Auth.RegisterRequest request, string ipAddress)
        {
            if (await _authRepo.EmailExistsAsync(request.Email))
                throw new InvalidOperationException("Email đã được sử dụng.");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email.ToLower().Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Customer,
                Phone = request.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _authRepo.CreateUserAsync(user);
            return await BuildAuthResponseAsync(created, ipAddress);
        }

        // ─── LOGIN ────────────────────────────────────────────────────

        public async Task<AuthResponse> LoginAsync(Models.DTOs.Auth.LoginRequest request, string ipAddress)
        {
            var normalizedEmail = request.Email.ToLower().Trim();
            var user = await _authRepo.GetByEmailAsync(normalizedEmail)
                ?? throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Tài khoản đã bị khoá. Vui lòng liên hệ admin.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

            return await BuildAuthResponseAsync(user, ipAddress);
        }

        // ─── REFRESH TOKEN ────────────────────────────────────────────

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken)
                ?? throw new UnauthorizedAccessException("Access token không hợp lệ.");

            var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                           ?? principal.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int accessTokenUserId))
                throw new UnauthorizedAccessException("Access token không hợp lệ.");

            var storedToken = await _authRepo.GetRefreshTokenAsync(request.RefreshToken)
                ?? throw new UnauthorizedAccessException("Refresh token không tồn tại.");

            if (!storedToken.IsActive)
                throw new UnauthorizedAccessException("Refresh token đã hết hạn hoặc bị thu hồi.");

            var user = storedToken.User;

            // Security Check: UserId from AccessToken MUST match UserId from RefreshToken
            if (user.UserId != accessTokenUserId)
                throw new UnauthorizedAccessException("Token không khớp. Truy cập bị từ chối.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Tài khoản đã bị khoá.");

            // Tạo refresh token mới (Rotation)
            var newRefreshToken = await CreateRefreshTokenAsync(user.UserId, ipAddress);

            // Thu hồi token cũ
            await _authRepo.RevokeRefreshTokenAsync(
                storedToken,
                revokedByIp: ipAddress,
                replacedByToken: newRefreshToken.Token);

            // Tạo access token mới
            var accessToken = _jwtService.GenerateAccessToken(user);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(
                    int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "60")),
                User = MapToProfile(user)
            };
        }

        // ─── LOGOUT ───────────────────────────────────────────────────

        public async Task LogoutAsync(string refreshToken, string ipAddress)
        {
            var token = await _authRepo.GetRefreshTokenAsync(refreshToken);
            if (token != null && token.IsActive)
                await _authRepo.RevokeRefreshTokenAsync(token, ipAddress);
        }

        public async Task LogoutAllDevicesAsync(int userId)
            => await _authRepo.RevokeAllUserTokensAsync(userId);

        // ─── PROFILE ──────────────────────────────────────────────────

        public async Task<UserProfileResponse> GetProfileAsync(int userId)
        {
            var user = await _authRepo.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Người dùng không tồn tại.");

            return MapToProfile(user);
        }

        public async Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var user = await _authRepo.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Người dùng không tồn tại.");

            user.FullName = request.FullName;
            user.Phone = request.Phone;

            var updated = await _authRepo.UpdateUserAsync(user);
            return MapToProfile(updated);
        }

        // ─── CHANGE PASSWORD ──────────────────────────────────────────

        public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _authRepo.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Người dùng không tồn tại.");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
                throw new InvalidOperationException("Mật khẩu hiện tại không đúng.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _authRepo.UpdateUserAsync(user);

            // Thu hồi toàn bộ refresh token (buộc đăng nhập lại)
            await _authRepo.RevokeAllUserTokensAsync(userId);
        }

        // ─── FORGOT / RESET PASSWORD (OTP FLOW) ───────────────────────

        public async Task SendOtpAsync(Models.DTOs.Auth.ForgotPasswordRequest request)
        {
            var user = await _authRepo.GetByEmailAsync(request.Email);
            if (user == null || !user.IsActive) return;

            // Generate 6 digit OTP
            var otp = GenerateOtp();
            
            // Save to cache
            var cacheKey = OtpCachePrefix + request.Email.ToLower().Trim();
            _cache.Set(cacheKey, otp, TimeSpan.FromMinutes(OtpExpiryMinutes));

            // Send Email
            await _emailService.SendOtpAsync(user.Email, otp);
            _logger.LogInformation("Sent OTP to {Email}", user.Email);
        }

        public Task<string> VerifyOtpAsync(Models.DTOs.Auth.VerifyOtpRequest request)
        {
            var cacheKey = OtpCachePrefix + request.Email.ToLower().Trim();

            if (!_cache.TryGetValue(cacheKey, out string? storedOtp) || storedOtp != request.Otp)
                throw new InvalidOperationException("Mã OTP không hợp lệ hoặc đã hết hạn.");

            _cache.Remove(cacheKey);

            // Generate reset token
            var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var resetKey = ResetTokenCachePrefix + resetToken;
            _cache.Set(resetKey, request.Email.ToLower().Trim(), TimeSpan.FromMinutes(15)); // Token valid for 15 mins

            return Task.FromResult(resetToken);
        }

        public async Task ResetPasswordAsync(Models.DTOs.Auth.ResetPasswordRequest request)
        {
            var resetKey = ResetTokenCachePrefix + request.Token;

            if (!_cache.TryGetValue(resetKey, out string? email) || string.IsNullOrEmpty(email))
                throw new UnauthorizedAccessException("Token đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
            
            if (!email.Equals(request.Email.ToLower().Trim(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Token không khớp với email.");

            var user = await _authRepo.GetByEmailAsync(email)
                ?? throw new KeyNotFoundException("Người dùng không tồn tại.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _authRepo.UpdateUserAsync(user);

            // Clean up cache & revoke sessions
            _cache.Remove(resetKey);
            await _authRepo.RevokeAllUserTokensAsync(user.UserId);
        }

        // ─── PRIVATE HELPERS ──────────────────────────────────────────

        private async Task<AuthResponse> BuildAuthResponseAsync(User user, string ipAddress)
        {
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user.UserId, ipAddress);
            var expiryMinutes = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "60");

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes),
                User = MapToProfile(user)
            };
        }

        private async Task<RefreshToken> CreateRefreshTokenAsync(int userId, string ipAddress)
        {
            var token = new RefreshToken
            {
                UserId = userId,
                Token = _jwtService.GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenDays),
                CreatedByIp = ipAddress
            };

            return await _authRepo.CreateRefreshTokenAsync(token);
        }

        private static string GenerateOtp()
        {
            var bytes = RandomNumberGenerator.GetBytes(4);
            var number = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
            return number.ToString("D6");
        }

        private static UserProfileResponse MapToProfile(User user) => new()
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Phone = user.Phone,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
