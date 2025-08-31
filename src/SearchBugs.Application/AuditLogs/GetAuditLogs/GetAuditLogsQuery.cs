using Shared.Messaging;

namespace SearchBugs.Application.AuditLogs.GetAuditLogs;

public sealed record GetAuditLogsQuery(
    Guid? UserId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int PageNumber = 1,
    int PageSize = 50
) : IQuery<List<AuditLogDto>>;
