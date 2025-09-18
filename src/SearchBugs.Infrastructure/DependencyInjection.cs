using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SearchBugs.Domain.Git;
using SearchBugs.Domain.Services;
using SearchBugs.Domain.Users;
using SearchBugs.Infrastructure.Authentication;
using SearchBugs.Infrastructure.Extensions;
using SearchBugs.Infrastructure.Options;
using SearchBugs.Infrastructure.Services;

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
        services.ConfigureOptions<JwtBearerOptionsSetup>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
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
