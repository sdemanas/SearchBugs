using SearchBugs.Api.Endpoints;
using SearchBugs.Api.Middleware;
using SearchBugs.Api.Services;
using SearchBugs.Application;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Infrastructure;
using SearchBugs.Persistence;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddInfrastructure();
        builder.Services.AddPersistence(builder.Configuration);
        builder.Services.AddApplication();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

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

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            //app.ApplyMigrations();
        }

        // Enable CORS
        app.UseCors();

        app.MapAuthenticationsEndpoints();
        app.MapBugsEndpoints();
        app.MapUserEndpoints();
        app.MapProjectsEndpoints();
        app.MapRepoEndpoints();

        // app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseStaticFiles();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.Run();
    }
}

public partial class Program { }