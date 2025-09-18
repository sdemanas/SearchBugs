using SearchBugs.Domain.Users;
using Shared.Primitives;

namespace SearchBugs.Domain.AuditLogs;

public sealed class AuditLog : Entity<AuditLogId>, IAuditable
{
    public string RequestName { get; private set; } = string.Empty;
    public string RequestData { get; private set; } = string.Empty;
    public string? ResponseData { get; private set; }
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public TimeSpan Duration { get; private set; }
    public UserId? UserId { get; private set; }
    public string? UserName { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? ModifiedOnUtc { get; private set; }

    private AuditLog(
        AuditLogId id,
        string requestName,
        string requestData,
        string? responseData,
        bool isSuccess,
        string? errorMessage,
        TimeSpan duration,
        UserId? userId,
        string? userName,
        string ipAddress,
        string userAgent,
        DateTime createdOnUtc) : base(id)
    {
        RequestName = requestName;
        RequestData = requestData;
        ResponseData = responseData;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Duration = duration;
        UserId = userId;
        UserName = userName;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedOnUtc = createdOnUtc;
    }

    private AuditLog() { }

    public static AuditLog Create(
        string requestName,
        string requestData,
        string? responseData,
        bool isSuccess,
        string? errorMessage,
        TimeSpan duration,
        UserId? userId,
        string? userName,
        string ipAddress,
        string userAgent,
        DateTime createdOnUtc)
    {
        return new AuditLog(
            new AuditLogId(Guid.NewGuid()),
            requestName,
            requestData,
            responseData,
            isSuccess,
            errorMessage,
            duration,
            userId,
            userName,
            ipAddress,
            userAgent,
            createdOnUtc);
    }
}
