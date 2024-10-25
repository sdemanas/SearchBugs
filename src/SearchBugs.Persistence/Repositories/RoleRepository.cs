
using SearchBugs.Domain.Roles;
using Shared.Results;

namespace SearchBugs.Persistence.Repositories;

internal sealed class RoleRepository : IRoleRepository
{

    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Role>> GetByIdAsync(int roleId, CancellationToken cancellationToken)
    {
        return Result.Create(
            _context.Roles
            .Where(r => r.Id == roleId)
            .Select(r => new Role(r.Id, r.Name))
            .SingleOrDefault());
    }

    public async Task<Result<Role>> GetByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        return Result.Create(
            _context.Roles
            .Where(r => r.Name == roleName)
            .Select(r => new Role(r.Id, r.Name))
            .SingleOrDefault());
    }
}
