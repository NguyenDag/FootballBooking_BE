using System.Security.Claims;
using FootballBooking_BE.Data.Entities;

namespace FootballBooking_BE.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        int GetUserIdFromToken(string token);
    }
}
