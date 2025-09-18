using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchBugs.Infrastructure.Authentication;
using SearchBugs.Infrastructure.Services;

namespace SearchBugs.Api.Controllers.Examples;

/// <summary>
/// Example controller demonstrating how to use the permission cache service
/// This is for documentation purposes only
/// </summary>
[ApiController]
[Route("api/examples/[controller]")]
public class PermissionCacheExampleController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly IPermissionCacheManager _cacheManager;

    public PermissionCacheExampleController(
        IPermissionService permissionService,
        IPermissionCacheManager cacheManager)
    {
        _permissionService = permissionService;
        _cacheManager = cacheManager;
    }

    /// <summary>
    /// Gets permissions for a user (automatically cached)
    /// </summary>
    [HttpGet("users/{userId}/permissions")]
    [Authorize]
    public async Task<IActionResult> GetUserPermissions(Guid userId)
    {
        try
        {
            // This call will automatically use caching
            var permissions = await _permissionService.GetPermissionsAsync(userId);

            return Ok(new
            {
                UserId = userId,
                Permissions = permissions,
                Cached = true // First call loads from DB, subsequent calls from cache
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error getting permissions: {ex.Message}");
        }
    }

    /// <summary>
    /// Invalidates cache for a specific user
    /// Call this when user roles change
    /// </summary>
    [HttpDelete("users/{userId}/cache")]
    [Authorize]
    public async Task<IActionResult> InvalidateUserCache(Guid userId)
    {
        try
        {
            await _cacheManager.InvalidateUserAsync(userId);

            return Ok(new
            {
                Message = $"Cache invalidated for user {userId}",
                UserId = userId
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error invalidating cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Invalidates cache for multiple users
    /// Call this when role permissions change affecting multiple users
    /// </summary>
    [HttpDelete("users/cache")]
    [Authorize]
    public async Task<IActionResult> InvalidateMultipleUsersCache([FromBody] List<Guid> userIds)
    {
        try
        {
            await _cacheManager.InvalidateUsersAsync(userIds);

            return Ok(new
            {
                Message = $"Cache invalidated for {userIds.Count} users",
                UserIds = userIds
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error invalidating cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Invalidates all permission cache
    /// Use sparingly - only when global permission changes occur
    /// </summary>
    [HttpDelete("cache/all")]
    [Authorize] // Should have admin role in real implementation
    public async Task<IActionResult> InvalidateAllCache()
    {
        try
        {
            await _cacheManager.InvalidateAllAsync();

            return Ok(new
            {
                Message = "All permission cache invalidated",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error invalidating all cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets cache configuration information
    /// </summary>
    [HttpGet("cache/info")]
    [Authorize]
    public IActionResult GetCacheInfo()
    {
        // This would typically come from configuration
        return Ok(new
        {
            Provider = "Memory",
            ExpiryMinutes = 30,
            SlidingExpiryMinutes = 10,
            Enabled = true,
            Message = "Permission caching is active. Permissions are cached for 30 minutes with sliding expiration of 10 minutes."
        });
    }
}
