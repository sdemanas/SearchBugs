using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Users;
using SearchBugs.Persistence;

namespace SearchBugs.Infrastructure.Authentication;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _dbContext;

    public PermissionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HashSet<string>> GetPermissionsAsync(Guid userId)
    {
        // Get user with their roles and permissions
        var user = await _dbContext.Users
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == new UserId(userId));

        if (user == null)
        {
            return new HashSet<string>();
        }

        // Get all permissions from user's roles
        var permissions = user.Roles
            .SelectMany(role => role.Permissions)
            .Select(permission => permission.Name)
            .ToHashSet();

        return permissions;
    }
}
