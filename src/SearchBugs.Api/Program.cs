using Scalar.AspNetCore;
using SearchBugs.Api.Endpoints;
using SearchBugs.Api.Extensions;
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
        builder.Services.AddOpenApiWithAuthentication();

        // Configure JSON serialization for UTC dates
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new UtcDateTimeConverter());
            options.SerializerOptions.Converters.Add(new UtcNullableDateTimeConverter());
        });

        builder.Services.AddInfrastructure();
        builder.Services.AddPersistence(builder.Configuration);
        builder.Services.AddApplication();

        // Debug: Log JWT configuration
        var jwtSection = builder.Configuration.GetSection("JwtOptions");
        var jwtSecret = jwtSection["Secret"];
        var jwtIssuer = jwtSection["Issuer"];
        var jwtAudience = jwtSection["Audience"];

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Add SignalR
        builder.Services.AddSignalR(options =>
        {
            // Configure SignalR for CORS
            options.EnableDetailedErrors = builder.Environment.IsDevelopment();
        });
        builder.Services.AddScoped<INotificationService, NotificationService>();

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins("http://localhost:5173", "https://searchbugs.com")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.Title = "SearchBugs API";
                options.Theme = ScalarTheme.Alternate;
                options.ShowSidebar = true;
            });
        }

        // Enable CORS first
        app.UseCors("AllowAll");

        // Add authentication and authorization middleware after CORS
        app.UseAuthentication();
        app.UseAuthorization();

        // Add custom middleware after auth
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseStaticFiles();
        app.MapAuthenticationsEndpoints();
        app.MapBugsEndpoints();
        app.MapUserEndpoints();
        app.MapProfileEndpoints();
        app.MapRoleEndpoints();
        app.MapProjectsEndpoints();
        app.MapRepoEndpoints();
        app.MapNotificationEndpoints();
        app.MapTestNotificationEndpoints();
        app.MapAuditLogEndpoints();

        // Map new comprehensive endpoints
        app.MapRepositoryEndpoints();
        app.MapAnalyticsEndpoints();
        app.MapAdminEndpoints();

        // Map SignalR hub with CORS
        app.MapHub<NotificationHub>("/hubs/notifications");

        // app.UseHttpsRedirection();
        app.Run();
    }
}

public abstract partial class Program { }