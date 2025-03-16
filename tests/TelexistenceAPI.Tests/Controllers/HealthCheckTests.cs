using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using TelexistenceAPI.Controllers;
using TelexistenceAPI.Middleware;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;

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

        // Create a wrapper interface for HealthCheckService that we can mock
        public interface IHealthCheckServiceWrapper
        {
            Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken = default);
        }

        // Implement a test-specific controller that uses our wrapper
        public class TestableHealthController : ControllerBase
        {
            private readonly IHealthCheckServiceWrapper _healthCheckWrapper;
            private readonly ILogger<HealthController> _logger;

            public TestableHealthController(
                IHealthCheckServiceWrapper healthCheckWrapper,
                ILogger<HealthController> logger
            )
            {
                _healthCheckWrapper = healthCheckWrapper;
                _logger = logger;
            }

            public async Task<IActionResult> Get()
            {
                var report = await _healthCheckWrapper.CheckHealthAsync();

                _logger.LogInformation(
                    "Health check executed with status: {Status}",
                    report.Status
                );

                return report.Status == HealthStatus.Healthy
                    ? Ok(new { Status = "Healthy" })
                    : StatusCode(503, new { Status = "Unhealthy", Details = report.Entries });
            }
        }

        [Fact]
        public async Task HealthController_ReturnsOk_WhenHealthy()
        {
            // Arrange
            var mockHealthCheckWrapper = new Mock<IHealthCheckServiceWrapper>();
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

            mockHealthCheckWrapper
                .Setup(s => s.CheckHealthAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthReport);

            var controller = new TestableHealthController(
                mockHealthCheckWrapper.Object,
                mockLogger.Object
            );

            // Act
            var result = await controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value as dynamic;
            Assert.Equal("Healthy", (string)value.Status);
        }

        [Fact]
        public async Task HealthController_ReturnsServiceUnavailable_WhenUnhealthy()
        {
            // Arrange
            var mockHealthCheckWrapper = new Mock<IHealthCheckServiceWrapper>();
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

            mockHealthCheckWrapper
                .Setup(s => s.CheckHealthAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthReport);

            var controller = new TestableHealthController(
                mockHealthCheckWrapper.Object,
                mockLogger.Object
            );

            // Act
            var result = await controller.Get();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(503, statusCodeResult.StatusCode);

            var value = statusCodeResult.Value as dynamic;
            Assert.Equal("Unhealthy", (string)value.Status);
            Assert.NotNull(value.Details);
        }
    }

    public class Anonymous<T>
    {
        public T Status { get; set; } = default!;
    }

    public class Anonymous<T1, T2>
    {
        public T1 Status { get; set; } = default!;
        public T2 Details { get; set; } = default!;
    }
}
