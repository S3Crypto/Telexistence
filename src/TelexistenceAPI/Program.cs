using Microsoft.OpenApi.Models;
using Serilog;
using TelexistenceAPI.Extensions;
using TelexistenceAPI.Middleware;
using TelexistenceAPI.Repositories;
using TelexistenceAPI.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.AddSerilogLogging();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Telexistence API", Version = "v1" });

    // Configure Swagger to use JWT authentication
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        }
    );

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        }
    );
});

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);

// Add JWT authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add CORS
builder.Services.AddCorsPolicy(builder.Configuration);

// Add health checks
builder.Services.AddAppHealthChecks();

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use Serilog request logging
app.UseSerilogRequestLogging();

// Apply API configuration (middleware, auth, etc.)
app.UseApiConfiguration(app.Environment);

// Seed sample data if needed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MongoDbContext>();
        var authService = services.GetRequiredService<IAuthService>();
        await SeedData.InitializeAsync(context, authService);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

// Make Program class accessible to integration tests
public partial class Program { }
