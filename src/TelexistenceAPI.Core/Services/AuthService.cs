using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TelexistenceAPI.Core.Interfaces;

namespace TelexistenceAPI.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<AuthService> logger
        )
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(string token, DateTime expiration)> AuthenticateAsync(
            string username,
            string password
        )
        {
            try
            {
                _logger.LogInformation("Attempting to authenticate user: {Username}", username);

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning(
                        "Authentication failed: User not found: {Username}",
                        username
                    );
                    throw new UnauthorizedAccessException("Invalid username or password");
                }

                if (!VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogWarning(
                        "Authentication failed: Incorrect password for user: {Username}",
                        username
                    );
                    throw new UnauthorizedAccessException("Invalid username or password");
                }

                var token = GenerateJwtToken(user.Id, user.Username, user.Roles);
                var expiration = DateTime.UtcNow.AddHours(1);

                _logger.LogInformation("User {Username} authenticated successfully", username);
                return (token, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for user {Username}", username);
                throw;
            }
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        private string GenerateJwtToken(string userId, string username, List<string> roles)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userId),
                    new Claim(JwtRegisteredClaimNames.UniqueName, username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var jwtKey = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                {
                    _logger.LogError("JWT Key configuration is missing or empty");
                    jwtKey = "DefaultSecretKeyForDevelopment";
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.UtcNow.AddHours(1);

                var issuer = _configuration["Jwt:Issuer"] ?? "TelexistenceAPI";
                var audience = _configuration["Jwt:Audience"] ?? "TelexistenceClients";

                _logger.LogInformation(
                    "Generating JWT token for user {Username} with issuer {Issuer} and audience {Audience}",
                    username,
                    issuer,
                    audience
                );

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: expires,
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user {UserId}", userId);
                throw;
            }
        }
    }
}
