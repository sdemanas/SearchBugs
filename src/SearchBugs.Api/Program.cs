using Scalar.AspNetCore;
using SearchBugs.Api.Endpoints;
using SearchBugs.Api.Hubs;
using SearchBugs.Api.Middleware;
using SearchBugs.Api.Services;
using SearchBugs.Application;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Infrastructure;
using SearchBugs.Persistence;

namespace SearchBugs.Api;

public abstract partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();

        builder.Services.AddInfrastructure();
        builder.Services.AddPersistence(builder.Configuration);
        builder.Services.AddApplication();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Add SignalR
        builder.Services.AddSignalR();
        builder.Services.AddScoped<INotificationService, NotificationService>();

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Add Cors for SignalR
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.WithOrigins("http://localhost:3000", "http://localhost:5173")
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

        // Enable CORS
        app.UseCors("AllowAll");

        // Add authentication and authorization middleware in correct order
        app.UseAuthentication();
        app.UseAuthorization();

        // Add custom middleware
        app.UseStaticFiles();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

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