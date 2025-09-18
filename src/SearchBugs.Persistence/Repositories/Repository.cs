using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain;
using Shared.Primitives;
using Shared.Results;

namespace SearchBugs.Persistence.Repositories;

public abstract class Repository<TEntity, TEntityId> : IRepository<TEntity, TEntityId>
    where TEntity : Entity<TEntityId>
    where TEntityId : class, IEntityId
{
    protected readonly ApplicationDbContext _context;

    protected Repository(ApplicationDbContext dbContext) => _context = dbContext;

    public Task<Result> Add(TEntity entity)
    {
        _context.Set<TEntity>().Add(entity);
        return Task.FromResult(Result.Success());
    }

    public async Task<Result> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
        return Result.Success();
    }

    public Task<Result> Update(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
        return Task.FromResult(Result.Success());
    }

    public Task<Result> Remove(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        return Task.FromResult(Result.Success());
    }

    public virtual async Task<Result<TEntity>> GetByIdAsync(IEntityId id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        if (includes.Length > 0)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }

        return Result.Create(await query.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken));
    }
}
