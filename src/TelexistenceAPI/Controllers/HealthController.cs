using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TelexistenceAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            HealthCheckService healthCheckService,
            ILogger<HealthController> logger
        )
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var report = await _healthCheckService.CheckHealthAsync();

            _logger.LogInformation("Health check executed with status: {Status}", report.Status);

            return report.Status == HealthStatus.Healthy
                ? Ok(new { Status = "Healthy" })
                : StatusCode(503, new { Status = "Unhealthy", Details = report.Entries });
        }
    }
}
