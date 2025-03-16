using TelexistenceAPI.Middleware;

namespace TelexistenceAPI.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            return app;
        }

        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app)
        {
            app.MapHealthChecks(
                "/health",
                new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    ResponseWriter = HealthCheckResponseWriter.WriteResponse
                }
            );

            return app;
        }

        public static IApplicationBuilder UseApiConfiguration(
            this IApplicationBuilder app,
            IWebHostEnvironment env
        )
        {
            // Global exception handling
            app.UseExceptionHandling();

            // HTTPS redirection
            app.UseHttpsRedirection();

            // CORS
            app.UseCors();

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Route endpoints
            app.MapControllers();

            // Health checks
            app.UseHealthChecks();

            return app;
        }
    }
}
