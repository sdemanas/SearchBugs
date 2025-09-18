using SearchBugs.Domain.Repositories;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.AuditLogs.GetAuditLogs;

internal sealed class GetAuditLogsQueryHandler : IQueryHandler<GetAuditLogsQuery, List<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<Result<List<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Domain.AuditLogs.AuditLog> auditLogs;

        if (request.UserId.HasValue)
        {
            auditLogs = await _auditLogRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken);
        }
        else if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            auditLogs = await _auditLogRepository.GetByDateRangeAsync(request.StartDate.Value, request.EndDate.Value, cancellationToken);
        }
        else
        {
            auditLogs = await _auditLogRepository.GetAllAsync(cancellationToken);
        }

        var pagedResults = auditLogs
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(AuditLogDto.FromAuditLog)
            .ToList();

        return Result.Success(pagedResults);
    }
}
