using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain.AuditLogs;
using SearchBugs.Domain.Repositories;
using SearchBugs.Domain.Users;

namespace SearchBugs.Persistence.Repositories;

internal sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .OrderByDescending(al => al.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userIdEntity = new UserId(userId);
        return await _context.AuditLogs
            .Where(al => al.UserId == userIdEntity)
            .OrderByDescending(al => al.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(al => al.CreatedOnUtc >= startDate && al.CreatedOnUtc <= endDate)
            .OrderByDescending(al => al.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }
}
