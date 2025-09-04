using SearchBugs.Infrastructure.Authentication;
using SearchBugs.Infrastructure.Services;

namespace SearchBugs.Infrastructure.Authentication;

/// <summary>
/// Cached permission service that decorates the original permission service with caching
/// </summary>
public class CachedPermissionService : IPermissionService
{
    private readonly IPermissionCacheService _cacheService;

    public CachedPermissionService(IPermissionCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<HashSet<string>> GetPermissionsAsync(Guid userId)
    {
        return await _cacheService.GetPermissionsAsync(userId);
    }
}
