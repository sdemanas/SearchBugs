using Scalar.AspNetCore;
using SearchBugs.Api.Endpoints;
using SearchBugs.Api.Middleware;
using SearchBugs.Application;
using SearchBugs.Infrastructure;
using SearchBugs.Persistence;

public partial class Program
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

        // Add Cors
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        app.MapAuthenticationsEndpoints();
        app.MapBugsEndpoints();
        app.MapUserEndpoints();
        app.MapProjectsEndpoints();
        //app.MapRepoEndpoints();

        // app.UseHttpsRedirection();

        app.UseCors("AllowAll");


        app.UseAuthentication();
        app.UseStaticFiles();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.Run();
    }
}

public partial class Program { }