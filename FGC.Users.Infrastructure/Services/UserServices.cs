using FGC.Users.Domain.Entities;
using FGC.Users.Domain.Interfaces;
using FGC.Users.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FGC.Users.Infrastructure.Services
{
    public class UserUniquenessService : IUserUniquenessService
    {
        private readonly IUserRepository _userRepository;

        public UserUniquenessService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<bool> IsEmailTakenAsync(Email email)
        {
            if (email == null)
                return false;

            return await _userRepository.ExistsByEmailAsync(email);
        }
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expireMinutes;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurada");
            _issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer não configurado");
            _audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience não configurado");
            _expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "120");

            if (_secretKey.Length < 32)
                throw new InvalidOperationException("JWT SecretKey deve ter pelo menos 32 caracteres");
        }

        public string GenerateToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!user.IsActive)
                throw new InvalidOperationException("Não é possível gerar token para usuário inativo");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email.Value),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public Guid? GetUserIdFromToken(string token)
        {
            var principal = ValidateToken(token);

            if (principal == null)
                return null;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
