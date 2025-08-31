using Scalar.AspNetCore;
using SearchBugs.Api.Endpoints;
using SearchBugs.Api.Hubs;
using SearchBugs.Api.Middleware;
using SearchBugs.Api.Services;
using SearchBugs.Application;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Infrastructure;
using SearchBugs.Persistence;
using System.IdentityModel.Tokens.Jwt;

namespace SearchBugs.Api;

public abstract partial class Program
{
    private static void Main(string[] args)
    {
        // Clear the default JWT claim type mapping to preserve original claim names
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();

        builder.Services.AddInfrastructure();
        builder.Services.AddPersistence(builder.Configuration);
        builder.Services.AddApplication();

        // Debug: Log JWT configuration
        var jwtSection = builder.Configuration.GetSection("JwtOptions");
        var jwtSecret = jwtSection["Secret"];
        var jwtIssuer = jwtSection["Issuer"];
        var jwtAudience = jwtSection["Audience"];
        Console.WriteLine($"JWT Config - Issuer: {jwtIssuer}, Audience: {jwtAudience}, Secret Length: {jwtSecret?.Length ?? 0}");

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Add SignalR
        builder.Services.AddSignalR();
        builder.Services.AddScoped<INotificationService, NotificationService>();

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        // Enable CORS first
        app.UseCors("AllowAll");

        // Add authentication and authorization middleware in correct order
        app.UseAuthentication();
        app.UseAuthorization();

        // Add custom middleware after auth
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseStaticFiles();

        app.MapAuthenticationsEndpoints();
        app.MapBugsEndpoints();
        app.MapUserEndpoints();
        app.MapRoleEndpoints();
        app.MapProjectsEndpoints();
        app.MapRepoEndpoints();
        app.MapNotificationEndpoints();
        app.MapTestNotificationEndpoints();
        app.MapAuditLogEndpoints();

        // Map SignalR hub
        app.MapHub<NotificationHub>("/hubs/notifications");

        // app.UseHttpsRedirection();
        app.Run();
    }
}

public abstract partial class Program { }