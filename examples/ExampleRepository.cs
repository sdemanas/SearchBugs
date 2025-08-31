using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain.YourEntity; // Replace with your actual domain entity
using SearchBugs.Persistence;
using SearchBugs.Persistence.Repositories;
using Shared.Results;
using System.Linq.Expressions;

namespace SearchBugs.Examples;

/// <summary>
/// Example concrete repository implementation
/// </summary>
internal sealed class ExampleRepository : Repository<YourEntity, YourEntityId>, IYourEntityRepository
{
    public ExampleRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    // Override GetByIdAsync if you need custom includes or logic
    public override async Task<Result<YourEntity>> GetByIdAsync(
        IEntityId id,
        CancellationToken cancellationToken = default,
        params Expression<Func<YourEntity, object>>[] includes)
    {
        var query = _context.Set<YourEntity>().AsQueryable();

        // Apply includes if provided
        if (includes.Length > 0)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }

        // Add any default includes your entity needs
        query = query
            .Include(e => e.RelatedEntity1)
            .Include(e => e.RelatedEntity2);

        var entity = await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        return Result.Create(entity);
    }

    // Custom repository methods
    public async Task<Result<YourEntity>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<YourEntity>()
            .FirstOrDefaultAsync(e => e.Name == name, cancellationToken);

        return Result.Create(entity);
    }

    public async Task<Result<IEnumerable<YourEntity>>> GetByProjectIdAsync(
        ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.Set<YourEntity>()
            .Where(e => e.ProjectId == projectId)
            .Include(e => e.RelatedEntity)
            .ToListAsync(cancellationToken);

        return Result.Success(entities.AsEnumerable());
    }

    // Custom method with error handling
    public async Task<Result<bool>> IsNameUniqueAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _context.Set<YourEntity>()
                .AnyAsync(e => e.Name.ToLower() == name.ToLower(), cancellationToken);

            return Result.Success(!exists);
        }
        catch (Exception)
        {
            // Log the exception if needed
            return Result.Failure<bool>(YourEntityErrors.DatabaseError);
        }
    }
}
