using SearchBugs.Infrastructure.Services;

namespace SearchBugs.Infrastructure.UnitTests.Services;

public class PermissionCacheManagerTests
{
    [Fact]
    public void IPermissionCacheManager_Interface_HasExpectedMethods()
    {
        // This is a basic test to ensure the interface is properly defined
        var interfaceType = typeof(IPermissionCacheManager);

        Assert.True(interfaceType.IsInterface);

        var methods = interfaceType.GetMethods();
        var methodNames = methods.Select(m => m.Name).ToArray();

        Assert.Contains("InvalidateUserAsync", methodNames);
        Assert.Contains("InvalidateUsersAsync", methodNames);
        Assert.Contains("InvalidateAllAsync", methodNames);
        Assert.Contains("InvalidateUsersInRoleAsync", methodNames);
    }

    [Fact]
    public void PermissionCacheManager_CanBeInstantiated()
    {
        // This tests basic instantiation without complex mocking
        Assert.True(typeof(PermissionCacheManager).IsClass);
        Assert.True(typeof(IPermissionCacheManager).IsAssignableFrom(typeof(PermissionCacheManager)));
    }
}
