using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SearchBugs.Infrastructure.Authentication;
using SearchBugs.Infrastructure.Options;

namespace SearchBugs.Infrastructure.Services;

public class MemoryPermissionCacheService : PermissionCacheServiceBase
{
    private readonly IMemoryCache _memoryCache;
    private readonly PermissionCacheOptions _cacheOptions;

    public MemoryPermissionCacheService(
        IMemoryCache memoryCache,
        PermissionService permissionService,
        IOptions<PermissionCacheOptions> cacheOptions,
        ILogger<MemoryPermissionCacheService> logger)
        : base(permissionService, logger)
    {
        _memoryCache = memoryCache;
        _cacheOptions = cacheOptions.Value;
    }

    public override async Task<HashSet<string>> GetPermissionsAsync(Guid userId)
    {
        var cacheKey = GetCacheKey(userId);

        if (_memoryCache.TryGetValue(cacheKey, out HashSet<string>? cachedPermissions))
        {
            Logger.LogDebug("Retrieved permissions for user {UserId} from cache", userId);
            return cachedPermissions ?? new HashSet<string>();
        }

        Logger.LogDebug("Permissions not found in cache for user {UserId}, loading from database", userId);

        var permissions = await LoadPermissionsFromSourceAsync(userId);

        // Cache the permissions with expiry
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheOptions.ExpiryMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(_cacheOptions.SlidingExpiryMinutes),
            Priority = CacheItemPriority.Normal
        };

        _memoryCache.Set(cacheKey, permissions, cacheOptions);

        Logger.LogDebug("Cached permissions for user {UserId} with {PermissionCount} permissions",
            userId, permissions.Count);

        return permissions;
    }

    public override Task InvalidateUserPermissionsAsync(Guid userId)
    {
        var cacheKey = GetCacheKey(userId);
        _memoryCache.Remove(cacheKey);

        Logger.LogDebug("Invalidated permissions cache for user {UserId}", userId);

        return Task.CompletedTask;
    }

    public override Task InvalidateAllPermissionsAsync()
    {
        // Since IMemoryCache doesn't provide a way to clear by prefix,
        // we'll need to implement a more sophisticated approach
        // For now, we can dispose and recreate the cache or track keys

        // Note: This is a simplified implementation. In production,
        // you might want to track cache keys or use a different approach
        Logger.LogWarning("InvalidateAllPermissionsAsync called - individual key removal recommended");

        return Task.CompletedTask;
    }

    public override Task PreloadPermissionsAsync(Guid userId, HashSet<string> permissions)
    {
        var cacheKey = GetCacheKey(userId);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheOptions.ExpiryMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(_cacheOptions.SlidingExpiryMinutes),
            Priority = CacheItemPriority.Normal
        };

        _memoryCache.Set(cacheKey, permissions, cacheOptions);

        Logger.LogDebug("Preloaded permissions for user {UserId} with {PermissionCount} permissions",
            userId, permissions.Count);

        return Task.CompletedTask;
    }
}
