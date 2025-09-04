using Microsoft.Extensions.DependencyInjection;
using SearchBugs.Infrastructure.Authentication;
using SearchBugs.Infrastructure.Services;

namespace SearchBugs.Infrastructure.Extensions;

public static class PermissionCacheServiceExtensions
{
    /// <summary>
    /// Adds memory-based permission caching
    /// </summary>
    public static IServiceCollection AddMemoryPermissionCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<IPermissionCacheService, MemoryPermissionCacheService>();
        return services;
    }

    /// <summary>
    /// Adds Redis-based permission caching (when implemented)
    /// </summary>
    public static IServiceCollection AddRedisPermissionCache(this IServiceCollection services)
    {
        // TODO: Add Redis configuration when implementing
        services.AddScoped<IPermissionCacheService, RedisPermissionCacheService>();
        return services;
    }

    /// <summary>
    /// Configures the permission service to use caching
    /// </summary>
    public static IServiceCollection AddCachedPermissionService(this IServiceCollection services)
    {
        // Register the original permission service as a concrete type
        services.AddScoped<PermissionService>();

        // Register the cached wrapper as the main interface
        services.AddScoped<IPermissionService, CachedPermissionService>();

        return services;
    }
}
