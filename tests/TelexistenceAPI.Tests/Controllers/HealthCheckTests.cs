using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using TelexistenceAPI.Controllers;
using TelexistenceAPI.Middleware;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TelexistenceAPI.Tests.Controllers
{
    public class HealthCheckTests
    {
        [Fact]
        public async Task APIHealthCheck_ReturnsHealthy_WhenNoErrors()
        {
            // Arrange
            var healthCheck = new APIHealthCheck();
            var context = new HealthCheckContext
            {
                Registration = new HealthCheckRegistration(
                    "Test",
                    healthCheck,
                    HealthStatus.Unhealthy,
                    Array.Empty<string>()
                )
            };

            // Act
            var result = await healthCheck.CheckHealthAsync(context);

            // Assert
            Assert.Equal(HealthStatus.Healthy, result.Status);
            Assert.Equal("API is healthy", result.Description);
        }

        [Fact]
        public async Task HealthController_ReturnsOk_WhenHealthy()
        {
            // Arrange
            var mockHealthCheckService = new Mock<HealthCheckService>();
            var mockLogger = new Mock<ILogger<HealthController>>();

            var healthReport = new HealthReport(
                new Dictionary<string, HealthReportEntry>
                {
                    {
                        "API",
                        new HealthReportEntry(
                            HealthStatus.Healthy,
                            "API is healthy",
                            TimeSpan.FromMilliseconds(1),
                            null,
                            null
                        )
                    }
                },
                TimeSpan.FromMilliseconds(1)
            );

            mockHealthCheckService
                .Setup(s => s.CheckHealthAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthReport);

            var controller = new HealthController(mockHealthCheckService.Object, mockLogger.Object);

            // Act
            var result = await controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<Anonymous<string>>(okResult.Value);
            Assert.Equal("Healthy", value.Status);
        }

        [Fact]
        public async Task HealthController_ReturnsServiceUnavailable_WhenUnhealthy()
        {
            // Arrange
            var mockHealthCheckService = new Mock<HealthCheckService>();
            var mockLogger = new Mock<ILogger<HealthController>>();

            var healthReport = new HealthReport(
                new Dictionary<string, HealthReportEntry>
                {
                    {
                        "API",
                        new HealthReportEntry(
                            HealthStatus.Unhealthy,
                            "API is unhealthy",
                            TimeSpan.FromMilliseconds(1),
                            new Exception("Test exception"),
                            null
                        )
                    }
                },
                TimeSpan.FromMilliseconds(1)
            );

            mockHealthCheckService
                .Setup(s => s.CheckHealthAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthReport);

            var controller = new HealthController(mockHealthCheckService.Object, mockLogger.Object);

            // Act
            var result = await controller.Get();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(503, statusCodeResult.StatusCode);

            var value = Assert.IsType<Anonymous<string, object>>(statusCodeResult.Value);
            Assert.Equal("Unhealthy", value.Status);
            Assert.NotNull(value.Details);
        }
    }

    // Helper class for anonymous types in tests
    public class Anonymous<T>
    {
        public T Status { get; set; }
    }

    public class Anonymous<T1, T2>
    {
        public T1 Status { get; set; }
        public T2 Details { get; set; }
    }
}
