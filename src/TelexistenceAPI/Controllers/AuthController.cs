using Microsoft.AspNetCore.Mvc;
using TelexistenceAPI.Core.Interfaces;
using TelexistenceAPI.DTOs;

namespace TelexistenceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var (token, expiration) = await _authService.AuthenticateAsync(
                    request.Username,
                    request.Password
                );

                return Ok(
                    new LoginResponseDto
                    {
                        Token = token,
                        Expiration = expiration,
                        Username = request.Username
                    }
                );
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid username or password");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}
