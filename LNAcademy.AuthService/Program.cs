using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using LNAcademy.AuthService.Data;
using LNAcademy.AuthService.Helpers;
using System;
using System.Text;
using LNAcademy.AuthService.Middleware;
using LNAcademy.AuthService.Models;
using LNAcademy.AuthService.Repositories;
using LNAcademy.AuthService.Services;
using Microsoft.IdentityModel.Tokens;

// Load environment variables from .env file (if it exists)
try
{
    DotEnv.Load(options: new DotEnvOptions(probeForEnv: true, probeLevelsToSearch: 5));
    Console.WriteLine("Environment variables loaded successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Failed to load .env file: {ex.Message}");
    // Continue execution - we'll check for required variables later
}

var builder = WebApplication.CreateBuilder(args);

// Create a logger factory early for startup logging
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
var startupLogger = loggerFactory.CreateLogger("Startup");

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Configure health checks that don't depend on database
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

// Try to set up database connection - fail fast if not configured
string connectionString;
try
{
    connectionString = DbConnectionHelper.GetConnectionString(builder.Configuration);
    startupLogger.LogInformation("Database connection string configured successfully");
}
catch (Exception ex)
{
    startupLogger.LogCritical(ex, "Failed to configure database connection");
    // Always fail if database connection isn't available
    startupLogger.LogCritical("Application startup aborted due to missing database configuration");
    return 1; // Exit with error code
}

// Add database context
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add database health check
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString,
        name: "database",
        tags: new[] { "db", "postgresql" });

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT
builder.Services.Configure<JwtSettings>(options => 
{
    // Get JWT secret from environment variable
    var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
    
    if (string.IsNullOrWhiteSpace(jwtSecret))
    {
        var errorMessage = "JWT_SECRET environment variable is not set. This is required for secure operation.";
        startupLogger.LogCritical(errorMessage);
        throw new InvalidOperationException(errorMessage);
    }
    
    // Get other settings from config but override the secret
    options.Secret = jwtSecret;
    options.Issuer = builder.Configuration["JwtSettings:Issuer"] ?? "LNAcademy";
    options.Audience = builder.Configuration["JwtSettings:Audience"] ?? "LNAcademyClients";
    
    // Parse expiry minutes with fallback to 60 minutes
    if (!int.TryParse(builder.Configuration["JwtSettings:ExpiryMinutes"], out int expiryMinutes))
    {
        expiryMinutes = 60;
    }
    options.ExpiryMinutes = expiryMinutes;
    
    startupLogger.LogInformation("JWT configuration loaded successfully");
});

builder.Services.AddScoped<ITokenService, TokenService>();

// Add authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "LNAcademy",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "LNAcademyClients",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});


// Repos
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();

WebApplication app;

try
{
    // Build the application
    app = builder.Build();
    
    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }
    
    // Use CORS
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    
    // Health check endpoint
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        },
        AllowCachingResponses = false
    }).AllowAnonymous();
    
    // Controllers
    app.UseMiddleware<ValidationMiddleware>();
    app.MapControllers();
    
    // Test database connection - always fail if connection test fails
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();
            var dbContext = services.GetRequiredService<AuthDbContext>();
            
            logger.LogInformation("Testing database connection...");
            if (dbContext.Database.CanConnect())
            {
                logger.LogInformation("Database connection successful");
            }
            else
            {
                logger.LogCritical("Database connection test failed");
                return 1; // Exit with error code
            }
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(ex, "Application startup failed due to database connection error");
        return 1; // Exit with error code
    }
    
    startupLogger.LogInformation("Application startup complete");
}
catch (Exception ex)
{
    startupLogger.LogCritical(ex, "Application failed to start");
    return 1; // Exit with error code
}

// Run the application
try
{
    app.Run();
    return 0;
}
catch (Exception ex)
{
    startupLogger.LogCritical(ex, "Unhandled exception during application execution");
    return 1;
}