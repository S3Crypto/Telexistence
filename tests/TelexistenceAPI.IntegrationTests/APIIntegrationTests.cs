using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MongoDb;
using TelexistenceAPI.DTOs;
using TelexistenceAPI.Repositories;
using Xunit;
using FluentAssertions;

namespace TelexistenceAPI.IntegrationTests
{
    public class ApiIntegrationTests : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongoDbContainer;
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private string _authToken;

        public ApiIntegrationTests()
        {
            _mongoDbContainer = new MongoDbBuilder()
                .WithImage("mongo:latest")
                .WithPortBinding(27017, true)
                .Build();
        }

        public async Task InitializeAsync()
        {
            // Start MongoDB container
            await _mongoDbContainer.StartAsync();

            // Configure test host with MongoDB connection
            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    (context, config) =>
                    {
                        config.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["ConnectionStrings:MongoDB"] =
                                    _mongoDbContainer.GetConnectionString(),
                                ["MongoDB:DatabaseName"] = "TelexistenceTestDB",
                                ["Jwt:Key"] = "YourSuperSecretKeyForTestingPurposesOnly",
                                ["Jwt:Issuer"] = "TelexistenceAPITest",
                                ["Jwt:Audience"] = "TelexistenceClientsTest"
                            }
                        );
                    }
                );

                builder.ConfigureServices(services =>
                {
                    // Add any test-specific service configurations here
                });
            });

            _client = _factory.CreateClient();

            // Authenticate and get token for subsequent requests
            await LoginAndSetToken();
        }

        public async Task DisposeAsync()
        {
            await _mongoDbContainer.StopAsync();
            _factory?.Dispose();
            _client?.Dispose();
        }

        private async Task LoginAndSetToken()
        {
            var loginRequest = new LoginRequestDto { Username = "admin", Password = "admin123" };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            _authToken = content.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _authToken
            );
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
            content.Id.Should().Be("1");
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
            content.Command.Should().Be("move");
            content.Robot.Should().Be("1");
            content.Status.Should().Be("Completed");
            content.ExecutedAt.Should().NotBeNull();

            // Verify command history contains the new command
            var historyResponse = await _client.GetAsync($"/api/history/{command.Robot}");
            historyResponse.EnsureSuccessStatusCode();

            var history = await historyResponse.Content.ReadFromJsonAsync<
                List<CommandResponseDto>
            >();
            history.Should().Contain(c => c.Id == content.Id);
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
    }
}
