namespace SearchBugs.Infrastructure.Services;

/// <summary>
/// Service for managing permission cache invalidation
/// Use this service when roles or permissions are updated
/// </summary>
public interface IPermissionCacheManager
{
    /// <summary>
    /// Invalidates cache for a specific user
    /// </summary>
    Task InvalidateUserAsync(Guid userId);

    /// <summary>
    /// Invalidates cache for multiple users
    /// </summary>
    Task InvalidateUsersAsync(IEnumerable<Guid> userIds);

    /// <summary>
    /// Invalidates cache for all users (use sparingly)
    /// </summary>
    Task InvalidateAllAsync();

    /// <summary>
    /// Invalidates cache for users with a specific role
    /// </summary>
    Task InvalidateUsersInRoleAsync(string roleName);
}

public class PermissionCacheManager : IPermissionCacheManager
{
    private readonly IPermissionCacheService _cacheService;

    public PermissionCacheManager(IPermissionCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task InvalidateUserAsync(Guid userId)
    {
        await _cacheService.InvalidateUserPermissionsAsync(userId);
    }

    public async Task InvalidateUsersAsync(IEnumerable<Guid> userIds)
    {
        var tasks = userIds.Select(userId => _cacheService.InvalidateUserPermissionsAsync(userId));
        await Task.WhenAll(tasks);
    }

    public async Task InvalidateAllAsync()
    {
        await _cacheService.InvalidateAllPermissionsAsync();
    }

    public async Task InvalidateUsersInRoleAsync(string roleName)
    {
        // This would require additional database queries to find users in the role
        // For now, invalidate all permissions
        await _cacheService.InvalidateAllPermissionsAsync();
    }
}
