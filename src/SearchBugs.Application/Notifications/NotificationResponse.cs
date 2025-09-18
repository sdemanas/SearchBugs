namespace SearchBugs.Application.Notifications;

public record NotificationResponse(
    string Id,
    string Type,
    string Message,
    string? Data,
    string? BugId,
    bool IsRead,
    DateTime CreatedAt);

public record PagedNotificationResponse(
    List<NotificationResponse> Notifications,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

public record UnreadCountResponse(int Count);
