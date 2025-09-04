namespace SearchBugs.Infrastructure.Services;

public interface IPermissionCacheService
{
    /// <summary>
    /// Gets permissions for a user from cache, or loads them if not cached
    /// </summary>
    Task<HashSet<string>> GetPermissionsAsync(Guid userId);

    /// <summary>
    /// Invalidates cached permissions for a specific user
    /// </summary>
    Task InvalidateUserPermissionsAsync(Guid userId);

    /// <summary>
    /// Invalidates all cached permissions (useful when roles/permissions are updated)
    /// </summary>
    Task InvalidateAllPermissionsAsync();

    /// <summary>
    /// Pre-loads permissions for a user into cache
    /// </summary>
    Task PreloadPermissionsAsync(Guid userId, HashSet<string> permissions);
}
