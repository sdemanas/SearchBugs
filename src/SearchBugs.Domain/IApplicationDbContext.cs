using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain.Bugs;
using SearchBugs.Domain.Roles;

namespace SearchBugs.Domain;

public interface IApplicationDbContext
{
    DbSet<RolePermission> RolePermissions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
