using Microsoft.AspNetCore.Cors.Infrastructure;

namespace SearchBugs.Api.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = new CorsSettings();
        configuration.GetSection(CorsSettings.SectionName).Bind(corsSettings);

        services.Configure<CorsSettings>(configuration.GetSection(CorsSettings.SectionName));

        services.AddCors(options =>
        {
            options.AddPolicy("ApiCorsPolicy", policy =>
            {
                ConfigureCorsPolicy(policy, corsSettings);
            });

            // Development policy for more permissive CORS during development
            options.AddPolicy("DevelopmentCorsPolicy", policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(30));
            });
        });

        return services;
    }

    private static void ConfigureCorsPolicy(CorsPolicyBuilder policy, CorsSettings settings)
    {
        // Configure origins - Cannot use AllowAnyOrigin with AllowCredentials
        if (settings.AllowAnyOrigin && !settings.AllowCredentials)
        {
            policy.AllowAnyOrigin();
        }
        else if (settings.AllowedOrigins.Any())
        {
            policy.WithOrigins(settings.AllowedOrigins.ToArray())
                  .SetIsOriginAllowedToAllowWildcardSubdomains();
        }
        else
        {
            // Fallback to specific origins if nothing is configured
            policy.WithOrigins("http://localhost:5173", "https://localhost:5173", "http://localhost:3000");
        }

        // Configure methods
        if (settings.AllowAnyMethod)
        {
            policy.AllowAnyMethod();
        }
        else if (settings.AllowedMethods.Any())
        {
            policy.WithMethods(settings.AllowedMethods.ToArray());
        }
        else
        {
            // Default methods for API
            policy.WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS");
        }

        // Configure headers
        if (settings.AllowAnyHeader)
        {
            policy.AllowAnyHeader();
        }
        else if (settings.AllowedHeaders.Any())
        {
            policy.WithHeaders(settings.AllowedHeaders.ToArray());
        }
        else
        {
            // Default headers
            policy.WithHeaders("Content-Type", "Authorization", "Accept", "X-Requested-With");
        }

        // Configure credentials - Only if not allowing any origin
        if (settings.AllowCredentials && !settings.AllowAnyOrigin)
        {
            policy.AllowCredentials();
        }

        // Configure exposed headers
        if (settings.ExposedHeaders.Any())
        {
            policy.WithExposedHeaders(settings.ExposedHeaders.ToArray());
        }

        // Configure preflight max age
        policy.SetPreflightMaxAge(settings.PreflightMaxAge);
    }

    public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            // Use more permissive CORS in development
            app.UseCors("DevelopmentCorsPolicy");
        }
        else
        {
            // Use configured CORS policy in production
            app.UseCors("ApiCorsPolicy");
        }

        return app;
    }
}
