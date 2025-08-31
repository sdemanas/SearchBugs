using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain.Bugs;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Users;

namespace SearchBugs.Domain;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Role> Roles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
