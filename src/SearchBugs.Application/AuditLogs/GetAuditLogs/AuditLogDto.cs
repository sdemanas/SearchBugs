using SearchBugs.Domain.AuditLogs;

namespace SearchBugs.Application.AuditLogs.GetAuditLogs;

public sealed record AuditLogDto(
    Guid Id,
    string RequestName,
    string RequestData,
    string? ResponseData,
    bool IsSuccess,
    string? ErrorMessage,
    TimeSpan Duration,
    Guid? UserId,
    string? UserName,
    string IpAddress,
    string UserAgent,
    DateTime CreatedOnUtc
)
{
    public static AuditLogDto FromAuditLog(AuditLog auditLog) => new(
        auditLog.Id.Value,
        auditLog.RequestName,
        auditLog.RequestData,
        auditLog.ResponseData,
        auditLog.IsSuccess,
        auditLog.ErrorMessage,
        auditLog.Duration,
        auditLog.UserId?.Value,
        auditLog.UserName,
        auditLog.IpAddress,
        auditLog.UserAgent,
        auditLog.CreatedOnUtc
    );
}
