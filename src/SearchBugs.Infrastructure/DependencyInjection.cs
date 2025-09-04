using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SearchBugs.Domain.Git;
using SearchBugs.Domain.Services;
using SearchBugs.Domain.Users;
using SearchBugs.Infrastructure.Authentication;
using SearchBugs.Infrastructure.Extensions;
using SearchBugs.Infrastructure.Options;
using SearchBugs.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace SearchBugs.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        //services.AddQuartz(options =>
        //{
        //    options.UseMicrosoftDependencyInjectionJobFactory();
        //});

        //services.AddQuartzHostedService(options =>
        //{
        //    options.WaitForJobsToComplete = true;
        //});
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.ConfigureOptions<LoggingBackgroundJobSetup>();

        // Configure options BEFORE adding authentication
        services.ConfigureOptions<JwtOptionsSetup>();
        services.ConfigureOptions<GitOptionsSetup>();
        services.ConfigureOptions<PermissionCacheOptionsSetup>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Get the JWT options from the service provider
                var serviceProvider = services.BuildServiceProvider();
                var jwtOptions = serviceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = "role",
                    ClockSkew = TimeSpan.Zero
                };

                options.MapInboundClaims = false;
            });
        services.AddTransient<IJwtProvider, JwtProvider>();
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IDataEncryptionService, DataEncryptionService>();
        services.AddScoped<IGitHttpService, GitHttpService>();
        services.AddScoped<IGitRepositoryService, GitRepositoryService>();

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        // Configure permission caching
        services.AddMemoryPermissionCache();
        services.AddCachedPermissionService();
        services.AddScoped<IPermissionCacheManager, PermissionCacheManager>();

    }
}
