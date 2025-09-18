using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain.Bugs;
using Shared.Primitives;
using Shared.Results;

namespace SearchBugs.Persistence.Repositories;

internal sealed class BugRepository : Repository<Bug, BugId>, IBugRepository
{
    public BugRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task<Result<Bug>> GetByIdAsync(IEntityId id, CancellationToken cancellationToken = default, params Expression<Func<Bug, object>>[] includes) =>
          Result.Create(await _context.Set<Bug>()
              .Include(b => b.Status)
              .Include(b => b.Priority)
              .Include(b => b.Comments)
                  .ThenInclude(c => c.User)
              .Include(b => b.BugHistories)
                  .ThenInclude(h => h.User)
              .Include(b => b.BugCustomFields)
                  .ThenInclude(bcf => bcf.CustomField)
              .FirstOrDefaultAsync(b => b.Id == (BugId)id, cancellationToken));

    public async Task<Result<BugStatus>> GetBugStatusByName(string name, CancellationToken cancellationToken = default) =>
        Result.Create(await _context.Set<BugStatus>().FirstOrDefaultAsync(s => s.Name == name, cancellationToken));

    public async Task<Result<BugPriority>> GetBugPriorityByName(string name, CancellationToken cancellationToken) =>
        Result.Create(await _context.Set<BugPriority>().FirstOrDefaultAsync(p => p.Name == name, cancellationToken));
}
