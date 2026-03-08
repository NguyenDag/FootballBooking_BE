using FootballBooking_BE.Data.Entities;

namespace FootballBooking_BE.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        // User
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);

        // RefreshToken
        Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken token);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(RefreshToken token, string? revokedByIp = null, string? replacedByToken = null);
        Task RevokeAllUserTokensAsync(int userId);

        // PasswordReset (lưu token reset vào cache/db)
        Task SavePasswordResetTokenAsync(int userId, string token, DateTime expiry);
        Task<(int UserId, DateTime Expiry)?> GetPasswordResetTokenAsync(string token);
        Task DeletePasswordResetTokenAsync(string token);
    }
}
