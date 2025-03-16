using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TelexistenceAPI.Controllers;
using TelexistenceAPI.Core.Interfaces;
using TelexistenceAPI.DTOs;
using Xunit;

namespace TelexistenceAPI.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var request = new LoginRequestDto { Username = "admin", Password = "admin123" };

            var expectedToken = "jwt-token";
            var expiration = DateTime.UtcNow.AddHours(1);

            _mockAuthService
                .Setup(s => s.AuthenticateAsync(request.Username, request.Password))
                .ReturnsAsync((expectedToken, expiration));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<LoginResponseDto>(okResult.Value);

            Assert.Equal(expectedToken, response.Token);
            Assert.Equal(expiration, response.Expiration);
            Assert.Equal(request.Username, response.Username);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequestDto { Username = "invalid", Password = "invalid" };

            _mockAuthService
                .Setup(s => s.AuthenticateAsync(request.Username, request.Password))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task Login_ServiceException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new LoginRequestDto { Username = "admin", Password = "admin123" };

            _mockAuthService
                .Setup(s => s.AuthenticateAsync(request.Username, request.Password))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}
