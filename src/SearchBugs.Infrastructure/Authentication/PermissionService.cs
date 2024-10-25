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
        ICollection<Role>[] roles = (ICollection<Role>[])await _dbContext.Set<User>()
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Permissions)
            .Where(u => u.Id == new UserId(userId))
            .Select(u => u.Roles)
            .ToArrayAsync();

        return roles.SelectMany(r => r.SelectMany(r => r.Permissions.Select(p => p.Name))).ToHashSet();

    }
}
