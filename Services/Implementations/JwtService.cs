using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FootballBooking_BE.Data.Entities;
using FootballBooking_BE.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace FootballBooking_BE.Services.Implementations
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenMinutes;

        public JwtService(IConfiguration config)
        {
            _config = config;
            _secretKey = config["Jwt:SecretKey"]!;
            _issuer = config["Jwt:Issuer"]!;
            _audience = config["Jwt:Audience"]!;
            _accessTokenMinutes = int.Parse(config["Jwt:AccessTokenExpiryMinutes"] ?? "60");
        }

        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email,          user.Email),
            new(ClaimTypes.Name,           user.FullName),
            new(ClaimTypes.Role,           user.Role),
            new("userId",                  user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // ← Cho phép token đã hết hạn
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                                               Encoding.UTF8.GetBytes(_secretKey))
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }

        public int GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId");
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}
