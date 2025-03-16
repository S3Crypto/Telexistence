using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace TelexistenceAPI.Extensions
{
    public static class LoggingExtensions
    {
        public static IHostBuilder AddSerilogLogging(this IHostBuilder hostBuilder)
        {
            hostBuilder.UseSerilog(
                (context, services, configuration) =>
                {
                    configuration.ReadFrom
                        .Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext()
                        .Enrich.WithExceptionDetails()
                        .Enrich.WithMachineName()
                        .Enrich.WithEnvironmentName()
                        .WriteTo.Console();

                    // Configure Application Insights logging if instrumentation key exists
                    var appInsightsKey =
                        context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]
                        ?? context.Configuration["ApplicationInsights:InstrumentationKey"];

                    if (!string.IsNullOrEmpty(appInsightsKey))
                    {
                        configuration.WriteTo.ApplicationInsights(
                            appInsightsKey,
                            new TraceTelemetryConverter(),
                            LogEventLevel.Information
                        );
                    }

                    // For development, write to a file for easier debugging
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        configuration.WriteTo.File(
                            "logs/telexistence-api-.log",
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
                            retainedFileCountLimit: 7,
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
                        );
                    }
                }
            );

            return hostBuilder;
        }
    }
}
