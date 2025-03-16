using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelexistenceAPI.DTOs;
using TelexistenceAPI.Repositories;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TelexistenceAPI.Core.Interfaces;
using TelexistenceAPI.Core.Entities;
using Moq;

namespace TelexistenceAPI.IntegrationTests
{
    [Collection("IntegrationTests")]
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private string? _authToken;

        // Define a consistent secret key for JWT auth
        private const string JwtSecretKey = "TestingKeyThatIsAtLeast32BytesLong1234567890";

        // Mocks for services
        private readonly Mock<ICommandService> _mockCommandService = new Mock<ICommandService>();
        private readonly Mock<IRobotService> _mockRobotService = new Mock<IRobotService>();
        private readonly Mock<IRobotRepository> _mockRobotRepository = new Mock<IRobotRepository>();

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // Setup mocks
            SetupMocks();

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    (context, config) =>
                    {
                        config.AddInMemoryCollection(
                            new Dictionary<string, string?>
                            {
                                ["ConnectionStrings:MongoDB"] = "mongodb://localhost:27017",
                                ["MongoDB:DatabaseName"] = "TelexistenceTestDB",
                                ["Jwt:Key"] = JwtSecretKey,
                                ["Jwt:Issuer"] = "TelexistenceAPITest",
                                ["Jwt:Audience"] = "TelexistenceClientsTest"
                            }
                        );
                    }
                );

                builder.ConfigureServices(services =>
                {
                    // Replace real services with mocks
                    services.AddScoped<IRobotService>(_ => _mockRobotService.Object);
                    services.AddScoped<ICommandService>(_ => _mockCommandService.Object);
                    services.AddScoped<IRobotRepository>(_ => _mockRobotRepository.Object);

                    // Configure JWT authentication with the same key for token generation and validation
                    services.Configure<JwtBearerOptions>(
                        JwtBearerDefaults.AuthenticationScheme,
                        options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = "TelexistenceAPITest",
                                ValidAudience = "TelexistenceClientsTest",
                                IssuerSigningKey = new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(JwtSecretKey)
                                )
                            };
                        }
                    );
                });
            });

            _client = _factory.CreateClient();

            // Authenticate immediately so all tests have access to token
            _authToken = LoginAsync().GetAwaiter().GetResult();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _authToken
            );
        }

        private void SetupMocks()
        {
            // Setup robot repository
            var robots = new List<Robot>
            {
                new Robot
                {
                    Id = "1",
                    Name = "TX-010",
                    CurrentPosition = new Position
                    {
                        X = 0,
                        Y = 0,
                        Z = 0,
                        Rotation = 0
                    },
                    Status = "Idle",
                    LastUpdated = DateTime.UtcNow
                },
                new Robot
                {
                    Id = "2",
                    Name = "TX-020",
                    CurrentPosition = new Position
                    {
                        X = 5,
                        Y = 5,
                        Z = 0,
                        Rotation = 90
                    },
                    Status = "Idle",
                    LastUpdated = DateTime.UtcNow
                }
            };

            _mockRobotRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(robots);
            _mockRobotRepository.Setup(r => r.GetAsync("1")).ReturnsAsync(robots[0]);
            _mockRobotRepository.Setup(r => r.GetAsync("2")).ReturnsAsync(robots[1]);
            _mockRobotRepository.Setup(r => r.GetAsync("999")).ReturnsAsync((Robot)null);

            // Setup robot service
            _mockRobotService.Setup(s => s.GetAllRobotsAsync()).ReturnsAsync(robots);
            _mockRobotService.Setup(s => s.GetRobotAsync("1")).ReturnsAsync(robots[0]);
            _mockRobotService.Setup(s => s.GetRobotAsync("2")).ReturnsAsync(robots[1]);
            _mockRobotService.Setup(s => s.GetRobotAsync("999")).ReturnsAsync((Robot)null);

            // Setup command service
            var commandId = Guid.NewGuid().ToString();
            var command = new Command
            {
                Id = commandId,
                CommandType = "move",
                RobotId = "1",
                UserId = "1",
                Status = "Completed",
                CreatedAt = DateTime.UtcNow,
                ExecutedAt = DateTime.UtcNow.AddSeconds(1),
                Parameters = new Dictionary<string, object>
                {
                    { "direction", "forward" },
                    { "distance", 1.0 }
                }
            };

            _mockCommandService
                .Setup(
                    s =>
                        s.CreateCommandAsync(
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<Dictionary<string, object>>()
                        )
                )
                .ReturnsAsync(command);

            _mockCommandService
                .Setup(s => s.ExecuteCommandAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _mockCommandService
                .Setup(s => s.GetCommandAsync(It.IsAny<string>()))
                .ReturnsAsync(command);

            _mockCommandService
                .Setup(s => s.GetCommandHistoryAsync("1", It.IsAny<int>()))
                .ReturnsAsync(new List<Command> { command });
        }

        private async Task<string> LoginAsync()
        {
            var loginRequest = new LoginRequestDto { Username = "admin", Password = "admin123" };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            return content?.Token
                ?? throw new InvalidOperationException("Failed to obtain authentication token");
        }

        [Fact]
        public async Task CheckHealth_ReturnsHealthyStatus()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            jsonDoc.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
        }

        [Fact]
        public async Task GetRobotStatus_ReturnsOkWithRobotData()
        {
            // Act
            var response = await _client.GetAsync("/api/status/1");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<RobotStatusDto>();

            content.Should().NotBeNull();
            content!.Id.Should().Be("1");
            content.Name.Should().Be("TX-010");
        }

        [Fact]
        public async Task GetAllRobotStatuses_ReturnsOkWithRobotsList()
        {
            // Act
            var response = await _client.GetAsync("/api/status");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<List<RobotStatusDto>>();

            content.Should().NotBeNull();
            content.Should().HaveCountGreaterThanOrEqualTo(2);
            content.Should().Contain(r => r.Id == "1" && r.Name == "TX-010");
            content.Should().Contain(r => r.Id == "2" && r.Name == "TX-020");
        }

        [Fact]
        public async Task CreateAndExecuteCommand_ReturnsOkWithCompletedCommand()
        {
            // Arrange
            var command = new CommandRequestDto
            {
                Command = "move",
                Robot = "1",
                Parameters = new Dictionary<string, object>
                {
                    { "direction", "forward" },
                    { "distance", 1.0 }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/command", command);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<CommandResponseDto>();

            content.Should().NotBeNull();
            content!.Command.Should().Be("move");
            content.Robot.Should().Be("1");
            content.Status.Should().Be("Completed");
            content.ExecutedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task GetNonExistentRobot_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/status/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AccessEndpointWithoutAuth_ReturnsUnauthorized()
        {
            // Arrange - create a new client without auth
            var unauthClient = _factory.CreateClient();

            // Act
            var response = await unauthClient.GetAsync("/api/status");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
