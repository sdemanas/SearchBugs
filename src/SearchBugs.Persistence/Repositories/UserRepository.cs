using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Users;
using Shared.Primitives;
using Shared.Results;

namespace SearchBugs.Persistence.Repositories;

internal sealed class UserRepository : Repository<User, UserId>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Result<Role>> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken) =>
          Result.Create(await _context.Set<Role>().FirstOrDefaultAsync(role => role.Id == roleId, cancellationToken));

    public async Task<Result<User>> GetUserByEmailAsync(string email, CancellationToken cancellationToken) =>
          Result.Create(await _context.Set<User>()
              .Include(u => u.Roles)
              .FirstOrDefaultAsync(user => user.Email.Value == email, cancellationToken));

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken) =>
        await _context.Set<User>()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(user => user.Email.Value == email.Value, cancellationToken);

    public override async Task<Result<User>> GetByIdAsync(IEntityId id, CancellationToken cancellationToken = default, params Expression<Func<User, object>>[] includes)
    {
        // Always include roles when getting a user by ID
        var allIncludes = includes.Concat(new Expression<Func<User, object>>[] { u => u.Roles }).ToArray();
        return await base.GetByIdAsync(id, cancellationToken, allIncludes);
    }

    public async Task<Result<User>> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken) =>
        Result.Create(await _context.Set<User>().FirstOrDefaultAsync(user => user.Email.Value == email.Value, cancellationToken));
}
