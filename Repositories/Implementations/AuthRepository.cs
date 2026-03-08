using FootballBooking_BE.Data;
using FootballBooking_BE.Data.Entities;
using FootballBooking_BE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FootballBooking_BE.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public AuthRepository(AppDbContext context)
        {
            _context = context;
        }

        // ─── USER ────────────────────────────────────────────────────

        public async Task<User?> GetByEmailAsync(string email)
            => await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        public async Task<User?> GetByIdAsync(int userId)
            => await _context.Users.FindAsync(userId);

        public async Task<bool> EmailExistsAsync(string email)
            => await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        // ─── REFRESH TOKEN ────────────────────────────────────────────

        public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
            => await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);

        public async Task RevokeRefreshTokenAsync(
            RefreshToken token,
            string? revokedByIp = null,
            string? replacedByToken = null)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = revokedByIp;
            token.ReplacedByToken = replacedByToken;

            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllUserTokensAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        // ─── PASSWORD RESET ───────────────────────────────────────────

        public async Task SavePasswordResetTokenAsync(int userId, string token, DateTime expiry)
        {
            // Xoá token cũ của user nếu có
            var existing = await _context.PasswordResetTokens
                .Where(t => t.UserId == userId && !t.IsUsed)
                .ToListAsync();

            _context.PasswordResetTokens.RemoveRange(existing);

            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = expiry
            });

            await _context.SaveChangesAsync();
        }

        public async Task<(int UserId, DateTime Expiry)?> GetPasswordResetTokenAsync(string token)
        {
            var record = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed);

            if (record == null) return null;
            return (record.UserId, record.ExpiresAt);
        }

        public async Task DeletePasswordResetTokenAsync(string token)
        {
            var record = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (record != null)
            {
                record.IsUsed = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
