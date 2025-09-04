using Microsoft.Extensions.Logging;
using SearchBugs.Infrastructure.Authentication;

namespace SearchBugs.Infrastructure.Services;

public abstract class PermissionCacheServiceBase : IPermissionCacheService
{
    protected readonly PermissionService PermissionService;
    protected readonly ILogger Logger;

    protected PermissionCacheServiceBase(
        PermissionService permissionService,
        ILogger logger)
    {
        PermissionService = permissionService;
        Logger = logger;
    }

    public abstract Task<HashSet<string>> GetPermissionsAsync(Guid userId);
    public abstract Task InvalidateUserPermissionsAsync(Guid userId);
    public abstract Task InvalidateAllPermissionsAsync();
    public abstract Task PreloadPermissionsAsync(Guid userId, HashSet<string> permissions);

    protected virtual string GetCacheKey(Guid userId) => $"user_permissions:{userId}";

    protected virtual async Task<HashSet<string>> LoadPermissionsFromSourceAsync(Guid userId)
    {
        Logger.LogDebug("Loading permissions for user {UserId} from database", userId);
        var permissions = await PermissionService.GetPermissionsAsync(userId);
        Logger.LogDebug("Loaded {PermissionCount} permissions for user {UserId}", permissions.Count, userId);
        return permissions;
    }
}
