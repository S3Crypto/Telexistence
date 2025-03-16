using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TelexistenceAPI.Middleware
{
    public class APIHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                // We can add more complex checks here if needed
                return Task.FromResult(HealthCheckResult.Healthy("API is healthy"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    new HealthCheckResult(
                        context.Registration.FailureStatus,
                        "API health check failed",
                        ex
                    )
                );
            }
        }
    }
}
