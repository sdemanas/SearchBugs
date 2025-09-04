# Permission Cache Service

This document explains how to use the permission cache service in the SearchBugs application.

## Overview

The permission cache service provides in-memory caching for user permissions to improve performance by reducing database queries. It's designed to be easily extensible to support Redis or other cache providers in the future.

## Architecture

The cache service follows a decorator pattern with these key components:

- `IPermissionCacheService` - Main cache interface
- `MemoryPermissionCacheService` - Memory-based cache implementation
- `RedisPermissionCacheService` - Redis-based cache implementation (template)
- `CachedPermissionService` - Decorator that wraps the original permission service
- `IPermissionCacheManager` - Helper service for cache invalidation

## Configuration

Add the following to your `appsettings.json`:

```json
{
  "PermissionCache": {
    "ExpiryMinutes": 30,
    "SlidingExpiryMinutes": 10,
    "Enabled": true,
    "Provider": "Memory"
  }
}
```

### Configuration Options

- `ExpiryMinutes`: Absolute expiration time (default: 30 minutes)
- `SlidingExpiryMinutes`: Sliding expiration time (default: 10 minutes)
- `Enabled`: Whether caching is enabled (default: true)
- `Provider`: Cache provider type - "Memory" or "Redis" (default: "Memory")
- `RedisConnectionString`: Redis connection string (when using Redis)

## Usage

### Basic Usage

The cache service is automatically registered and used transparently through the `IPermissionService` interface:

```csharp
public class MyController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public MyController(IPermissionService permissionService)
    {
        _permissionService = permissionService; // This is automatically cached
    }

    public async Task<IActionResult> GetPermissions(Guid userId)
    {
        var permissions = await _permissionService.GetPermissionsAsync(userId);
        return Ok(permissions);
    }
}
```

### Cache Management

Use `IPermissionCacheManager` when you need to invalidate cache:

```csharp
public class RoleController : ControllerBase
{
    private readonly IPermissionCacheManager _cacheManager;

    public RoleController(IPermissionCacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    [HttpPut("{roleId}/permissions")]
    public async Task<IActionResult> UpdateRolePermissions(Guid roleId, [FromBody] UpdatePermissionsRequest request)
    {
        // Update role permissions...

        // Invalidate cache for all users (since role permissions changed)
        await _cacheManager.InvalidateAllAsync();

        return Ok();
    }

    [HttpPut("users/{userId}/roles")]
    public async Task<IActionResult> UpdateUserRoles(Guid userId, [FromBody] UpdateRolesRequest request)
    {
        // Update user roles...

        // Invalidate cache for specific user
        await _cacheManager.InvalidateUserAsync(userId);

        return Ok();
    }
}
```

## Switching to Redis

When ready to use Redis, follow these steps:

1. Install required packages:

```bash
dotnet add package StackExchange.Redis
```

2. Complete the Redis implementation in `RedisPermissionCacheService.cs`

3. Update dependency injection:

```csharp
// In DependencyInjection.cs, replace:
services.AddMemoryPermissionCache();

// With:
services.AddRedisPermissionCache();
```

4. Update configuration:

```json
{
  "PermissionCache": {
    "Provider": "Redis",
    "RedisConnectionString": "localhost:6379"
  }
}
```

## Cache Invalidation Strategies

### When to Invalidate Cache

- **User role assignments change**: Invalidate specific user
- **Role permissions change**: Invalidate all users (or users in that role)
- **Permission definitions change**: Invalidate all users
- **User deleted**: Invalidate specific user

### Best Practices

1. **Selective Invalidation**: Prefer invalidating specific users over all users
2. **Batch Operations**: When updating multiple users, collect user IDs and invalidate in batch
3. **Background Jobs**: Consider using background jobs for large-scale invalidation operations
4. **Monitoring**: Monitor cache hit/miss ratios to tune expiration settings

## Performance Considerations

- Memory cache is suitable for small to medium applications
- For high-scale applications or multi-instance deployments, use Redis
- Monitor memory usage if caching many users
- Adjust expiration times based on your permission change frequency

## Extending the Cache Service

To add a new cache provider:

1. Create a class inheriting from `PermissionCacheServiceBase`
2. Implement abstract methods
3. Register in dependency injection
4. Add configuration options

Example:

```csharp
public class MyCustomCacheService : PermissionCacheServiceBase
{
    // Implement abstract methods...
}
```
