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

        public static IApplicationBuilder ConfigureHealthChecks(this IApplicationBuilder app)
        {
            // Health checks are now configured in UseApiConfiguration
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

            // Setup routing - THIS MUST COME BEFORE ENDPOINTS
            app.UseRouting();

            // CORS
            app.UseCors();

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map endpoints - this is where route mapping happens
            app.UseEndpoints(endpoints =>
            {
                // Map controllers
                endpoints.MapControllers();

                // Map health checks
                endpoints.MapHealthChecks(
                    "/health",
                    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                    {
                        ResponseWriter = HealthCheckResponseWriter.WriteResponse
                    }
                );
            });

            return app;
        }
    }
}
