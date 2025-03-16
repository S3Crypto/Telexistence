using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TelexistenceAPI.Core.Interfaces;
using TelexistenceAPI.Core.Services;
using TelexistenceAPI.Middleware;
using TelexistenceAPI.Repositories;

namespace TelexistenceAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // Add MongoDB
            services.AddSingleton<MongoDbContext>();

            // Add repositories
            services.AddScoped<IRobotRepository, RobotRepository>();
            services.AddScoped<ICommandRepository, CommandRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Add services
            services.AddScoped<IRobotService, RobotService>();
            services.AddScoped<ICommandService, CommandService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var jwtKey = configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopment";
            var jwtIssuer = configuration["Jwt:Issuer"] ?? "TelexistenceAPI";
            var jwtAudience = configuration["Jwt:Audience"] ?? "TelexistenceClients";

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };
                });

            return services;
        }

        public static IServiceCollection AddCorsPolicy(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var allowedOrigins =
                configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:3000" };

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            return services;
        }

        public static IServiceCollection AddAppHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks().AddCheck<APIHealthCheck>("API");

            return services;
        }
    }
}
