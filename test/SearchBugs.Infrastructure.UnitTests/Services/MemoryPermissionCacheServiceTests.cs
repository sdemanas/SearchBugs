using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SearchBugs.Infrastructure.Options;
using Xunit;

namespace SearchBugs.Infrastructure.UnitTests.Services;

public class PermissionCacheOptionsTests
{
    [Fact]
    public void PermissionCacheOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new PermissionCacheOptions();

        // Assert
        Assert.Equal(30, options.ExpiryMinutes);
        Assert.Equal(10, options.SlidingExpiryMinutes);
        Assert.True(options.Enabled);
        Assert.Equal("Memory", options.Provider);
        Assert.Equal("PermissionCache", PermissionCacheOptions.ConfigurationSection);
    }

    [Fact]
    public void MemoryCache_CanStoreAndRetrievePermissions()
    {
        // Arrange
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var userId = Guid.NewGuid();
        var cacheKey = $"user_permissions:{userId}";
        var permissions = new HashSet<string> { "Permission1", "Permission2" };

        // Act
        memoryCache.Set(cacheKey, permissions);
        var retrieved = memoryCache.Get<HashSet<string>>(cacheKey);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(permissions, retrieved);
    }

    [Fact]
    public void MemoryCache_CanRemovePermissions()
    {
        // Arrange
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var userId = Guid.NewGuid();
        var cacheKey = $"user_permissions:{userId}";
        var permissions = new HashSet<string> { "Permission1", "Permission2" };

        // Act
        memoryCache.Set(cacheKey, permissions);
        memoryCache.Remove(cacheKey);
        var retrieved = memoryCache.Get<HashSet<string>>(cacheKey);

        // Assert
        Assert.Null(retrieved);
    }
}
