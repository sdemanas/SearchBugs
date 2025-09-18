using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain.Users;
using Shared.Primitives;
using Shared.Results;

namespace SearchBugs.Persistence.Repositories;

internal sealed class ProfileRepository : Repository<Profile, ProfileId>, IProfileRepository
{
    public ProfileRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Result<Profile>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var profile = await _context.Set<Profile>()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        return Result.Create(profile);
    }

    public async Task<Result<Profile>> CreateOrUpdateAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        var existingProfile = await _context.Set<Profile>()
            .FirstOrDefaultAsync(p => p.UserId == profile.UserId, cancellationToken);

        if (existingProfile != null)
        {
            _context.Set<Profile>().Update(profile);
        }
        else
        {
            await _context.Set<Profile>().AddAsync(profile, cancellationToken);
        }

        return Result.Success(profile);
    }

    public override async Task<Result<Profile>> GetByIdAsync(IEntityId id, CancellationToken cancellationToken = default, params Expression<Func<Profile, object>>[] includes)
    {
        IQueryable<Profile> query = _context.Set<Profile>().Include(p => p.User);

        if (includes.Length > 0)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        var profile = await query.FirstOrDefaultAsync(p => p.Id == (ProfileId)id, cancellationToken);
        return Result.Create(profile);
    }
}
